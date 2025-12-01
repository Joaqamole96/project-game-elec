// ================================================== //
// Scripts/Controllers/MeleeEnemyController.cs (UPDATED)
// ================================================== //

using UnityEngine;

public class MeleeEnemyController : EnemyController
{
    protected override void OnStart()
    {
        maxHealth = 30;
        damage = 10;
        moveSpeed = 2.5f;
        attackRange = 1.5f;
        detectionRange = 8f;
        attackCooldown = 2f;
    }
    
    protected override void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        if (animator == null) return;
        
        // Update animator based on state
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
        
        if (PlayerInRange(attackRange))
        {
            DealDamageToPlayer(damage);
            Debug.Log($"Melee enemy dealt {damage} damage");
        }
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