using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Combat")]
    public int maxHealth = 100;
    public int playerDamage = 15;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer = 1;
    
    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    
    // Properties
    public static PlayerController Instance { get; private set; }
    public int CurrentHealth { get; private set; }
    
    private Vector3 movement;
    private bool isMoving;
    private float lastAttackTime = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        CurrentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        SpawnAtEntrance();
    }
    
    void Update()
    {
        HandleInput();
        HandleCombatInput();
        UpdateAnimations();
        UpdateRoomDetection();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        movement = new Vector3(horizontal, 0, vertical).normalized;
        isMoving = movement.magnitude > 0.1f;
    }
    
    private void HandleCombatInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }
    
    private void HandleMovement()
    {
        if (isMoving)
        {
            Vector3 moveVelocity = movement * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
            
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    
    private void PerformAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(playerDamage);
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveSpeed", movement.magnitude);
        }
    }
    
    private void UpdateRoomDetection()
    {
        if (Time.frameCount % 30 == 0)
        {
            RoomManager roomManager = FindObjectOfType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.UpdatePlayerRoom(transform.position);
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        Debug.Log("Player died - Game Over!");
        // GameManager can handle respawn/restart logic later
    }
    
    private void SpawnAtEntrance()
    {
        DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
        if (generator != null)
        {
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            transform.position = spawnPosition;
        }
    }
    
    // Mobile controls interface
    public void SetMovementInput(Vector2 input)
    {
        movement = new Vector3(input.x, 0, input.y);
        isMoving = movement.magnitude > 0.1f;
    }
    
    public void OnAttackButtonPressed()
    {
        PerformAttack();
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}