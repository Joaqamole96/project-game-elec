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
        // new BiomeData(ResourceService.BIOME_DEFAULT, 1, 100),
        new BiomeData(ResourceService.BIOME_GRASSLANDS, 1, 5),
        new BiomeData(ResourceService.BIOME_DUNGEON, 6, 10),
        new BiomeData(ResourceService.BIOME_CAVES, 11, 15),
    };
    
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
            floorLevel >= biome.startLevel && floorLevel <= biome.endLevel
        );
        
        if (validBiomes.Count == 0) Debug.LogError("No valid biome found.");

        if (validBiomes.Count == 1)
        {
            _currentBiome = validBiomes[0].biomeName;
            return _currentBiome;
        }

        _currentBiome = validBiomes[_random.Next(0, validBiomes.Count)].biomeName;
        return _currentBiome;
    }
}