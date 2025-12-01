// ================================================== //
// MELEE WEAPON - Combo System
// ================================================== //

using UnityEngine;

public class MeleeWeaponModel : WeaponModel
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