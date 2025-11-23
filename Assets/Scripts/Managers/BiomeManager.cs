// -------------------------------------------------- //
// Scripts/Managers/BiomeManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class BiomeManager : MonoBehaviour
{
    // NOTE: Find a way to make populated Biome list appear in Inspector.
    // Also use a BiomeConfig class instead of a List<> here.
    public List<BiomeModel> Biomes = new()
    {
        new BiomeModel("Default", 1, 100),
    };

    public Dictionary<string, GameObject> _prefabCache = new();

    private System.Random _random;

    // ------------------------- //

    public void InitializeRandom(int seed) 
    {
        _random = new(seed);
    }

    public BiomeModel GetBiomeForFloor(int floorLevel)
    {
        var validBiomes = Biomes
            .FindAll(biome => (floorLevel >= biome.StartLevel) && (floorLevel <= biome.EndLevel));
        
        if (validBiomes.Count == 0) return Biomes[0];

        if (validBiomes.Count == 1) return validBiomes[0];

        return validBiomes[_random.Next(0, validBiomes.Count)];
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