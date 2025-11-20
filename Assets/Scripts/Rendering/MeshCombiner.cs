// MeshCombiner.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class for combining multiple meshes into single mesh objects for performance optimization.
/// Handles vertex limits and chunking automatically.
/// </summary>
public class MeshCombiner
{
    /// <summary>
    /// Creates a combined mesh from a list of positions, handling vertex limits automatically.
    /// </summary>
    public GameObject CreateCombinedMesh(List<Vector3> positions, string name, Transform parent)
    {
        if (positions == null || positions.Count == 0) 
        {
            Debug.LogWarning($"No positions provided for mesh: {name}");
            return null;
        }

        // Check if we need to split due to vertex limit
        const int maxVerticesPerMesh = 60000; // Conservative limit
        const int verticesPerCube = 24; // A cube has 24 vertices in Unity

        int maxCubesPerMesh = maxVerticesPerMesh / verticesPerCube;
        
        if (positions.Count > maxCubesPerMesh)
        {
            Debug.Log($"Splitting {name}: {positions.Count} cubes exceeds vertex limit, splitting into chunks");
            return CreateSplitMeshes(positions, name, parent, maxCubesPerMesh);
        }

        return CreateSingleCombinedMesh(positions, name, parent);
    }

    private GameObject CreateSingleCombinedMesh(List<Vector3> positions, string name, Transform parent)
    {
        // Create container object
        GameObject container = new(name);
        container.transform.SetParent(parent);
        
        // Create individual cubes as children (will be combined)
        var meshFilters = new List<MeshFilter>();
        
        foreach (var position in positions)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.SetParent(container.transform);
            
            var meshFilter = cube.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
                meshFilters.Add(meshFilter);
        }

        Debug.Log($"Created {meshFilters.Count} cubes for {name}");

        // Combine meshes if we have multiple
        if (meshFilters.Count > 1)
        {
            CombineMeshesInContainer(container);
        }
        else if (meshFilters.Count == 1)
        {
            // Handle single mesh case
            SetupSingleMeshContainer(container, meshFilters[0]);
        }
        else
        {
            Debug.LogWarning($"No valid meshes created for {name}");
            Object.DestroyImmediate(container);
            return null;
        }
        
