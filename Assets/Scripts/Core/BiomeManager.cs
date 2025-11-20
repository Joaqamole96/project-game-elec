// BiomeManager.cs
using UnityEngine;
using System.Collections.Generic;

public class BiomeManager
{
    private System.Random _random = new();
    public List<BiomeModel> Biomes { get; private set; } = new List<BiomeModel>();

    private readonly Dictionary<string, GameObject> _prefabCache = new();

    public BiomeManager()
    {
        InitializeDefaultBiomes();
    }

    private void InitializeDefaultBiomes()
    {
        Biomes.Add(new BiomeModel("Default", 1, 100, 1.0f));
    }

    public BiomeModel GetBiomeForFloor(int floorLevel, int seed)
    {
        _random = new System.Random(seed + floorLevel);
        var validBiomes = Biomes.FindAll(biome => floorLevel >= biome.MinLevel && floorLevel <= biome.MaxLevel);
        if (validBiomes.Count == 0)
        {
            Debug.LogWarning($"No biome found for floor {floorLevel}, using default");
            return Biomes[0];
        }
        if (validBiomes.Count == 1)
            return validBiomes[0];
        float totalWeight = CalculateTotalWeight(validBiomes);
        float randomValue = (float)_random.NextDouble() * totalWeight;
        return SelectBiomeByWeight(validBiomes, randomValue);
    }

    private float CalculateTotalWeight(List<BiomeModel> biomes)
    {
        float total = 0f;
        foreach (var biome in biomes)
            total += biome.Weight;
        return total;
    }

    private BiomeModel SelectBiomeByWeight(List<BiomeModel> biomes, float randomValue)
    {
        float currentWeight = 0f;
        foreach (var biome in biomes)
        {
            currentWeight += biome.Weight;
            if (randomValue <= currentWeight)
                return biome;
        }
        return biomes[0];
    }

    public GameObject GetPrefab(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning("Empty prefab path provided!");
            return null;
        }
        if (_prefabCache.TryGetValue(prefabPath, out GameObject cachedPrefab))
            return cachedPrefab;
        return LoadAndCachePrefab(prefabPath);
    }

    private GameObject LoadAndCachePrefab(string prefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab != null)
            _prefabCache[prefabPath] = prefab;
        else
        {
            Debug.LogError($"Prefab not found at path: {prefabPath}");
            LogAvailableResources(prefabPath);
        }
        return prefab;
    }

    private void LogAvailableResources(string failedPath)
    {
        var availableFloors = Resources.LoadAll<GameObject>("Biomes/Default");
        Debug.Log($"Available resources in Biomes/Default: {availableFloors.Length}");
        foreach (var resource in availableFloors)
            Debug.Log($" - {resource.name}");
    }

    public GameObject GetSpecialRoomPrefab(RoomType roomType, BiomeModel biome)
    {
        if (biome == null) 
            return null;
        string path = GetSpecialRoomPath(roomType, biome);
        return GetPrefab(path);
    }

    private string GetSpecialRoomPath(RoomType roomType, BiomeModel biome)
    {
        return roomType switch
        {
            RoomType.Entrance => $"Biome/{biome.Name}/EntrancePrefab";
            RoomType.Exit => biome.ExitPrefabPath,
            RoomType.Shop => biome.ShopPrefabPath,
            RoomType.Treasure => biome.TreasurePrefabPath,
            _ => null
        };
    }

    // Helper methods with fallback support
    public GameObject GetFloorPrefab(BiomeModel biome) => 
        GetPrefab(biome?.FloorPrefabPath) ?? GetPrefab("Biomes/Default/FloorPrefab");

    public GameObject GetWallPrefab(BiomeModel biome) => 
        GetPrefab(biome?.WallPrefabPath) ?? GetPrefab("Biomes/Default/WallPrefab");

    public GameObject GetDoorPrefab(BiomeModel biome) => 
        GetPrefab(biome?.DoorPrefabPath) ?? GetPrefab("Biomes/Default/DoorPrefab");

    public GameObject GetDoorTopPrefab(BiomeModel biome) => 
        GetPrefab(biome?.DoorTopPrefabPath) ?? GetPrefab("Biomes/Default/DoorTopPrefab");
}