// ================================================== //
// MELEE WEAPON - Combo System
// ================================================== //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponModel : WeaponModel
{
    [Header("Melee Settings")]
    public float swingAngle = 60f;
    public GameObject slashEffectPrefab;
    
    [Header("Combo System")]
    public int maxCombo = 3;
    public int currentCombo = 0;
    private float lastComboTime = 0f;
    private float comboWindow = 0.5f;
    private bool comboQueued = false;

    private Coroutine attackTimeoutCoroutine;
    
    void Update()
    {
        // Reset combo if window expired
        if (Time.time > lastComboTime + comboWindow && currentCombo > 0)
        {
            ResetCombo();
        }
        
        // Process queued combo
        if (comboQueued && !isAttacking && CanAttack())
        {
            comboQueued = false;
            ContinueCombo();
        }
    }
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        // if (!CanAttack())
        // {
        //     Debug.LogError("Cannot attack.");
        //     if (isAttacking && Time.time <= lastComboTime + comboWindow)
        //     {
        //         Debug.Log("Attack is queued into combo.");
        //         comboQueued = true;
        //     }
        //     return;
        // }

        StartAttack();

        // Start a timeout to ensure isAttacking gets reset
        if (attackTimeoutCoroutine != null)
            StopCoroutine(attackTimeoutCoroutine);
        attackTimeoutCoroutine = StartCoroutine(AttackTimeout());
        
        if (currentCombo == 0)
        {
            Debug.Log("Combo index is 0. Starting combo...");
            StartCombo();
        }
    }

    private IEnumerator AttackTimeout()
    {
        // Wait for maximum reasonable attack duration (2x attackCooldown)
        yield return new WaitForSeconds(attackCooldown * 2f);
        
        if (isAttacking)
        {
            Debug.LogWarning($"Attack timeout - forcing isAttacking to false");
            isAttacking = false;
        }
    }
    
    private void StartCombo()
    {
        currentCombo = 1;
        lastComboTime = Time.time;
        // isAttacking = true;
        
        if (animator != null)
        {
            animator.SetInteger("ComboIndex", currentCombo);
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("Animator is null.");
        }
        
        Debug.Log("Registering attack...");
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
        // isAttacking = true;
        
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
    public override void OnAttackComplete()
    {
        isAttacking = false;
        
        // Stop the timeout coroutine
        if (attackTimeoutCoroutine != null)
        {
            StopCoroutine(attackTimeoutCoroutine);
            attackTimeoutCoroutine = null;
        }
        
        Debug.Log("Attack animation completed");
    }

    void OnDisable()
    {
        // Clean up
        if (attackTimeoutCoroutine != null)
        {
            StopCoroutine(attackTimeoutCoroutine);
            attackTimeoutCoroutine = null;
        }
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