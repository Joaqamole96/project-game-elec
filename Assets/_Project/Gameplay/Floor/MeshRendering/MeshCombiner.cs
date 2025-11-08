using UnityEngine;
using System.Collections.Generic;

public class MeshCombiner
{
    public GameObject CreateCombinedMesh(List<Vector3> positions, string name, Transform parent)
    {
        if (positions.Count == 0) return null;

        // Create container object
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent);
        
        // Create individual cubes as children (will be combined)
        var meshFilters = new List<MeshFilter>();
        
        foreach (var position in positions)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.SetParent(container.transform);
            
            var meshFilter = cube.GetComponent<MeshFilter>();
            if (meshFilter != null)
                meshFilters.Add(meshFilter);
        }

        // Combine meshes if we have multiple
        if (meshFilters.Count > 1)
        {
            CombineMeshesInContainer(container);
        }
        
        return container;
    }

    private void CombineMeshesInContainer(GameObject container)
    {
        MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();
        
        if (meshFilters.Length == 0) return;

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        
        // Skip the first mesh filter (it's the container's own filter)
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh != null)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilters[i].sharedMesh;
                combineInstance.transform = meshFilters[i].transform.localToWorldMatrix;
                combineInstances.Add(combineInstance);
            }
        }

        if (combineInstances.Count == 0) return;

        // FIX: Check if we'll exceed vertex limit and use chunking if needed
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
        Mesh combinedMesh = new Mesh();
        
        // FIX: Set index format to support more vertices
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        combinedMesh.CombineMeshes(combineInstances.ToArray());
        
        // Set up the container with combined mesh
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
        
        // Destroy individual child objects
        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(container.transform.GetChild(i).gameObject);
        }
    }

    private void CombineMeshesInChunks(GameObject container, List<CombineInstance> combineInstances)
    {
        const int maxVerticesPerChunk = 60000; // Safe margin
        List<CombineInstance> currentChunk = new List<CombineInstance>();
        int currentVertexCount = 0;
        int chunkIndex = 0;

        // Clear existing components from container
        var existingFilter = container.GetComponent<MeshFilter>();
        var existingRenderer = container.GetComponent<MeshRenderer>();
        if (existingFilter != null) Object.DestroyImmediate(existingFilter);
        if (existingRenderer != null) Object.DestroyImmediate(existingRenderer);

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
        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(container.transform.GetChild(i).gameObject);
        }

        Debug.Log($"Created {chunkIndex + 1} mesh chunks to avoid vertex limit");
    }

    private void CreateChunkMesh(GameObject container, List<CombineInstance> combineInstances, int chunkIndex)
    {
        if (combineInstances.Count == 0) return;

        // Create chunk object
        GameObject chunk = new GameObject($"Chunk_{chunkIndex}");
        chunk.transform.SetParent(container.transform);
        chunk.transform.localPosition = Vector3.zero;

        // Create combined mesh for this chunk
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray());

        // Add components to chunk
        MeshFilter chunkFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer chunkRenderer = chunk.AddComponent<MeshRenderer>();
        
        chunkFilter.mesh = combinedMesh;

        // Copy material from first combine instance if possible
        if (container.transform.childCount > 0)
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