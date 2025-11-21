// DecorRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders decorative non-functional objects: trees, rocks, barrels, etc.
/// Uses mesh combining for performance where appropriate.
/// Throws errors for missing assets - no fallbacks.
/// </summary>
public class DecorRenderer
{
    private BiomeManager _biomeManager;
    private MeshCombiner _meshCombiner;

    public DecorRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        _meshCombiner = new MeshCombiner();
    }

    /// <summary>
    /// Renders all decor objects for the level.
    /// </summary>
    public void RenderAllDecor(LevelModel layout, List<RoomModel> rooms, BiomeModel biome, Transform parent)
    {
        if (layout == null) throw new System.ArgumentNullException(nameof(layout));
        if (biome == null) throw new System.ArgumentNullException(nameof(biome));
        if (parent == null) throw new System.ArgumentNullException(nameof(parent));

        Debug.Log($"Starting decor rendering for biome: {biome.Name}");

        // Render room-specific decor
        if (rooms != null)
        {
            foreach (var room in rooms)
            {
                RenderRoomDecor(room, biome, parent);
            }
        }

        // Render corridor decor
        RenderCorridorDecor(layout, biome, parent);

        // Finalize combined meshes
        _meshCombiner.FinalizeRendering(parent);

        Debug.Log("Decor rendering completed");
    }

    /// <summary>
    /// Renders decorative objects for a specific room.
    /// </summary>
    public void RenderRoomDecor(RoomModel room, BiomeModel biome, Transform parent)
    {
        if (room == null) return;

        var decorConfig = _biomeManager.GetRoomDecorConfig(biome, room.Type);
        if (decorConfig == null) return;

        int decorCount = CalculateRoomDecorCount(room, decorConfig);
        
        for (int i = 0; i < decorCount; i++)
        {
            var decorType = GetRandomDecorType(decorConfig);
            var decorPrefab = _biomeManager.GetDecorPrefab(biome, decorType);
            
            if (decorPrefab == null)
                throw new System.MissingReferenceException($"Decor prefab not found for type: {decorType} in biome: {biome.Name}");

            Vector3 position = CalculateRoomDecorPosition(room, decorPrefab, i);
            Quaternion rotation = CalculateDecorRotation(decorType);

            if (decorConfig.UseMeshCombining)
            {
                // Add to mesh combiner for batch rendering
                AddDecorToCombiner(decorPrefab, position, rotation);
            }
            else
            {
                // Instantiate as individual object
                CreateIndividualDecor(decorPrefab, position, rotation, parent, $"{decorType}_Room{room.ID}_{i}");
            }
        }
    }

    /// <summary>
    /// Renders decorative objects in corridors.
    /// </summary>
    public void RenderCorridorDecor(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.AllFloorTiles == null) return;

        var corridorDecorConfig = _biomeManager.GetCorridorDecorConfig(biome);
        if (corridorDecorConfig == null) return;

        // Sample corridor positions for decor placement
        var corridorPositions = SampleCorridorPositions(layout, corridorDecorConfig.Density);

        foreach (var position in corridorPositions)
        {
            var decorType = GetRandomDecorType(corridorDecorConfig);
            var decorPrefab = _biomeManager.GetDecorPrefab(biome, decorType);
            
            if (decorPrefab == null)
                throw new System.MissingReferenceException($"Corridor decor prefab not found for type: {decorType} in biome: {biome.Name}");

            Vector3 worldPos = new Vector3(position.x + 0.5f, 0f, position.y + 0.5f);
            Quaternion rotation = CalculateDecorRotation(decorType);

            if (corridorDecorConfig.UseMeshCombining)
            {
                AddDecorToCombiner(decorPrefab, worldPos, rotation);
            }
            else
            {
                CreateIndividualDecor(decorPrefab, worldPos, rotation, parent, $"{decorType}_Corridor_{position.x}_{position.y}");
            }
        }
    }

    #region Helper Methods

    private int CalculateRoomDecorCount(RoomModel room, DecorConfig config)
    {
        int roomArea = room.Bounds.width * room.Bounds.height;
        float density = Random.Range(config.MinDensity, config.MaxDensity);
        return Mathf.RoundToInt(roomArea * density);
    }

    private List<Vector2Int> SampleCorridorPositions(LevelModel layout, float density)
    {
        var positions = new List<Vector2Int>();
        
        if (layout.AllFloorTiles == null) return positions;

        // Only use corridor tiles (not in rooms)
        foreach (var floorPos in layout.AllFloorTiles)
        {
            if (layout.GetRoomAtPosition(floorPos) == null && Random.value < density)
            {
                positions.Add(floorPos);
            }
        }

        return positions;
    }

    private string GetRandomDecorType(DecorConfig config)
    {
        if (config.PrefabWeights == null || config.PrefabWeights.Count == 0)
            return "Default";

        // Simple weighted random selection
        float totalWeight = 0f;
        foreach (var weight in config.PrefabWeights.Values)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var kvp in config.PrefabWeights)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                return kvp.Key;
            }
        }

        return "Default";
    }

    private Vector3 CalculateRoomDecorPosition(RoomModel room, GameObject decorPrefab, int index)
    {
        // Calculate position within room bounds, avoiding walls and other objects
        var bounds = room.Bounds;
        
        // Leave 1-unit border from walls
        int minX = bounds.xMin + 1;
        int maxX = bounds.xMax - 1;
        int minY = bounds.yMin + 1;
        int maxY = bounds.yMax - 1;

        if (minX >= maxX || minY >= maxY) 
            return new Vector3(room.Center.x, 0f, room.Center.y);

        // Use index to create deterministic but varied positions
        float x = minX + (index % (maxX - minX)) + 0.5f;
        float z = minY + ((index * 7) % (maxY - minY)) + 0.5f; // Prime number for better distribution

        // Add small random offset for natural look
        x += Random.Range(-0.2f, 0.2f);
        z += Random.Range(-0.2f, 0.2f);

        return new Vector3(x, 0f, z);
    }

    private Quaternion CalculateDecorRotation(string decorType)
    {
        // Some decor types might have specific rotation logic
        // For example, trees might rotate randomly, while walls might have fixed orientation
        if (decorType.Contains("Tree") || decorType.Contains("Rock"))
        {
            return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
        
        return Quaternion.identity;
    }

    private void AddDecorToCombiner(GameObject decorPrefab, Vector3 position, Quaternion rotation)
    {
        Mesh decorMesh = GetPrefabMesh(decorPrefab);
        Material decorMaterial = GetPrefabMaterial(decorPrefab);
        Vector3 decorScale = decorPrefab.transform.localScale;

        if (decorMesh != null && decorMaterial != null)
        {
            _meshCombiner.AddMesh(decorMesh, position, rotation, decorScale, decorMaterial);
        }
    }

    private void CreateIndividualDecor(GameObject decorPrefab, Vector3 position, Quaternion rotation, Transform parent, string name)
    {
        var decor = GameObject.Instantiate(decorPrefab, position, rotation, parent);
        decor.name = name;

        // Ensure decor has collider if it's meant to be physical
        if (decor.GetComponent<Collider>() == null)
        {
            decor.AddComponent<BoxCollider>();
        }
    }

    private Mesh GetPrefabMesh(GameObject prefab)
    {
        if (prefab == null) return null;
        var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
        return meshFilter != null ? meshFilter.sharedMesh : null;
    }

    private Material GetPrefabMaterial(GameObject prefab)
    {
        if (prefab == null) return null;
        var renderer = prefab.GetComponentInChildren<Renderer>();
        return renderer != null ? renderer.sharedMaterial : null;
    }

    #endregion
}

// Configuration for decor placement
[System.Serializable]
public class DecorConfig
{
    public float MinDensity = 0.1f;
    public float MaxDensity = 0.3f;
    public bool UseMeshCombining = true;
    public Dictionary<string, float> PrefabWeights = new Dictionary<string, float>();
}