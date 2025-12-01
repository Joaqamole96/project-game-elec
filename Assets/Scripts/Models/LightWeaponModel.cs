// ================================================== //
// RAPID WEAPON - Fast alternating attacks
// ================================================== //

using UnityEngine;

public class LightWeaponModel : WeaponModel
{
    [Header("Rapid Settings")]
    public float attackSpeedMultiplier = 1.5f;
    
    private int attackCount = 0;
    private int finisherThreshold = 3;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack()) return;
        
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