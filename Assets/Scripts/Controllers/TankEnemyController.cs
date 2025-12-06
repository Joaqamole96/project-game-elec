// ================================================== //
// Scripts/Controllers/TankEnemyController.cs (UPDATED)
// ================================================== //

using UnityEngine;

public class TankEnemyController : EnemyController
{
    [Header("Tank Settings")]
    public float areaAttackRadius = 2.5f;
    public float knockbackForce = 5f;
    
    protected override void OnStart()
    {
        maxHealth = 60;
        damage = 20;
        moveSpeed = 1.5f;
        attackRange = 1f;
        detectionRange = 8f;
        attackCooldown = 2.5f;
    }
    
    protected override void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        if (animator == null) return;
        
        animator.SetBool("IsMoving", newState == EnemyState.Chasing || newState == EnemyState.Patrolling);
        animator.SetBool("IsAttacking", newState == EnemyState.Attacking);
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
    
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        
        // Tank has minimal flinch reaction
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }
}