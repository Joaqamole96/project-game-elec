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
        
        // Get camera-relative movement direction
        movement = GetCameraRelativeMovement(new Vector3(horizontal, 0, vertical));
        isMoving = movement.magnitude > 0.1f;
    }
    
    private Vector3 GetCameraRelativeMovement(Vector3 input)
    {
        if (Camera.main != null)
        {
            // Get camera's forward and right vectors (ignore Y for ground movement)
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Combine input with camera direction
            return (cameraForward * input.z) + (cameraRight * input.x);
        }
        
        return input; // Fallback to original input
    }
    
    private void HandleCombatInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }
    
    // Alternative HandleMovement method - Player faces camera direction
    private void HandleMovement()
    {
        if (isMoving)
        {
            Vector3 moveVelocity = movement * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
            
            Debug.Log($"Movement: {movement}, Current Rotation: {transform.rotation.eulerAngles}");
            
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                Debug.Log($"Target Rotation: {targetRotation.eulerAngles}");
                
                // Force the rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                
                Debug.Log($"New Rotation: {transform.rotation.eulerAngles}");
            }
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
        
        // Attack in the direction player is facing
        Vector3 attackDirection = transform.forward;
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + attackDirection * 1f, attackRange, enemyLayer);
        
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
        movement = GetCameraRelativeMovement(new Vector3(input.x, 0, input.y));
        isMoving = movement.magnitude > 0.1f;
    }
    
    public void OnAttackButtonPressed()
    {
        PerformAttack();
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, attackRange);
        
        // Draw movement direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, movement * 2f);
    }
}