        return container;
    }

    private void SetupSingleMeshContainer(GameObject container, MeshFilter meshFilter)
    {
        var containerFilter = container.GetComponent<MeshFilter>();
        if (containerFilter == null)
            containerFilter = container.AddComponent<MeshFilter>();
            
        MeshRenderer containerRenderer = container.GetComponent<MeshRenderer>();
        if (containerRenderer == null)
            containerRenderer = container.AddComponent<MeshRenderer>();
        
        containerFilter.mesh = meshFilter.sharedMesh;
        
        // Copy material from the cube
        var cubeRenderer = meshFilter.GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            containerRenderer.sharedMaterial = cubeRenderer.sharedMaterial;
        }
        
        // Destroy the child cube
        Object.DestroyImmediate(meshFilter.gameObject);
    }

    private GameObject CreateSplitMeshes(List<Vector3> positions, string name, Transform parent, int maxCubesPerChunk)
    {
        GameObject mainContainer = new(name);
        mainContainer.transform.SetParent(parent);
        
        int chunkCount = Mathf.CeilToInt((float)positions.Count / maxCubesPerChunk);
        int chunksCreated = 0;
        
        for (int i = 0; i < positions.Count; i += maxCubesPerChunk)
        {
            int chunkSize = Mathf.Min(maxCubesPerChunk, positions.Count - i);
            var chunkPositions = positions.GetRange(i, chunkSize);
            
            var chunk = CreateSingleCombinedMesh(chunkPositions, $"{name}_Chunk{chunksCreated}", mainContainer.transform);
            if (chunk != null)
            {
                chunksCreated++;
            }
        }
        
        Debug.Log($"Created {chunksCreated} chunks for {name}");
        return mainContainer;
    }

    private void CombineMeshesInContainer(GameObject container)
    {
        MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();
        
        if (meshFilters.Length == 0) return;

        List<CombineInstance> combineInstances = new();
        
        // Skip the first mesh filter (it's the container's own filter)
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh != null)
            {
                CombineInstance combineInstance = new();
                combineInstance.mesh = meshFilters[i].sharedMesh;
                combineInstance.transform = meshFilters[i].transform.localToWorldMatrix;
                combineInstances.Add(combineInstance);
            }
        }

        if (combineInstances.Count == 0) return;

        // Check if we'll exceed vertex limit and use chunking if needed
        int totalVertices = CalculateTotalVertices(combineInstances);
        
        if (totalVertices > 60000) // Safe margin below 65k
        {
            Debug.LogWarning($"Mesh too large ({totalVertices} vertices), using chunking...");
            CombineMeshesInChunks(container, combineInstances);
        }
        else
        {
            CombineAllMeshes(container, combineInstances);
        }
    }

    private void CombineAllMeshes(GameObject container, List<CombineInstance> combineInstances)
    {
        // Create combined mesh
        Mesh combinedMesh = new();
        
        // Set index format to support more vertices
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        combinedMesh.CombineMeshes(combineInstances.ToArray());
        
        // Set up the container with combined mesh
        SetupContainerWithMesh(container, combinedMesh);
        
        // Destroy individual child objects
        DestroyChildObjects(container);
    }

    private void SetupContainerWithMesh(GameObject container, Mesh combinedMesh)
    {
        MeshFilter containerFilter = container.GetComponent<MeshFilter>();
        if (containerFilter == null)
            containerFilter = container.AddComponent<MeshFilter>();
            
        MeshRenderer containerRenderer = container.GetComponent<MeshRenderer>();
        if (containerRenderer == null)
            containerRenderer = container.AddComponent<MeshRenderer>();
        
        containerFilter.mesh = combinedMesh;
        
        // Copy material from first child
        if (container.transform.childCount > 0)
        {
            var firstRenderer = container.transform.GetChild(0).GetComponent<Renderer>();
            if (firstRenderer != null)
            {
                containerRenderer.sharedMaterial = firstRenderer.sharedMaterial;
            }
        }
    }

    private void CombineMeshesInChunks(GameObject container, List<CombineInstance> combineInstances)
    {
        const int maxVerticesPerChunk = 60000; // Safe margin
        List<CombineInstance> currentChunk = new();
        int currentVertexCount = 0;
        int chunkIndex = 0;

        // Clear existing components from container
        ClearContainerComponents(container);

        for (int i = 0; i < combineInstances.Count; i++)
        {
            var combineInstance = combineInstances[i];
            int meshVertices = combineInstance.mesh.vertexCount;

            // If adding this mesh would exceed limit and we have items in current chunk, create chunk
            if (currentVertexCount + meshVertices > maxVerticesPerChunk && currentChunk.Count > 0)
            {
                CreateChunkMesh(container, currentChunk, chunkIndex);
                currentChunk.Clear();
                currentVertexCount = 0;
                chunkIndex++;
            }

            currentChunk.Add(combineInstance);
            currentVertexCount += meshVertices;
        }

        // Create final chunk
        if (currentChunk.Count > 0)
        {
            CreateChunkMesh(container, currentChunk, chunkIndex);
        }

        // Destroy individual child objects
        DestroyChildObjects(container);

        Debug.Log($"Created {chunkIndex + 1} mesh chunks to avoid vertex limit");
    }

    private void ClearContainerComponents(GameObject container)
    {
        var existingFilter = container.GetComponent<MeshFilter>();
        var existingRenderer = container.GetComponent<MeshRenderer>();
        if (existingFilter != null) Object.DestroyImmediate(existingFilter);
        if (existingRenderer != null) Object.DestroyImmediate(existingRenderer);
    }

    private void CreateChunkMesh(GameObject container, List<CombineInstance> combineInstances, int chunkIndex)
    {
        if (combineInstances.Count == 0) return;

        // Create chunk object
        GameObject chunk = new($"Chunk_{chunkIndex}");
        chunk.transform.SetParent(container.transform);
        chunk.transform.localPosition = Vector3.zero;

        // Create combined mesh for this chunk
        Mesh combinedMesh = new();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray());

        // Add components to chunk
        MeshFilter chunkFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer chunkRenderer = chunk.AddComponent<MeshRenderer>();
        
        chunkFilter.mesh = combinedMesh;

        // Copy material from first combine instance if possible
        CopyMaterialToChunk(container, chunkRenderer);
    }

    private void CopyMaterialToChunk(GameObject container, MeshRenderer chunkRenderer)
    {
        // Try to find a renderer in the original children to copy material from
        foreach (Transform child in container.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                chunkRenderer.sharedMaterial = renderer.sharedMaterial;
                break;
            }
        }
    }

    private void DestroyChildObjects(GameObject container)
    {
        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(container.transform.GetChild(i).gameObject);
        }
    }

    private int CalculateTotalVertices(List<CombineInstance> combineInstances)
    {
        int total = 0;
        foreach (var instance in combineInstances)
        {
            if (instance.mesh != null)
            {
                total += instance.mesh.vertexCount;
            }
        }
        return total;
    }
}