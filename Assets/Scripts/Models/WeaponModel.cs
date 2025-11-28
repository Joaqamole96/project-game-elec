// -------------------------------------------------- //
// Scripts/Models/WeaponModel.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Base class for all weapons in the game
/// Provides common functionality for weapon behavior and state management
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
    
    /// <summary>
    /// Checks if the weapon can currently attack based on cooldown
    /// </summary>
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;
    
    /// <summary>
    /// Equips the weapon and activates its GameObject
    /// </summary>
    public virtual void Equip()
    {
        try
        {
            isEquipped = true;
            gameObject.SetActive(true);
            Debug.Log($"WeaponModel: Equipped {weaponName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WeaponModel: Error equipping weapon {weaponName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Unequips the weapon and deactivates its GameObject
    /// </summary>
    public virtual void Unequip()
    {
        try
        {
            isEquipped = false;
            gameObject.SetActive(false);
            Debug.Log($"WeaponModel: Unequipped {weaponName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WeaponModel: Error unequipping weapon {weaponName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Performs attack at specified position and direction
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="attackPosition">World position to attack from</param>
    /// <param name="attackDirection">Direction of the attack</param>
    public abstract void Attack(Vector3 attackPosition, Vector3 attackDirection);
    
    /// <summary>
    /// Registers that an attack has occurred and updates cooldown timer
    /// </summary>
    protected void RegisterAttack()
    {
        try
        {
            lastAttackTime = Time.time;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WeaponModel: Error registering attack: {ex.Message}");
        }
    }
}

/// <summary>
/// Melee weapon implementation for close-range combat (sword, axe, etc.)
/// Uses cone-based attack detection for swing mechanics
/// </summary>
public class MeleeWeapon : WeaponModel
{
    [Header("Melee Settings")]
    public float swingAngle = 60f;
    public GameObject slashEffectPrefab;
    
    /// <summary>
    /// Performs a melee attack in a cone-shaped area
    /// </summary>
    /// <param name="attackPosition">World position to attack from</param>
    /// <param name="attackDirection">Direction of the melee swing</param>
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        try
        {
            if (!CanAttack) 
            {
                Debug.Log($"MeleeWeapon: Cannot attack - cooldown active for {weaponName}");
                return;
            }
            
            RegisterAttack();
            
            // Spawn visual effect
            if (slashEffectPrefab != null)
            {
                GameObject slash = Instantiate(slashEffectPrefab, attackPosition, Quaternion.LookRotation(attackDirection));
                Destroy(slash, 0.5f);
                Debug.Log($"MeleeWeapon: Spawned slash effect for {weaponName}");
            }
            else
            {
                Debug.LogWarning($"MeleeWeapon: No slash effect prefab assigned for {weaponName}");
            }
            
            // Attack in cone
            Vector3 attackCenter = attackPosition + attackDirection * (attackRange * 0.5f);
            Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, targetLayer);
            
            Debug.Log($"MeleeWeapon: {weaponName} attack found {hits.Length} potential targets in range");
            
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
        catch (System.Exception ex)
        {
            Debug.LogError($"MeleeWeapon: Error during attack with {weaponName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies damage to a valid target
    /// </summary>
    /// <param name="target">The target GameObject to damage</param>
    protected virtual void DamageTarget(GameObject target)
    {
        try
        {
            if (target.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(baseDamage);
                Debug.Log($"{weaponName} hit {target.name} for {baseDamage} damage");
            }
            else
            {
                Debug.Log($"MeleeWeapon: {weaponName} hit {target.name} but no EnemyController found");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MeleeWeapon: Error damaging target {target.name}: {ex.Message}");
        }
    }
}

/// <summary>
/// Ranged weapon implementation for projectile-based attacks (bow, gun, etc.)
/// Spawns and launches projectiles toward targets
/// </summary>
public class RangedWeapon : WeaponModel
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public Transform firePoint;
    
    /// <summary>
    /// Performs a ranged attack by spawning and launching a projectile
    /// </summary>
    /// <param name="attackPosition">World position to attack from</param>
    /// <param name="attackDirection">Direction to fire the projectile</param>
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        try
        {
            if (!CanAttack) 
            {
                Debug.Log($"RangedWeapon: Cannot attack - cooldown active for {weaponName}");
                return;
            }
            
            RegisterAttack();
            
            // Spawn projectile
            if (projectilePrefab != null)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : attackPosition;
                GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(attackDirection));
                
                Debug.Log($"RangedWeapon: Fired projectile from {weaponName} at position {spawnPos}");
                
                // Setup projectile
                if (projectile.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = attackDirection * projectileSpeed;
                }
                else
                {
                    Debug.LogWarning($"RangedWeapon: Projectile from {weaponName} has no Rigidbody component");
                }
                
                if (projectile.TryGetComponent<ProjectileModel>(out var proj))
                {
                    proj.damage = baseDamage;
                    proj.targetLayer = targetLayer;
                }
                else
                {
                    Debug.LogWarning($"RangedWeapon: Projectile from {weaponName} has no ProjectileModel component");
                }
                
                Destroy(projectile, 5f);
            }
            else
            {
                Debug.LogError($"RangedWeapon: No projectile prefab assigned for {weaponName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"RangedWeapon: Error during attack with {weaponName}: {ex.Message}");
        }
    }
}

/// <summary>
/// Serializable data container for weapon properties and configuration
/// Used for weapon definition and data storage separate from MonoBehaviour logic
/// </summary>
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
}

/// <summary>
/// Enum defining the different types of weapons available in the game
/// </summary>
public enum WeaponType
{
    Melee,
    Ranged,
    Magic
}