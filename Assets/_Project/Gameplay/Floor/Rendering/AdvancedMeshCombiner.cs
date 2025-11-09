// AdvancedMeshCombiner.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Advanced mesh combining system that handles large numbers of meshes efficiently.
/// Supports material-based grouping and automatic vertex limit management.
/// </summary>
[System.Serializable]
public class AdvancedMeshCombiner
{
    [System.Serializable]
    public class CombinedMeshData
    {
        public string Name;
        public List<CombineInstance> CombineInstances = new List<CombineInstance>();
        public Material Material;
        public int VertexCount => CalculateVertexCount();
        
        private int CalculateVertexCount()
        {
            int total = 0;
            foreach (var instance in CombineInstances)
            {
                if (instance.mesh != null)
                    total += instance.mesh.vertexCount;
            }
            return total;
        }
    }

    private Dictionary<Material, CombinedMeshData> _materialMeshes = new Dictionary<Material, CombinedMeshData>();
    private const int MAX_VERTICES_PER_MESH = 60000;

    public void AddMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
    {
        if (mesh == null || material == null) return;

        if (!_materialMeshes.ContainsKey(material))
        {
            _materialMeshes[material] = new CombinedMeshData
            {
                Name = $"Combined_{material.name}",
                Material = material
            };
        }

        var combineInstance = new CombineInstance
        {
            mesh = mesh,
            transform = Matrix4x4.TRS(position, rotation, scale)
        };

        _materialMeshes[material].CombineInstances.Add(combineInstance);
    }

    public List<GameObject> BuildAllCombinedMeshes(Transform parent)
    {
        List<GameObject> combinedObjects = new List<GameObject>();
        
        foreach (var kvp in _materialMeshes)
        {
            var meshData = kvp.Value;
            
            if (meshData.CombineInstances.Count == 0) continue;

            // Split if too large
            if (meshData.VertexCount > MAX_VERTICES_PER_MESH)
            {
                var splitMeshes = BuildSplitMeshes(meshData, parent);
                combinedObjects.AddRange(splitMeshes);
            }
            else
            {
                var combinedObject = BuildSingleCombinedMesh(meshData, parent);
                if (combinedObject != null) combinedObjects.Add(combinedObject);
            }
        }

        _materialMeshes.Clear();
        return combinedObjects;
    }

    private GameObject BuildSingleCombinedMesh(CombinedMeshData meshData, Transform parent)
    {
        try
        {
            GameObject combinedObject = new GameObject(meshData.Name);
            combinedObject.transform.SetParent(parent);

            MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMesh.CombineMeshes(meshData.CombineInstances.ToArray());
            
            meshFilter.mesh = combinedMesh;
            meshRenderer.sharedMaterial = meshData.Material;

            // Add optimized collider
            var meshCollider = combinedObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = combinedMesh;

            Debug.Log($"Created combined mesh: {meshData.Name} with {meshData.CombineInstances.Count} instances, {combinedMesh.vertexCount} vertices");
            return combinedObject;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create combined mesh {meshData.Name}: {e.Message}");
            return null;
        }
    }

    private List<GameObject> BuildSplitMeshes(CombinedMeshData meshData, Transform parent, int maxInstancesPerChunk = 2000)
    {
        List<GameObject> chunks = new List<GameObject>();
        int instanceCount = meshData.CombineInstances.Count;
        int chunkCount = Mathf.CeilToInt((float)instanceCount / maxInstancesPerChunk);

        for (int i = 0; i < chunkCount; i++)
        {
            int startIndex = i * maxInstancesPerChunk;
            int count = Mathf.Min(maxInstancesPerChunk, instanceCount - startIndex);
            
            var chunkData = new CombinedMeshData
            {
                Name = $"{meshData.Name}_Chunk{i+1}",
                Material = meshData.Material
            };
            
            chunkData.CombineInstances.AddRange(meshData.CombineInstances.GetRange(startIndex, count));
            
            var chunkObject = BuildSingleCombinedMesh(chunkData, parent);
            if (chunkObject != null) chunks.Add(chunkObject);
        }

        Debug.Log($"Split {meshData.Name} into {chunks.Count} chunks");
        return chunks;
    }
}