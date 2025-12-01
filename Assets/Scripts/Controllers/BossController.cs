// ================================================== //
// Scripts/Controllers/BossController.cs
// ================================================== //

using UnityEngine;
using System.Collections;

/// <summary>
/// Boss enemy controller with 2-phase combat and enrage mechanics
/// Simple but challenging boss encounter
/// </summary>
public class BossController : EnemyController
{
    [Header("Boss Stats")]
    public string bossName = "Boss";
    
    [Header("Phase System")]
    public bool isEnraged = false;
    public float enrageThreshold = 0.5f;
    
    [Header("Enrage Buffs")]
    public float enrageDamageMultiplier = 1.2f;
    public float enrageSpeedMultiplier = 1.1f;
    
    [Header("Visual Effects")]
    public ParticleSystem enrageEffect;
    
    private float baseMovespeed;
    private int baseDamage;
    private bool hasEnraged = false;
    
    protected override void OnStart()
    {
        // Boss base stats
        maxHealth = 200;
        damage = 25;
        moveSpeed = 3f;
        attackRange = 2.5f;
        detectionRange = 15f;
        attackCooldown = 5f;
        
        baseMovespeed = moveSpeed;
        baseDamage = damage;
        
        Debug.Log($"{bossName} initialized with {maxHealth} HP");
    }
    
    protected override void OnUpdate()
    {
        // Check for enrage transition
        if (!hasEnraged && CurrentHealth <= maxHealth * enrageThreshold)
        {
            EnterEnrage();
        }
    }
    
    // ------------------------- //
    // ENRAGE SYSTEM
    // ------------------------- //
    
    private void EnterEnrage()
    {
        hasEnraged = true;
        isEnraged = true;
        
        Debug.Log($"{bossName} ENRAGED!");
        Debug.Log($"Current health: {CurrentHealth}");
        Debug.Log($"Max health: {maxHealth}");
        Debug.Log($"Enraged health threshold: {maxHealth * enrageThreshold}");
        Debug.Log($"Is enraged: {CurrentHealth <= maxHealth * enrageThreshold}");
        
        // Apply enrage buffs
        damage = Mathf.RoundToInt(baseDamage * enrageDamageMultiplier);
        moveSpeed = baseMovespeed * enrageSpeedMultiplier;
        agent.speed = moveSpeed;
        attackCooldown *= 0.7f; // Attack 30% faster
        
        // Trigger enrage animation
        if (animator != null)
        {
            animator.SetTrigger("Enrage");
            animator.SetBool("IsEnraged", true);
        }
        
        // Visual effect
        if (enrageEffect != null)
        {
            enrageEffect.Play();
        }
        
        StartCoroutine(EnrageVisualEffect());
    }
    
    private IEnumerator EnrageVisualEffect()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) yield break;
        
        Color original = rend.material.color;
        
        // Flash red
        for (int i = 0; i < 3; i++)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.2f);
        }
        
        // Permanent red tint
        rend.material.color = new Color(1f, 0.3f, 0.3f);
    }
    
    // ------------------------- //
    // COMBAT OVERRIDES
    // ------------------------- //
    
    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            // Alternate between two attacks
            int attackType = Random.Range(1, 3); // 1 or 2
            animator.SetTrigger($"Attack{attackType}");
        }
        
        if (PlayerInRange(attackRange))
        {
            DealDamageToPlayer(damage);
        }
    }
    
    public override void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        base.TakeDamage(damageAmount);
    }
    
    protected override void Die()
    {
        Debug.Log($"{bossName} DEFEATED!");
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        if (enrageEffect != null && enrageEffect.isPlaying)
        {
            enrageEffect.Stop();
        }
        
        base.Die();
    }
    
    protected override void DropLoot()
    {
        // Bosses drop 100-200 gold
        int goldAmount = Random.Range(100, 200);
        
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
        
        Debug.Log($"Boss dropped {goldAmount} gold!");
    }
    
    // ------------------------- //
    // DEBUG
    // ------------------------- //
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}