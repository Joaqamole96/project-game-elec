// -------------------------------------------------- //
// Scripts/Controllers/EnemyController.cs
// -------------------------------------------------- //

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public int maxHealth = 30;
    public int damage = 10;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;
    public float attackCooldown = 2f;
    public NavMeshAgent agent;
    public Animator animator;
    public enum EnemyState { Patrolling, Chasing, Attacking, Dead }
    public int CurrentHealth { get; private set; }
    public EnemyState CurrentState { get; private set; }
    
    private Transform player;
    private Vector3 patrolPoint;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    
    void Start()
    {
        CurrentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange - 0.2f;
        
        SetRandomPatrolPoint();
        SetState(EnemyState.Patrolling);
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        switch (CurrentState)
        {
            case EnemyState.Patrolling: UpdatePatrolling(); break;
            case EnemyState.Chasing: UpdateChasing(); break;
            case EnemyState.Attacking: UpdateAttacking(); break;
        }
    }
    
    private void UpdatePatrolling()
    {
        if (PlayerInRange(detectionRange))
        {
            SetState(EnemyState.Chasing);
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance) SetRandomPatrolPoint();
    }
    
    private void UpdateChasing()
    {
        if (!PlayerInRange(detectionRange))
        {
            SetState(EnemyState.Patrolling);
            return;
        }
        
        if (PlayerInRange(attackRange))
        {
            SetState(EnemyState.Attacking);
            return;
        }
        
        if (agent != null && agent.isOnNavMesh) agent.SetDestination(player.position);
    }
    
    private void UpdateAttacking()
    {
        if (agent != null) agent.SetDestination(transform.position);
        
        FacePlayer();
        
        if (!PlayerInRange(attackRange)) SetState(EnemyState.Chasing);
        else if (Time.time >= lastAttackTime + attackCooldown) PerformAttack();
    }
    
    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (animator != null) animator.SetTrigger("Attack");
        
        if (PlayerInRange(attackRange))
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null) playerController.TakeDamage(damage);
        }
    }
    
    private bool PlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }
    
    private void SetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * 3f;
        patrolPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            if (agent != null && agent.isOnNavMesh) agent.SetDestination(patrolPoint);
        }
    }
    
    private void SetState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", CurrentState == EnemyState.Chasing || CurrentState == EnemyState.Patrolling);
            animator.SetBool("IsAttacking", CurrentState == EnemyState.Attacking);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        if (CurrentState != EnemyState.Chasing && CurrentState != EnemyState.Attacking) SetState(EnemyState.Chasing);
        
        if (CurrentHealth <= 0) Die();
    }
    
    private void Die()
    {
        isDead = true;
        SetState(EnemyState.Dead);
        
        if (agent != null) agent.isStopped = true;
            
        if (animator != null) animator.SetTrigger("Die");
        
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders) col.enabled = false;
        
        Destroy(gameObject, 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}