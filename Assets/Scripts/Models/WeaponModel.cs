// ================================================== //
// WEAPON DATA - Simple data class
// ================================================== //

using UnityEngine;

[System.Serializable]
public class WeaponModel
{
    public string weaponName;
    public WeaponType weaponType;
    public string description;
    
    public int damage;
    public float attackSpeed;
    public float range;
    public float projectileSpeed;
    public int manaCost;
    
    public GameObject prefab;
}

