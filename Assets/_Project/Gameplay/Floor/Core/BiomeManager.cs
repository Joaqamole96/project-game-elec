// Core/BiomeManager.cs
using UnityEngine;
using System.Collections.Generic;

public class BiomeManager
{
    [System.Serializable]
    public class BiomeSettings
    {
        public string Name;
        public int StartLevel;
        public int EndLevel;
        public GameObject[] FloorVariations;
        public GameObject[] WallVariations;
        public GameObject[] DoorVariations;
        public GameObject EntrancePrefab;
        public GameObject ExitPrefab;
        public GameObject ShopPrefab;
        public GameObject TreasurePrefab;
        public GameObject BossPrefab;
    }

    public List<BiomeSettings> Biomes = new List<BiomeSettings>();
    private System.Random _random;

    public BiomeManager()
    {
        _random = new System.Random();
        InitializeDefaultBiomes();
    }

    // In BiomeManager.cs - update the InitializeDefaultBiomes method
    private void InitializeDefaultBiomes()
    {
        // Grasslands Biome (Levels 1-5)
        Biomes.Add(new BiomeSettings
        {
            Name = "Grasslands",
            StartLevel = 1,
            EndLevel = 5,
            FloorVariations = LoadPrefabs("Floors/Grasslands"),
            WallVariations = LoadPrefabs("Walls/Grasslands"), 
            DoorVariations = LoadPrefabs("Doors/Grasslands"),
            EntrancePrefab = Resources.Load<GameObject>("Special/Entrance_Prefab"),
            ExitPrefab = Resources.Load<GameObject>("Special/Exit_Prefab"),
            ShopPrefab = Resources.Load<GameObject>("Special/Shop_Tent"),
            TreasurePrefab = Resources.Load<GameObject>("Special/Treasure_Chest")
        });
    }

    private GameObject[] LoadPrefabs(string resourcePath)
    {
        // Load all prefabs from Resources folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcePath);
        
        // Log for debugging
        if (prefabs.Length == 0)
        {
            Debug.LogWarning($"No prefabs found at path: {resourcePath}");
        }
        else
        {
            Debug.Log($"Loaded {prefabs.Length} prefabs from: {resourcePath}");
        }
        
        return prefabs;
    }

    public GameObject GetFloorPrefab(RoomType roomType, Vector2Int position)
    {
        var biome = GetCurrentBiome();
        if (biome?.FloorVariations == null || biome.FloorVariations.Length == 0)
            return null;

        // Use position for deterministic variation selection
        int variationIndex = Mathf.Abs(position.x * 31 + position.y * 17) % biome.FloorVariations.Length;
        return biome.FloorVariations[variationIndex];
    }

    public GameObject GetWallPrefab(WallType wallType, Vector2Int position)
    {
        var biome = GetCurrentBiome();
        if (biome?.WallVariations == null || biome.WallVariations.Length == 0)
            return null;

        // You could have different variation sets for different wall types
        // For now, we'll use the same variations but you can expand this
        int variationIndex = Mathf.Abs(position.x * 23 + position.y * 29) % biome.WallVariations.Length;
        
        // Optional: Filter or select different prefabs based on wall type
        GameObject selectedPrefab = biome.WallVariations[variationIndex];
        
        // You could add logic here like:
        // if (wallType == WallType.Corridor) return GetCorridorWallPrefab(position);
        // if (wallType.ToString().Contains("Corner")) return GetCornerWallPrefab(position);
        
        return selectedPrefab;
    }
    public GameObject GetDoorPrefab(Vector2Int position)
    {
        var biome = GetCurrentBiome();
        if (biome?.DoorVariations == null || biome.DoorVariations.Length == 0)
            return null;

        int variationIndex = Mathf.Abs(position.x * 37 + position.y * 41) % biome.DoorVariations.Length;
        return biome.DoorVariations[variationIndex];
    }

    public GameObject GetSpecialRoomPrefab(RoomType roomType)
    {
        var biome = GetCurrentBiome();
        return roomType switch
        {
            RoomType.Entrance => biome?.EntrancePrefab,
            RoomType.Exit => biome?.ExitPrefab,
            RoomType.Shop => biome?.ShopPrefab,
            RoomType.Treasure => biome?.TreasurePrefab,
            RoomType.Boss => biome?.BossPrefab,
            _ => null
        };
    }

    private BiomeSettings GetCurrentBiome()
    {
        // In real implementation, get current floor level from game state
        int currentLevel = 1; // This should come from your game state
        return Biomes.Find(b => currentLevel >= b.StartLevel && currentLevel <= b.EndLevel);
    }
}