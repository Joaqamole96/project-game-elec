// -------------------------------------------------- //
// Scripts/Models/WeaponModel.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Base class for all weapons
/// </summary>
public abstract class WeaponModel : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName = "Weapon";
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    public float attackRange = 2f;
    public LayerMask targetLayer;
    
    [Header("State")]
    protected float lastAttackTime = 0f;
    protected bool isEquipped = false;
    
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;
    
    public virtual void Equip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
        Debug.Log($"Equipped: {weaponName}");
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

// -------------------------------------------------- //
// Scripts/Models/MeleeWeapon.cs
// -------------------------------------------------- //

/// <summary>
/// Melee weapon (sword, axe, etc.)
/// </summary>
public class MeleeWeapon : WeaponModel
{
    [Header("Melee Settings")]
    public float swingAngle = 60f;
    public GameObject slashEffectPrefab;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        RegisterAttack();
        
        // Spawn visual effect
        if (slashEffectPrefab != null)
        {
            GameObject slash = Instantiate(slashEffectPrefab, attackPosition, Quaternion.LookRotation(attackDirection));
            Destroy(slash, 0.5f);
        }
        
        // Attack in cone
        Vector3 attackCenter = attackPosition + attackDirection * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, targetLayer);
        
        foreach (Collider hit in hits)
        {
            // Check if in swing cone
            Vector3 dirToTarget = (hit.transform.position - attackPosition).normalized;
            float angleToTarget = Vector3.Angle(attackDirection, dirToTarget);
            
            if (angleToTarget <= swingAngle / 2f)
            {
                DamageTarget(hit.gameObject);
            }
        }
    }
    
    protected virtual void DamageTarget(GameObject target)
    {
        if (target.TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.TakeDamage(baseDamage);
            Debug.Log($"{weaponName} hit {target.name} for {baseDamage} damage");
        }
    }
}

// -------------------------------------------------- //
// Scripts/Models/RangedWeapon.cs
// -------------------------------------------------- //

/// <summary>
/// Ranged weapon (bow, gun, etc.)
/// </summary>
public class RangedWeapon : WeaponModel
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public Transform firePoint;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        RegisterAttack();
        
        // Spawn projectile
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : attackPosition;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(attackDirection));
            
            // Setup projectile
            if (projectile.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = attackDirection * projectileSpeed;
            }
            
            if (projectile.TryGetComponent<Projectile>(out var proj))
            {
                proj.damage = baseDamage;
                proj.targetLayer = targetLayer;
            }
            
            Destroy(projectile, 5f);
        }
    }
}

// -------------------------------------------------- //
// Scripts/Models/Projectile.cs
// -------------------------------------------------- //

/// <summary>
/// Projectile fired by ranged weapons
/// </summary>
public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayer;
    public GameObject hitEffectPrefab;
    
    private bool hasHit = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Check if hit valid target
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Hit wall or obstacle
        else if (!other.isTrigger)
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }
    
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
}