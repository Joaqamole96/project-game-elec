// -------------------------------------------------- //
// Scripts/Configs/BiomeRegistry.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public static class BiomeRegistry
{
    public static List<BiomeModel> Biomes = new()
    {
        new BiomeModel("Grasslands", 1, 5),
        new BiomeModel("Dungeons", 6, 10),
        new BiomeModel("Caves", 11, 15),
    };
}