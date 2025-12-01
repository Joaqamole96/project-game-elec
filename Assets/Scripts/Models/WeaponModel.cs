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
// MELEE WEAPON - Combo System
// ================================================== //

public class MeleeWeapon : WeaponModel
{
    [Header("Melee Settings")]
    public float swingAngle = 60f;
    public GameObject slashEffectPrefab;
    
    [Header("Combo System")]
    public int maxCombo = 3;
    private int currentCombo = 0;
    private float lastComboTime = 0f;
    private float comboWindow = 0.5f;
    private bool comboQueued = false;
    
    void Update()
    {
        // Reset combo if window expired
        if (Time.time > lastComboTime + comboWindow && currentCombo > 0)
        {
            ResetCombo();
        }
        
        // Process queued combo
        if (comboQueued && !isAttacking && CanAttack)
        {
            comboQueued = false;
            ContinueCombo();
        }
    }
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack)
        {
            // Queue next combo attack if within window
            if (isAttacking && Time.time <= lastComboTime + comboWindow)
            {
                comboQueued = true;
            }
            return;
        }
        
        if (currentCombo == 0)
        {
            StartCombo();
        }
    }
    
    private void StartCombo()
    {
        currentCombo = 1;
        lastComboTime = Time.time;
        isAttacking = true;
        
        if (animator != null)
        {
            animator.SetInteger("ComboIndex", currentCombo);
            animator.SetTrigger("Attack");
        }
        
        RegisterAttack();
    }
    
    private void ContinueCombo()
    {
        currentCombo++;
        if (currentCombo > maxCombo)
        {
            currentCombo = 1; // Loop back
        }
        
        lastComboTime = Time.time;
        isAttacking = true;
        
        if (animator != null)
        {
            animator.SetInteger("ComboIndex", currentCombo);
            animator.SetTrigger("Attack");
        }
        
        RegisterAttack();
    }
    
    private void ResetCombo()
    {
        currentCombo = 0;
        comboQueued = false;
        
        if (animator != null)
        {
            animator.SetInteger("ComboIndex", 0);
        }
    }
    
    // Called by animation event
    public void OnDealDamage()
    {
        // Damage detection in cone
        Vector3 attackCenter = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, targetLayer);
        
        foreach (Collider hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angleToTarget <= swingAngle / 2f)
            {
                DamageTarget(hit.gameObject);
            }
        }
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
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

// ================================================== //
// CHARGE WEAPON - Hold to charge
// ================================================== //

public class ChargeWeapon : WeaponModel
{
    [Header("Charge Settings")]
    public float maxChargeTime = 2f;
    public float chargeMultiplier = 2f;
    
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float currentCharge = 0f;
    
    void Update()
    {
        if (isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            currentCharge = Mathf.Clamp01(chargeTime / maxChargeTime);
            
            if (animator != null)
            {
                animator.SetFloat("ChargeLevel", currentCharge);
            }
        }
    }
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        // Start charging
        isCharging = true;
        chargeStartTime = Time.time;
        
        if (animator != null)
        {
            animator.SetBool("IsCharging", true);
            animator.SetTrigger("Attack");
        }
    }
    
    public void ReleaseAttack()
    {
        if (!isCharging) return;
        
        isCharging = false;
        
        if (animator != null)
        {
            animator.SetBool("IsCharging", false);
        }
        
        RegisterAttack();
    }
    
    // Called by animation event
    public void OnDealDamage()
    {
        int finalDamage = Mathf.RoundToInt(baseDamage * (1f + currentCharge * chargeMultiplier));
        
        Vector3 attackCenter = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, targetLayer);
        
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(finalDamage);
                
                // Knockback on charged attacks
                if (currentCharge > 0.5f && hit.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 knockback = (hit.transform.position - transform.position).normalized * 10f;
                    rb.AddForce(knockback, ForceMode.Impulse);
                }
            }
        }
        
        currentCharge = 0f;
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}

// ================================================== //
// RANGED WEAPON - Draw and release
// ================================================== //

public class RangedWeapon : WeaponModel
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public Transform firePoint;
    
    private bool isDrawing = false;
    private float drawStartTime = 0f;
    private float drawAmount = 0f;
    private float drawTime = 0.5f;
    
    void Update()
    {
        if (isDrawing)
        {
            drawAmount += Time.deltaTime * 2f; // 0.5s to full draw
            
            if (animator != null)
            {
                animator.SetFloat("DrawAmount", drawAmount);
            }
        }
    }
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        // Start drawing
        isDrawing = true;
        drawStartTime = Time.time;
        
        if (animator != null)
        {
            animator.SetBool("IsDrawing", true);
            animator.SetTrigger("Attack");
        }
    }
    
    public void ReleaseAttack()
    {
        if (!isDrawing) return;
        
        isDrawing = false;
        
        if (animator != null)
        {
            animator.SetBool("IsDrawing", false);
        }
        
        RegisterAttack();
    }
    
    // Called by animation event
    public void OnSpawnProjectile()
    {
        if (projectilePrefab == null) return;
        
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(transform.forward));
        
        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = transform.forward * projectileSpeed * (0.5f + drawAmount * 0.5f);
        }
        
        if (projectile.TryGetComponent<ProjectileModel>(out var proj))
        {
            proj.damage = Mathf.RoundToInt(baseDamage * (0.5f + drawAmount * 0.5f));
            proj.targetLayer = targetLayer;
        }
        
        Destroy(projectile, 5f);
        drawAmount = 0f;
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}

// ================================================== //
// MAGIC WEAPON - Spell types
// ================================================== //

public class MagicWeapon : WeaponModel
{
    [Header("Magic Settings")]
    public GameObject[] spellPrefabs; // 0:Fireball, 1:Lightning, 2:Ice
    public float projectileSpeed = 15f;
    public Transform firePoint;
    public int currentSpellType = 0;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        isAttacking = true;
        
        if (animator != null)
        {
            animator.SetInteger("SpellType", currentSpellType);
            animator.SetTrigger("Attack");
        }
        
        RegisterAttack();
    }
    
    public void SetSpellType(int spellType)
    {
        currentSpellType = Mathf.Clamp(spellType, 0, spellPrefabs.Length - 1);
    }
    
    // Called by animation event
    public void OnSpawnProjectile()
    {
        if (spellPrefabs[currentSpellType] == null) return;
        
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject spell = Instantiate(spellPrefabs[currentSpellType], spawnPos, Quaternion.LookRotation(transform.forward));
        
        if (spell.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = transform.forward * projectileSpeed;
        }
        
        if (spell.TryGetComponent<ProjectileModel>(out var proj))
        {
            proj.damage = baseDamage;
            proj.targetLayer = targetLayer;
        }
        
        Destroy(spell, 5f);
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}

// ================================================== //
// RAPID WEAPON - Fast alternating attacks
// ================================================== //

public class RapidWeapon : WeaponModel
{
    [Header("Rapid Settings")]
    public float attackSpeedMultiplier = 1.5f;
    
    private int attackCount = 0;
    private int finisherThreshold = 3;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        isAttacking = true;
        attackCount++;
        
        if (animator != null)
        {
            animator.SetInteger("AttackCount", attackCount % 2); // Alternate 0/1
            animator.SetTrigger("Attack");
            
            // Speed up animation
            animator.speed = attackSpeedMultiplier;
        }
        
        RegisterAttack();
        
        // Reset count after finisher
        if (attackCount >= finisherThreshold)
        {
            attackCount = 0;
        }
    }
    
    // Called by animation event
    public void OnDealDamage()
    {
        Vector3 attackCenter = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, targetLayer);
        
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(baseDamage);
            }
        }
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
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