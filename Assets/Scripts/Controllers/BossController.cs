// ================================================== //
// Scripts/Controllers/BossController.cs
// ================================================== //

using UnityEngine;
using System.Collections;

/// <summary>
/// Boss enemy controller with multi-phase combat, special abilities, and enrage mechanics
/// Extends base EnemyController with enhanced behaviors for challenging encounters
/// </summary>
public class BossController : EnemyController
{
    [Header("Boss Stats")]
    public string bossName = "Boss";
    public int phase = 1;
    public int maxPhases = 3;
    
    [Header("Phase Thresholds (% of max health)")]
    public float phase2Threshold = 0.66f;
    public float phase3Threshold = 0.33f;
    
    [Header("Boss Abilities")]
    public float specialAbilityCooldown = 10f;
    public float areaAttackRadius = 5f;
    public float areaAttackDamage = 15f;
    public int summonCount = 2;
    
    [Header("Enrage (Final Phase)")]
    public bool isEnraged = false;
    public float enrageDamageMultiplier = 1.5f;
    public float enrageSpeedMultiplier = 1.3f;
    
    [Header("Visual Effects")]
    public ParticleSystem phaseChangeEffect;
    public ParticleSystem enrageEffect;
    public ParticleSystem specialAbilityEffect;
    
    private float lastSpecialAbilityTime = 0f;
    private float baseMovespeed;
    private int baseDamage;
    private bool hasEnteredPhase2 = false;
    private bool hasEnteredPhase3 = false;
    
    protected override void OnStart()
    {
        // Boss base stats
        maxHealth = 200;
        damage = 25;
        moveSpeed = 3f;
        attackRange = 2.5f;
        detectionRange = 15f;
        attackCooldown = 2f;
        
        baseMovespeed = moveSpeed;
        baseDamage = damage;
        
        // Boss-specific setup
        agent.stoppingDistance = attackRange - 0.5f;
        
        Debug.Log($"{bossName} initialized with {maxHealth} HP");
    }
    
    protected override void OnUpdate()
    {
        // Check phase transitions
        CheckPhaseTransition();
        
        // Use special abilities
        if (Time.time >= lastSpecialAbilityTime + specialAbilityCooldown)
        {
            UseSpecialAbility();
        }
    }
    
    // ------------------------- //
    // PHASE MANAGEMENT
    // ------------------------- //
    
    private void CheckPhaseTransition()
    {
        float healthPercent = (float)CurrentHealth / maxHealth;
        
        if (!hasEnteredPhase2 && healthPercent <= phase2Threshold)
        {
            EnterPhase2();
        }
        else if (!hasEnteredPhase3 && healthPercent <= phase3Threshold)
        {
            EnterPhase3();
        }
    }
    
    private void EnterPhase2()
    {
        hasEnteredPhase2 = true;
        phase = 2;
        
        Debug.Log($"{bossName} entered PHASE 2!");
        
        // Phase 2 buffs
        attackCooldown *= 0.8f; // Attack 20% faster
        
        if (animator != null)
        {
            animator.SetTrigger("PhaseChange");
        }
        
        if (phaseChangeEffect != null)
        {
            phaseChangeEffect.Play();
        }
        
        // Heal slightly
        Heal(maxHealth / 10);
    }
    
    private void EnterPhase3()
    {
        hasEnteredPhase3 = true;
        phase = 3;
        isEnraged = true;
        
        Debug.Log($"{bossName} entered PHASE 3 - ENRAGED!");
        
        // Enrage buffs
        damage = Mathf.RoundToInt(baseDamage * enrageDamageMultiplier);
        moveSpeed = baseMovespeed * enrageSpeedMultiplier;
        agent.speed = moveSpeed;
        attackCooldown *= 0.6f; // Attack 40% faster
        
        if (animator != null)
        {
            animator.SetTrigger("Enrage");
            animator.SetBool("IsEnraged", true);
        }
        
        if (enrageEffect != null)
        {
            enrageEffect.Play();
        }
        
        // Visual feedback
        StartCoroutine(EnrageVisualEffect());
    }
    
