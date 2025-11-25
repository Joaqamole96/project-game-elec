// -------------------------------------------------- //
// Scripts/Configs/BiomeRegistry.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeRegistry", menuName = "Configs/BiomeRegistry")]
[System.Serializable]
public class BiomeRegistry : ScriptableObject
{
    public List<BiomeModel> Biomes = new()
    {
        new BiomeModel("Grasslands", 1, 5),
        new BiomeModel("Dungeons", 6, 10),
        new BiomeModel("Caves", 11, 15),
    };
}