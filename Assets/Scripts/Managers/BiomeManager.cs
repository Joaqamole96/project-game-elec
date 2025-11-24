// -------------------------------------------------- //
// Scripts/Managers/BiomeManager.cs (UPDATED)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class BiomeManager : MonoBehaviour
{
    [System.Serializable]
    public class BiomeData
    {
        public string biomeName;
        public int startLevel;
        public int endLevel;
        
        public BiomeData(string name, int start, int end)
        {
            biomeName = name;
            startLevel = start;
            endLevel = end;
        }
    }
    
    // Biome configuration - Define which biomes are available at which floors
    public List<BiomeData> availableBiomes = new()
    {
        new BiomeData(ResourceService.BIOME_DEFAULT, 1, 100),
        new BiomeData(ResourceService.BIOME_GRASSLANDS, 5, 20),
        new BiomeData(ResourceService.BIOME_DUNGEON, 15, 40),
        new BiomeData(ResourceService.BIOME_CAVES, 30, 100)
    };
    
    private System.Random _random;
    private string _currentBiome = ResourceService.BIOME_DEFAULT;
    
    public string CurrentBiome => _currentBiome;

    // ------------------------- //

    public void InitializeRandom(int seed) 
    {
        _random = new System.Random(seed);
    }

    public string GetBiomeForFloor(int floorLevel)
    {
        var validBiomes = availableBiomes.FindAll(biome => 
            floorLevel >= biome.startLevel && floorLevel <= biome.endLevel
        );
        
        if (validBiomes.Count == 0)
        {
            _currentBiome = ResourceService.BIOME_DEFAULT;
            return _currentBiome;
        }

        if (validBiomes.Count == 1)
        {
            _currentBiome = validBiomes[0].biomeName;
            return _currentBiome;
        }

        _currentBiome = validBiomes[_random.Next(0, validBiomes.Count)].biomeName;
        return _currentBiome;
    }
    
    // ------------------------- //
    // LAYOUT PREFABS
    // ------------------------- //

    public GameObject GetFloorPrefab(string biome = null)
        => ResourceService.LoadFloorPrefab(biome ?? _currentBiome);
    
    public GameObject GetWallPrefab(string biome = null)
        => ResourceService.LoadWallPrefab(biome ?? _currentBiome);
    
    public GameObject GetDoorPrefab(string biome = null)
        => ResourceService.LoadDoorPrefab(biome ?? _currentBiome);
    
    public GameObject GetDoorTopPrefab(string biome = null)
        => ResourceService.LoadDoorTopPrefab(biome ?? _currentBiome);
    
    public GameObject GetCeilingPrefab(string biome = null)
        => ResourceService.LoadCeilingPrefab(biome ?? _currentBiome);
    
    // ------------------------- //
    // PROP PREFABS
    // ------------------------- //
    
    public GameObject GetProp(string propName, string biome = null)
        => ResourceService.LoadProp(biome ?? _currentBiome, propName);
    
    public GameObject GetTorchPrefab(string biome = null)
        => ResourceService.LoadTorchPrefab(biome ?? _currentBiome);
    
    public GameObject GetPillarPrefab(string biome = null)
        => ResourceService.LoadPillarPrefab(biome ?? _currentBiome);
    
    public GameObject GetBarrelPrefab(string biome = null)
        => ResourceService.LoadBarrelPrefab(biome ?? _currentBiome);
    
    public GameObject GetCratePrefab(string biome = null)
        => ResourceService.LoadCratePrefab(biome ?? _currentBiome);
    
    // ------------------------- //
    // ENEMY PREFABS
    // ------------------------- //
    
    public GameObject GetEnemy(string enemyName, string biome = null)
        => ResourceService.LoadEnemy(biome ?? _currentBiome, enemyName);
    
    public GameObject GetBasicEnemyPrefab(string biome = null)
        => ResourceService.LoadBasicEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetEliteEnemyPrefab(string biome = null)
        => ResourceService.LoadEliteEnemyPrefab(biome ?? _currentBiome);
    
    public GameObject GetBossEnemyPrefab(string biome = null)
        => ResourceService.LoadBossEnemyPrefab(biome ?? _currentBiome);
    
    // ------------------------- //
    // LANDMARK PREFABS (Biome-independent)
    // ------------------------- //

    public GameObject GetLandmarkPrefab(RoomType roomType)
        => ResourceService.LoadLandmarkPrefab(roomType);
    
    // ------------------------- //
    // DEPRECATED (For backwards compatibility)
    // ------------------------- //
    
    [System.Obsolete("Use GetLandmarkPrefab(RoomType.XXX) instead")]
    public GameObject GetSpecialRoomPrefab(RoomType roomType)
        => GetLandmarkPrefab(roomType);
    
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