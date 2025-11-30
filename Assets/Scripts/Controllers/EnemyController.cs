// ================================================== //
// Scripts/Controllers/EnemyController.cs (REFACTORED BASE CLASS)
// ================================================== //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 30;
    public int damage = 10;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;
    public float attackCooldown = 2f;
    
    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    
    public EnemyState CurrentState { get; protected set; }
    public int CurrentHealth { get; protected set; }
    
    public enum EnemyState { Patrolling, Chasing, Attacking, Retreating, Dead }
    
    protected Transform player;
    protected Vector3 patrolPoint;
    protected bool isDead = false;
    protected float lastAttackTime = 0f;
    
    // ------------------------- //
    // UNITY LIFECYCLE
    // ------------------------- //
    
    protected virtual void Start()
    {
        CurrentHealth = maxHealth;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
        // CRITICAL: Wait for NavMesh before enabling agent
        StartCoroutine(WaitForNavMesh());
        
        OnStart();
    }
    
    protected virtual void Update()
    {
        if (isDead || player == null) return;
        
        UpdateState();
        ExecuteStateBehavior();
        
        OnUpdate();
    }
    
    // ------------------------- //
    // VIRTUAL METHODS FOR SUBCLASSES
    // ------------------------- //
    
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnStateChanged(EnemyState oldState, EnemyState newState) { }
    
    // ------------------------- //
    // STATE MANAGEMENT
    // ------------------------- //
    
    protected virtual void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            SetState(EnemyState.Attacking);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chasing);
        }
        else
        {
            SetState(EnemyState.Patrolling);
        }
    }
    
    protected virtual void ExecuteStateBehavior()
    {
        switch (CurrentState)
        {
            case EnemyState.Patrolling:
                UpdatePatrolling();
                break;
            case EnemyState.Chasing:
                UpdateChasing();
                break;
            case EnemyState.Attacking:
                UpdateAttacking();
                break;
            case EnemyState.Retreating:
                UpdateRetreating();
                break;
        }
    }
    
    protected virtual void UpdatePatrolling()
    {
        // CRITICAL: Check if agent is on NavMesh before accessing properties
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }
        
        // Check if reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomPatrolPoint();
        }
    }
    
    protected virtual void UpdateChasing()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
    
    protected virtual void UpdateAttacking()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
        }
        
        FacePlayer();
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }
    }
    
    protected virtual void UpdateRetreating()
    {
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * 3f;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(retreatTarget);
        }
    }
    
    protected void SetState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        
        EnemyState oldState = CurrentState;
        CurrentState = newState;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", CurrentState == EnemyState.Chasing || CurrentState == EnemyState.Patrolling);
            animator.SetBool("IsAttacking", CurrentState == EnemyState.Attacking);
        }
        
        OnStateChanged(oldState, newState);
    }
    
    // ------------------------- //
    // COMBAT
    // ------------------------- //
    
    protected virtual void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            DealDamageToPlayer(damage);
        }
    }
    
    protected void DealDamageToPlayer(int damageAmount)
    {
        if (player.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.TakeDamage(damageAmount);
        }
    }
    
    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        CurrentHealth -= damageAmount;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        StartCoroutine(FlashRed());
        
        // Force chase state if hit
        if (CurrentState != EnemyState.Chasing && CurrentState != EnemyState.Attacking)
        {
            SetState(EnemyState.Chasing);
        }
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        isDead = true;
        SetState(EnemyState.Dead);
        
        if (agent != null) agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders) col.enabled = false;
        
        // Notify combat manager
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            combatManager.OnEnemyDied(gameObject);
        }
        
        DropLoot();
        
        Destroy(gameObject, 2f);
    }
    
    protected virtual void DropLoot()
    {
        int goldAmount = Random.Range(5, 15);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var pc))
        {
            if (pc.inventory != null)
            {
                // Apply gold power modifier
                if (pc.powerManager != null)
                {
                    goldAmount = pc.powerManager.ModifyGoldGained(goldAmount);
                }
                
                pc.inventory.AddGold(goldAmount);
            }
        }
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    protected void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    protected void SetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * 5f;
        Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // CRITICAL: Sample NavMesh position before setting destination
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            
            if (agent != null && agent.isOnNavMesh && agent.isActiveAndEnabled)
            {
                agent.SetDestination(patrolPoint);
            }
        }
    }
    
    protected bool PlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    protected IEnumerator FlashRed() 
    {
        Renderer rend = GetComponent<Renderer>();
        Color original = rend.material.color;
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        rend.material.color = original;
    }

    protected IEnumerator WaitForNavMesh()
    {
        // Wait until we're on NavMesh
        int attempts = 0;
        while (!agent.isOnNavMesh && attempts < 10)
        {
            yield return new WaitForSeconds(0.5f);
            attempts++;
            
            // Try to warp to nearest NavMesh position
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                break;
            }
        }
        
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"Enemy {gameObject.name} could not be placed on NavMesh after {attempts} attempts!");
            enabled = false; // Disable the enemy controller
            yield break;
        }
        
        // Setup agent properties
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange - 0.2f;
        
        SetRandomPatrolPoint();
        SetState(EnemyState.Patrolling);
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