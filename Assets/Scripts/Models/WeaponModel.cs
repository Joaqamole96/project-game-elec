// ================================================== //
// Scripts/Models/WeaponModel.cs (UPDATED)
// ================================================== //

using System.Collections;
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

    private Coroutine attackResetCoroutine;
    
    protected float lastAttackTime = 0f;
    protected bool isEquipped = false;
    protected bool isAttacking = false;

    // public bool CanAttack => Time.time >= lastAttackTime + attackCooldown && !isAttacking;
    public bool CanAttack()
    {
        Debug.Log($"Time: {Time.time}");
        Debug.Log($"Cooldown: {lastAttackTime + attackCooldown}");
        Debug.Log($"Able to attack: {Time.time >= lastAttackTime + attackCooldown}");
        Debug.Log($"Is attacking: {isAttacking}");
        return Time.time >= lastAttackTime + attackCooldown && !isAttacking;
    }

    protected virtual void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Start auto-reset coroutine
        if (attackResetCoroutine != null)
            StopCoroutine(attackResetCoroutine);
        attackResetCoroutine = StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState()
    {
        // Auto-reset after reasonable time (attackCooldown + buffer)
        yield return new WaitForSeconds(attackCooldown + 0.5f);
        
        if (isAttacking)
        {
            Debug.LogWarning("Auto-resetting attack state (animation event may have failed)");
            isAttacking = false;
        }
        
        attackResetCoroutine = null;
    }

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
        Debug.Log($"Attack registered on {lastAttackTime}");
        lastAttackTime = Time.time;
    }

    public virtual void OnAttackComplete()
    {
        isAttacking = false;
        Debug.Log($"Attack completed, isAttacking set to false");
    }

    public void CompleteAttack()
    {
        isAttacking = false;
        
        if (attackResetCoroutine != null)
        {
            StopCoroutine(attackResetCoroutine);
            attackResetCoroutine = null;
        }
    }

    void OnDisable()
    {
        if (attackResetCoroutine != null)
        {
            StopCoroutine(attackResetCoroutine);
            attackResetCoroutine = null;
        }
        isAttacking = false;
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