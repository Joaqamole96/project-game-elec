// -------------------- //
// Scripts/Configs/LevelConfig.cs
// -------------------- //

using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    [Header("Seed & Progression")]
    public int Seed = 0;
    [Range(1, 100)] public int LevelNumber = 1;
    
    [Header("Floor Growth")]
    [Range(5, 50)] public int Growth = 20;

    // -------------------------------------------------- //

    public void Validate()
    {
        Seed = Mathf.Max(0, Seed);
        LevelNumber = Mathf.Clamp(LevelNumber, 1, 100);
        Growth = Mathf.Clamp(Growth, 5, 50);

        Debug.Log("LevelConfig.Validate(): Validated.");
    }

    public LevelConfig Clone()
    {
        Debug.Log("LevelConfig.Clone(): Cloned.");
        return new() 
        { 
            Seed = Seed, 
            LevelNumber = LevelNumber, 
            Growth = Growth,
        };
    }
    
    /// <summary>
    /// Calculates floor width based on level progression.
    /// Uses BASE_FLOOR_SIZE + (Growth × Level), clamped to safe bounds.
    /// </summary>
    public int GetFloorWidth()
    {
        int calculatedWidth = LevelModel.BASE_FLOOR_SIZE + (Growth * LevelNumber);
        return Mathf.Clamp(calculatedWidth, LevelModel.BASE_FLOOR_SIZE, LevelModel.MAX_FLOOR_SIZE);
    }
    
    /// <summary>
    /// Calculates floor height based on level progression.
    /// Uses BASE_FLOOR_SIZE + (Growth × Level), clamped to safe bounds.
    /// </summary>
    public int GetFloorHeight()
    {
        int calculatedHeight = LevelModel.BASE_FLOOR_SIZE + (Growth * LevelNumber);
        return Mathf.Clamp(calculatedHeight, LevelModel.BASE_FLOOR_SIZE, LevelModel.MAX_FLOOR_SIZE);
    }
}