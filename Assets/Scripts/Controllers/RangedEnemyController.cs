// ================================================== //
// Scripts/Controllers/RangedEnemyController.cs
// ================================================== //

using UnityEngine;

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
        Material mat = new(Shader.Find("Standard"))
        {
            color = Color.red
        };
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