// ================================================== //
// Scripts/Controllers/RangedEnemyController.cs (UPDATED)
// ================================================== //

using UnityEngine;

public class RangedEnemyController : EnemyController
{
    [Header("Ranged Settings")]
    public float minDistance = 4f;
    public float projectileSpeed = 10f;
    
    protected override void OnStart()
    {
        maxHealth = 20;
        damage = 8;
        moveSpeed = 2f;
        attackRange = 8f;
        detectionRange = 12f;
        attackCooldown = 2.5f;
        
        agent.stoppingDistance = minDistance;
    }
    
    protected override void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
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
    
    protected override void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        if (animator == null) return;
        
        animator.SetBool("IsMoving", newState == EnemyState.Chasing || 
                                      newState == EnemyState.Patrolling || 
                                      newState == EnemyState.Retreating);
        animator.SetBool("IsAttacking", newState == EnemyState.Attacking);
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
        projectile.name = "EnemyProjectile";
        
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = direction * projectileSpeed;
        
        Renderer renderer = projectile.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = Color.red
        };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.red * 2f);
        renderer.material = mat;
        
        ProjectileController projScript = projectile.AddComponent<ProjectileController>();
        projScript.damage = damage;
        
        Destroy(projectile, 5f);
        
        Debug.Log("Ranged enemy shot projectile");
    }
    
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        
        if (!isDead && animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
    }
    
    protected override void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        base.Die();
    }
    
    protected override void DropLoot()
    {
        base.DropLoot(); // Use ItemRegistry system
    }
}