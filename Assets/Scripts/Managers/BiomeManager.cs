// -------------------------------------------------- //
// Scripts/Managers/BiomeManager.cs (UPDATED)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class BiomeManager : MonoBehaviour
{

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
}