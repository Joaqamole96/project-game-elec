// ================================================== //
// WEAPON DATA - Simple data class
// ================================================== //

using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public WeaponType weaponType;
    public string description;
    
    [Header("Stats")]
    public int damage;
    public float attackSpeed;
    public float range;
    public float projectileSpeed;
    public int manaCost;
    
    [Header("Visual")]
    public GameObject prefab;
}

public enum WeaponType
{
    Melee,      // Sword - fast swing, normal damage
    Charge,     // Axe - slow swing, high damage, larger area
    Ranged,     // Bow - projectile, medium damage
    Magic       // Staff - projectile, high damage, slow
}