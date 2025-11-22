// -------------------- //
// Scripts/Configs/LevelConfig.cs
// -------------------- //

using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    [Header("Configurations")]
    public int Seed = 0;
    [Range(1, 100)] public int LevelNumber = 1;
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
}