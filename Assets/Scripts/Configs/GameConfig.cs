// -------------------- //
// Scripts/Configs/GameConfig.cs
// -------------------- //

using UnityEngine;

[System.Serializable]
public class GameConfig
{
    [Header("Configurations")]
    public bool SimplifyGeometry = true;

    // -------------------------------------------------- //

    public void Validate() { Debug.Log("GameConfig.Validate(): Validated successfully."); }
    public GameConfig Clone()
    {
        Debug.Log("GameConfig.Clone(): Cloning...");
        return new()
        {
            SimplifyGeometry = SimplifyGeometry,
        };
    }
}