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
        detectionRange = 8f;
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



