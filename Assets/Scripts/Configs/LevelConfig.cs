// -------------------------------------------------- //
// Scripts/Configs/LevelConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
[System.Serializable]
public class LevelConfig : ScriptableObject
{
    public int Seed = 0;

    [Range(1, 100)] public int FloorLevel = 1;

    [Range(50, 1000)] public int Width = 100;

    [Range(50, 1000)] public int Height = 100;

    [Range(5, 50)] public int FloorGrowth = 10;

    [Range(50, 200)] public int MinFloorSize = 100;

    [Range(200, 2000)] public int MaxFloorSize = 1000;

    // ------------------------- //

    public void Validate()
    {
        Seed = Mathf.Max(0, Seed);
        FloorLevel = Mathf.Clamp(FloorLevel, 1, 100);
        Width = Mathf.Clamp(Width, 50, 1000);
        Height = Mathf.Clamp(Height, 50, 1000);
        FloorGrowth = Mathf.Clamp(FloorGrowth, 5, 50);
        MinFloorSize = Mathf.Clamp(MinFloorSize, 50, 200);
        MaxFloorSize = Mathf.Clamp(MaxFloorSize, 200, 2000);

        if (MinFloorSize >= MaxFloorSize) MinFloorSize = MaxFloorSize - 50;
    }

    public void GrowFloor(bool growWidth)
    {
        if (growWidth) Width = Mathf.Min(Width + FloorGrowth, MaxFloorSize);
        else Height = Mathf.Min(Height + FloorGrowth, MaxFloorSize);
    }
}