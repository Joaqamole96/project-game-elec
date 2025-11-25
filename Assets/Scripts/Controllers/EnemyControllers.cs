// ================================================== //
// Scripts/Controllers/MeleeEnemyController.cs
// ================================================== //

using UnityEngine;

/// <summary>
/// Standard melee enemy - balanced stats, straightforward behavior
/// </summary>
public class MeleeEnemyController : EnemyController
{
    protected override void OnStart()
    {
        // Melee enemy defaults (already set in base, can override here)
        maxHealth = 30;
        damage = 10;
        moveSpeed = 2.5f;
        attackRange = 1.5f;
        detectionRange = 10f;
        attackCooldown = 1.5f;
    }
    
    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Simple melee swing
        if (PlayerInRange(attackRange))
        {
            DealDamageToPlayer(damage);
            Debug.Log($"Melee enemy dealt {damage} damage");
        }
    }
    
    protected override void DropLoot()
    {
        int goldAmount = Random.Range(8, 15);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var pc))
        {
            if (pc.inventory != null)
            {
                if (pc.powerManager != null)
                {
                    goldAmount = pc.powerManager.ModifyGoldGained(goldAmount);
                }
                pc.inventory.AddGold(goldAmount);
            }
        }
    }
}

// ================================================== //
// Scripts/Controllers/RangedEnemyController.cs
// ================================================== //

/// <summary>
/// Ranged enemy - shoots projectiles, maintains distance
/// </summary>
public class RangedEnemyController : EnemyController
{
    [Header("Ranged Settings")]
    public float minDistance = 4f;
    public float projectileSpeed = 10f;
    
    protected override void OnStart()
    {
        // Ranged enemy stats
        maxHealth = 20;
        damage = 8;
        moveSpeed = 2f;
        attackRange = 8f;
        detectionRange = 12f;
        attackCooldown = 2f;
        
        agent.stoppingDistance = minDistance;
    }
    
    protected override void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Custom state logic for ranged enemy
        if (distanceToPlayer < minDistance)
        {
            SetState(EnemyState.Retreating);
        }
        else if (distanceToPlayer <= attackRange)
        {
            SetState(EnemyState.Attacking);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chasing);
        }
        else
        {
            SetState(EnemyState.Patrolling);
        }
    }
    
    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        ShootProjectile();
    }
    
    private void ShootProjectile()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 spawnPosition = transform.position + Vector3.up * 1f + direction * 0.5f;
        
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one * 0.3f;
        projectile.name = "EnemyProjectileModel";
        
        // Physics
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = direction * projectileSpeed;
        
        // Visual
        Renderer renderer = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.red * 2f);
        renderer.material = mat;
        
        // Damage component
        EnemyProjectileModel projScript = projectile.AddComponent<EnemyProjectileModel>();
        projScript.damage = damage;
        
        Destroy(projectile, 5f);
        
        Debug.Log("Ranged enemy shot projectile");
    }
    
    protected override void DropLoot()
    {
        int goldAmount = Random.Range(10, 18);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var pc))
        {
            if (pc.inventory != null)
            {
                if (pc.powerManager != null)
                {
                    goldAmount = pc.powerManager.ModifyGoldGained(goldAmount);
                }
                pc.inventory.AddGold(goldAmount);
            }
        }
    }
}

// ================================================== //
// Scripts/Controllers/TankEnemyController.cs
// ================================================== //

/// <summary>
/// Tank enemy - high HP, slow, heavy damage with area attack
/// </summary>
public class TankEnemyController : EnemyController
{
    [Header("Tank Settings")]
    public float areaAttackRadius = 2.5f;
    public float knockbackForce = 5f;
    
    protected override void OnStart()
    {
        // Tank enemy stats
        maxHealth = 60;
        damage = 20;
        moveSpeed = 1.5f;
        attackRange = 2f;
        detectionRange = 8f;
        attackCooldown = 2.5f;
    }
    
    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        HeavyAreaAttack();
    }
    
    private void HeavyAreaAttack()
    {
        // Area attack in front of tank
        Collider[] hits = Physics.OverlapSphere(transform.position, areaAttackRadius);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<PlayerController>(out var playerController))
                {
                    playerController.TakeDamage(damage);
                    Debug.Log($"Tank enemy dealt {damage} HEAVY damage!");
                    
                    // Knockback
                    if (hit.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 knockbackDir = (player.position - transform.position).normalized;
                        rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
    
    protected override void DropLoot()
    {
        int goldAmount = Random.Range(15, 30);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var pc))
        {
            if (pc.inventory != null)
            {
                if (pc.powerManager != null)
                {
                    goldAmount = pc.powerManager.ModifyGoldGained(goldAmount);
                }
                pc.inventory.AddGold(goldAmount);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // NOTE: Apparently inaccessible due to protection level, thus commented out.
        // base.OnDrawGizmosSelected();
        
        // Draw area attack radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }
}