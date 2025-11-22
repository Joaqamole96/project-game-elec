// -------------------------------------------------- //
// Scripts/Configs/GameConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[System.Serializable]
public class PlayerConfig
{
    [Range(1, 100)] public int PlayerBaseHealth = 100;

    [Range(1f, 10f)] public float PlayerMovementSpeed = 5f;

    public PlayerConfig Clone() => this;

    public void Validate()
    {
        PlayerBaseHealth = Mathf.Clamp(PlayerBaseHealth, 1, 100);
        PlayerMovementSpeed = Mathf.Clamp(PlayerMovementSpeed, 1f, 10f);
    }
}