    private IEnumerator EnrageVisualEffect()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) yield break;
        
        Color original = rend.material.color;
        
        for (int i = 0; i < 5; i++)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.2f);
        }
        
        // Permanent red tint when enraged
        rend.material.color = new Color(1f, 0.3f, 0.3f);
    }
    
    // ------------------------- //
    // SPECIAL ABILITIES
    // ------------------------- //
    
    private void UseSpecialAbility()
    {
        lastSpecialAbilityTime = Time.time;
        
        // Choose ability based on phase
        switch (phase)
        {
            case 1:
                PerformGroundSlam();
                break;
            case 2:
                if (Random.value > 0.5f)
                    PerformGroundSlam();
                else
                    SummonMinions();
                break;
            case 3:
                // Enraged - use both abilities more frequently
                if (Random.value > 0.5f)
                    PerformGroundSlam();
                else
                    PerformRageAttack();
                break;
        }
    }
    
    private void PerformGroundSlam()
    {
        Debug.Log($"{bossName} performs GROUND SLAM!");
        
        if (animator != null)
        {
            animator.SetTrigger("GroundSlam");
        }
        
        if (specialAbilityEffect != null)
        {
            specialAbilityEffect.Play();
        }
        
        // Delayed damage (animation sync)
        StartCoroutine(GroundSlamDamage());
    }
    
    private IEnumerator GroundSlamDamage()
    {
        yield return new WaitForSeconds(0.5f); // Animation wind-up
        
        // Deal area damage
        Collider[] hits = Physics.OverlapSphere(transform.position, areaAttackRadius);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<PlayerController>(out var playerController))
                {
                    int areaDamage = Mathf.RoundToInt(areaAttackDamage * (isEnraged ? enrageDamageMultiplier : 1f));
                    playerController.TakeDamage(areaDamage);
                    
                    // Knockback
                    if (hit.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 knockbackDir = (hit.transform.position - transform.position).normalized;
                        rb.AddForce(knockbackDir * 10f, ForceMode.Impulse);
                    }
                }
            }
        }
        
        // Visual shockwave
        CreateShockwave();
    }
    
    private void CreateShockwave()
    {
        GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shockwave.transform.position = transform.position;
        shockwave.transform.localScale = new Vector3(areaAttackRadius * 2f, 0.1f, areaAttackRadius * 2f);
        
        Renderer renderer = shockwave.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = new Color(1f, 0.5f, 0f, 0.5f)
        };
        renderer.material = mat;
        
        Destroy(shockwave.GetComponent<Collider>());
        Destroy(shockwave, 0.5f);
    }
    
    private void SummonMinions()
    {
        Debug.Log($"{bossName} summons minions!");
        
        if (animator != null)
        {
            animator.SetTrigger("Summon");
        }
        
        for (int i = 0; i < summonCount; i++)
        {
            Vector3 spawnOffset = new(
                Random.Range(-3f, 3f),
                0,
                Random.Range(-3f, 3f)
            );
            
            Vector3 spawnPosition = transform.position + spawnOffset;
            
            // Spawn minion (use procedural enemy)
            GameObject minion = EnemyGenerator.CreateMeleeEnemy(spawnPosition);
            minion.name = $"{bossName}_Minion";
            
            // Make minions weaker
            if (minion.TryGetComponent<EnemyController>(out var enemyController))
            {
                enemyController.maxHealth = 15;
                enemyController.damage = 5;
            }
        }
    }
    
    private void PerformRageAttack()
    {
        Debug.Log($"{bossName} performs RAGE ATTACK!");
        
        if (animator != null)
        {
            animator.SetTrigger("RageAttack");
        }
        
        // Multi-hit combo attack
        StartCoroutine(RageAttackSequence());
    }
    
    private IEnumerator RageAttackSequence()
    {
        for (int i = 0; i < 3; i++)
        {
            if (PlayerInRange(attackRange + 1f))
            {
                DealDamageToPlayer(damage);
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    // ------------------------- //
    // COMBAT OVERRIDES
    // ------------------------- //
    
    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            // Random attack animation
            int attackType = Random.Range(1, 4);
            animator.SetTrigger($"Attack{attackType}");
        }
        
        if (PlayerInRange(attackRange))
        {
            DealDamageToPlayer(damage);
            
            // Phase 3: Chance for double hit
            if (isEnraged && Random.value > 0.7f)
            {
                StartCoroutine(DelayedSecondHit());
            }
        }
    }
    
    private IEnumerator DelayedSecondHit()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (PlayerInRange(attackRange))
        {
            DealDamageToPlayer(damage / 2);
        }
    }
    
    public override void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        // Boss takes reduced damage when not enraged
        if (!isEnraged)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * 0.8f);
        }
        
        base.TakeDamage(damageAmount);
        
        // Boss-specific reactions
        if (CurrentHealth > 0 && Random.value > 0.7f)
        {
            // Chance to counter-attack
            if (PlayerInRange(attackRange + 2f))
            {
                PerformQuickCounter();
            }
        }
    }
    
    private void PerformQuickCounter()
    {
        if (animator != null)
        {
            animator.SetTrigger("Counter");
        }
        
        if (PlayerInRange(attackRange + 2f))
        {
            DealDamageToPlayer(damage / 2);
        }
    }
    
    protected override void Die()
    {
        Debug.Log($"{bossName} DEFEATED!");
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Stop all effects
        if (enrageEffect != null && enrageEffect.isPlaying)
        {
            enrageEffect.Stop();
        }
        
        // Create dramatic death effect
        CreateDeathExplosion();
        
        base.Die();
    }
    
    private void CreateDeathExplosion()
    {
        // Visual feedback for boss death
        for (int i = 0; i < 5; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = transform.position + Vector3.up;
            particle.transform.localScale = Vector3.one * 0.5f;
            
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new(Shader.Find("Standard"))
            {
                color = new Color(1f, 0.8f, 0f)
            };
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.yellow * 3f);
            renderer.material = mat;
            
            if (particle.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }
            
            Destroy(particle, 2f);
        }
    }
    
    protected override void DropLoot()
    {
        // Bosses drop much more gold
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
    // UTILITY
    // ------------------------- //
    
    private void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        Debug.Log($"{bossName} healed for {amount}! HP: {CurrentHealth}/{maxHealth}");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw area attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }
}