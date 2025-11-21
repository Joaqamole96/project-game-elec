// -------------------- //
// Scripts/Core/BiomeManager.cs
// -------------------- //

using UnityEngine;
using System;
using System.Collections.Generic;

public class BiomeManager
{
    public List<BiomeModel> Biomes = new();

    // NOTE: Use LevelConfig.Seed instead for deterministic biome selection.
    // private System.Random _random = new();
    private System.Random _random;
    private Dictionary<string, GameObject> _prefabCache;

    // -------------------------------------------------- //

    public BiomeManager(LevelConfig levelConfig)
    {
        if (levelConfig == null)
            throw new Exception("BiomeManager(): LevelConfig cannot be null.");

        _random = new System.Random(levelConfig.Seed);
        InitializeBiomes();
    }

    private void InitializeBiomes()
    {
        // Don't initialize a Default Biome; Default is for testing only.
        // Biomes.Add(new BiomeModel("Default", 1, 5));
        // NOTE: Provisionary biomes; may change in the future.
        Biomes.Add(new BiomeModel("Grasslands", 1, 5));
        Biomes.Add(new BiomeModel("Catacombs", 6, 10));
        Biomes.Add(new BiomeModel("Pits", 11, 15));
    }

    // -------------------------------------------------- //

    public BiomeModel GetBiomeForFloor(int floorLevel)
    {
        var candidateBiomes = Biomes.FindAll(biome => (floorLevel >= biome.MinLevel) && (floorLevel <= biome.MaxLevel));

        if (candidateBiomes.Count == 0)
            throw new Exception($"BiomeManager.GetBiomeForFloor(): No candidate biome found for floor {floorLevel}.");

        int selectedBiomeIndex = _random.Next(0, candidateBiomes.Count - 1);
        var selectedBiome = candidateBiomes[selectedBiomeIndex];

        Debug.Log($"BiomeManager.GetBiomeForFloor(): Returning {selectedBiome.Name}...");
        return selectedBiome;
    }

    // -------------------------------------------------- //

    public GameObject GetPrefab(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
            throw new Exception("BiomeManager.GetPrefab(): Empty prefab path provided!");
        
        if (_prefabCache.TryGetValue(prefabPath, out GameObject cachedPrefab))
        {
            Debug.Log($"BiomeManager.GetPrefab(): Returning {cachedPrefab.name}...");
            return cachedPrefab;
        }
        else
            return GetAndCachePrefab(prefabPath);
    }

    public GameObject GetFloorPrefab(BiomeModel biome)
        => GetPrefab($"Biomes/{biome.Name}/FloorPrefab");

    public GameObject GetWallPrefab(BiomeModel biome)
        => GetPrefab($"Biomes/{biome.Name}/WallPrefab");

    public GameObject GetDoorPrefab(BiomeModel biome)
        => GetPrefab($"Biomes/{biome.Name}/DoorPrefab");

    public GameObject GetDoorTopPrefab(BiomeModel biome) 
        => GetPrefab($"Biomes/{biome.Name}/DoorTopPrefab");

    private GameObject GetAndCachePrefab(string prefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
            throw new Exception($"BiomeManager.GetAndCachePrefab(): Prefab not found at path: {prefabPath}");

        _prefabCache[prefabPath] = prefab;
        Debug.Log($"BiomeManager.GetAndCachePrefab(): Cached {prefab.name} successfully.");

        Debug.Log($"BiomeManager.GetAndCachePrefab(): Returning {prefab.name}...");
        return prefab;
    }

    // -------------------------------------------------- //

    // Biomes are randomly selected; no need for weight.
    // private float CalculateTotalWeight(List<BiomeModel> biomes)
    // {
    //     float total = 0f;
    //     foreach (var biome in biomes)
    //         total += biome.Weight;
    //     return total;
    // }

    // private BiomeModel SelectBiomeByWeight(List<BiomeModel> biomes, float randomValue)
    // {
    //     float currentWeight = 0f;
    //     foreach (var biome in biomes)
    //     {
    //         // currentWeight += biome.Weight;
    //         if (randomValue <= currentWeight)
    //             return biome;
    //     }
    //     return biomes[0];
    // }

    

    private void LogAvailableResources(string failedPath)
    {
        var availableFloors = Resources.LoadAll<GameObject>("Biomes/Default");
        Debug.Log($"Available resources in Biomes/Default: {availableFloors.Length}");
        foreach (var resource in availableFloors)
            Debug.Log($" - {resource.name}");
    }
}