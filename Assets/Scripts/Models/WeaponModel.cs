// ================================================== //
// Scripts/Models/WeaponModel.cs (UPDATED)
// ================================================== //

using UnityEngine;

/// <summary>
/// Base weapon class with animator integration
/// </summary>
public abstract class WeaponModel : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName = "Weapon";
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    public float attackRange = 2f;
    public LayerMask targetLayer;
    
    [Header("Animation")]
    public Animator animator;
    public RuntimeAnimatorController animatorController;
    
    protected float lastAttackTime = 0f;
    protected bool isEquipped = false;
    protected bool isAttacking = false;
    
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown && !isAttacking;
    
    public virtual void Equip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    
    public virtual void Unequip()
    {
        isEquipped = false;
        gameObject.SetActive(false);
    }
    
    public abstract void Attack(Vector3 attackPosition, Vector3 attackDirection);
    
    protected void RegisterAttack()
    {
        lastAttackTime = Time.time;
    }
}











// ================================================== //
// WEAPON DATA - Configuration
// ================================================== //

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
    
    [Header("Prefab")]
    public GameObject prefab;
    
    [Header("Animation")]
    public RuntimeAnimatorController animatorController;
}

public enum WeaponType
{
    Melee,      // Sword - combo system
    Charge,     // Axe - hold to charge
    Ranged,     // Bow - draw and release
    Magic,      // Staff - spell types
    Rapid       // Daggers - fast attacks
}