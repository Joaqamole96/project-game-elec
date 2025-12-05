// -------------------------------------------------- //
// Scripts/Managers/BiomeManager.cs (UPDATED)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class BiomeManager : MonoBehaviour
{
    // [System.Serializable]
    // public class BiomeData
    // {
    //     public string biomeName;
    //     public int startLevel;
    //     public int endLevel;
        
    //     public BiomeData(string name, int start, int end)
    //     {
    //         biomeName = name;
    //         startLevel = start;
    //         endLevel = end;
    //     }
    // }
    
    // Biome configuration - Define which biomes are available at which floors
    // public List<BiomeData> availableBiomes = new()
    // {
    //     // new BiomeData(ResourceService.BIOME_DEFAULT, 1, 100),
    //     new BiomeData(ResourceService.BIOME_GRASSLANDS, 1, 5),
    //     new BiomeData(ResourceService.BIOME_DUNGEON, 6, 10),
    //     new BiomeData(ResourceService.BIOME_CAVES, 11, 15),
    // };

    public List<BiomeModel> availableBiomes = BiomeRegistry.Biomes;
    
    private System.Random _random;
    private string _currentBiome;
    
    public string CurrentBiome => _currentBiome;

    // ------------------------- //

    public void InitializeRandom(int seed) 
    {
        _random = new System.Random(seed);
    }

    public string GetBiomeForFloor(int floorLevel)
    {
        var validBiomes = availableBiomes.FindAll(biome => 
            floorLevel >= biome.StartLevel && floorLevel <= biome.EndLevel
        );
        
        if (validBiomes.Count == 0)
        {
            Debug.LogError($"No valid biomes found for floor level {floorLevel}.");
        }

        if (validBiomes.Count == 1)
        {
            _currentBiome = validBiomes[0].Name;
            return _currentBiome;
        }

        _currentBiome = validBiomes[_random.Next(0, validBiomes.Count)].Name;
        return _currentBiome;
    }
    
    // ------------------------- //
    // LAYOUT PREFABS
    // ------------------------- //

    public GameObject GetFloorPrefab(string biome = null)
        => ResourceService.LoadFloorPrefab();
    
    public GameObject GetWallPrefab(string biome = null)
        => ResourceService.LoadWallPrefab();
    
    public GameObject GetDoorPrefab(string biome = null)
        => ResourceService.LoadDoorPrefab();
    
    public GameObject GetCeilingPrefab(string biome = null)
        => ResourceService.LoadCeilingPrefab();
    
    // ------------------------- //
    // ENEMY PREFABS
    // ------------------------- //
    
    
    // public GameObject GetBasicEnemyPrefab(string biome = null)
    //     => ResourceService.LoadBasicEnemyPrefab(biome ?? _currentBiome);
    
    // public GameObject GetEliteEnemyPrefab(string biome = null)
    //     => ResourceService.LoadEliteEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetBossEnemyPrefab(string biome = null)
        => ResourceService.LoadBossEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetMeleeEnemyPrefab(string biome = null)
        => ResourceService.LoadMeleeEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetRangedEnemyPrefab(string biome = null)
        => ResourceService.LoadRangedEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetTankEnemyPrefab(string biome = null)
        => ResourceService.LoadTankEnemyPrefab(biome ?? _currentBiome);
    
    // ------------------------- //
    // LANDMARK PREFABS (Biome-independent)
    // ------------------------- //
    
    // ------------------------- //
    // DEPRECATED (For backwards compatibility)
    // ------------------------- //
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    public void PreloadCurrentBiome()
    {
        ResourceService.PreloadBiome(_currentBiome);
    }
    
    public void ClearBiomeCache()
    {
        ResourceService.ClearBiomeCache(_currentBiome);
    }
}