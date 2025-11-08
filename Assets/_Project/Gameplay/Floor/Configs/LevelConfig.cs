using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    [Header("Floor Configuration")]
    public int Seed = 0;
    public int FloorLevel = 1;
    public int Width = 150;
    public int Height = 150;
    public int FloorGrowth = 20;
    public int MinFloorSize = 100;
    public int MaxFloorSize = 1000;
}