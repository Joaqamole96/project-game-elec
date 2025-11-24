// -------------------------------------------------- //
// Scripts/Configs/BiomeConfig.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeConfig", menuName = "Configs/Biome Config")]
[System.Serializable]
public class BiomeConfig : ScriptableObject
{
    public List<BiomeModel> Biomes = new()
    {
        new BiomeModel("_Default", 1, 100)
    };
}