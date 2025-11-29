// ================================================== //
// Scripts/Controllers/TankEnemyController.cs
// ================================================== //

using UnityEngine;

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