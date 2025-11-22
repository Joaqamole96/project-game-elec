// -------------------------------------------------- //
// Scripts/Managers/BiomeManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class BiomeManager
{
    public List<BiomeModel> Biomes = new();
    public Dictionary<string, GameObject> _prefabCache = new();

    private readonly System.Random _random;

    // ------------------------- //

    public BiomeManager(int seed)
    {
        _random = new(seed);
        InitializeDefaultBiomes();
    }

    private void InitializeDefaultBiomes()
    {
        Biomes.Add(new BiomeModel("Default", 1, 100, 1.0f));
    }

    public BiomeModel GetBiomeForFloor(int floorLevel)
    {
        var validBiomes = Biomes
            .FindAll(biome => (floorLevel >= biome.StartLevel) && (floorLevel <= biome.EndLevel));
        
        if (validBiomes.Count == 0) return Biomes[0];

        if (validBiomes.Count == 1) return validBiomes[0];

        // Weighted random selection
        float totalWeight = CalculateTotalWeight(validBiomes);
        float randomValue = (float)_random.NextDouble() * totalWeight;
        
        return SelectBiomeByWeight(validBiomes, randomValue);
    }

    private float CalculateTotalWeight(List<BiomeModel> biomes)
    {
        float total = 0f;

        foreach (var biome in biomes) total += biome.Weight;

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
        if (string.IsNullOrEmpty(prefabPath)) throw new("Empty prefab path provided!");

        if (_prefabCache.TryGetValue(prefabPath, out GameObject cachedPrefab)) return cachedPrefab;

        return LoadAndCachePrefab(prefabPath);
    }

    private GameObject LoadAndCachePrefab(string prefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        
        if (prefab != null) _prefabCache[prefabPath] = prefab;
        else throw new($"Prefab not found at path: {prefabPath}");

        return prefab;
    }

    // private void LogAvailableResources(string failedPath)
    // {
    //     var availableFloors = Resources.LoadAll<GameObject>("Biomes/Default");
    //     Debug.Log($"Available resources in Biomes/Default: {availableFloors.Length}");
    //     foreach (var resource in availableFloors)
    //     {
    //         Debug.Log($" - {resource.name}");
    //     }
    // }

    public GameObject GetSpecialRoomPrefab(RoomType roomType)
    {
        string path = GetSpecialRoomPath(roomType);
        return GetPrefab(path);
    }

    private string GetSpecialRoomPath(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Entrance => "Landmarks/EntrancePrefab",
            RoomType.Exit => "Landmarks/ExitPrefab",
            RoomType.Shop => "Landmarks/ShopPrefab",
            RoomType.Treasure => "Landmarks/TreasurePrefab",
            _ => null
        };
    }

    public GameObject GetFloorPrefab(BiomeModel biome) => GetPrefab(biome.FloorPrefabPath);

    public GameObject GetWallPrefab(BiomeModel biome) => GetPrefab(biome.WallPrefabPath);

    public GameObject GetDoorPrefab(BiomeModel biome) => GetPrefab(biome.DoorPrefabPath);

    public GameObject GetDoorTopPrefab(BiomeModel biome) => GetPrefab(biome.DoorTopPrefabPath);
}