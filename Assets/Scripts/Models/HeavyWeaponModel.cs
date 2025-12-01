// ================================================== //
// CHARGE WEAPON - Hold to charge
// ================================================== //

using UnityEngine;

public class HeavyWeaponModel : WeaponModel
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
        if (!CanAttack()) return;
        
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