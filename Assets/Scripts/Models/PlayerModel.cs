// PlayerModel.cs
using UnityEngine;

public class PlayerModel : IEntity
{
    public int MaxHealth = 100;
    public int Health { get; set; } = 100;
    public int Damage = 20;
    public float Range = 2f;
    public float AttackCooldown = 1f;
    public Vector2 Position;
    public float MovementSpeed = 10f;
    public float JumpHeight = 5f;
    public WeaponModel Weapon;
    public bool IsAlive { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public PlayerModel() { }

    public void Die()
    {
        IsAlive = false;
        IsActive = false;
    }

    public void Equip(WeaponModel weapon)
    {
        Weapon = weapon;
        UpdateStats(/*Stats*/);
    }

    public void UpdateStats(/*Stats*/) { }
}

