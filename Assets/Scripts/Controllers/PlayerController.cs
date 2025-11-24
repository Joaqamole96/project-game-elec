// -------------------------------------------------- //
// Scripts/Controllers/PlayerController.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f; // Increased for snappier rotation
    
    [Header("Combat")]
    public int maxHealth = 100;
    public int playerDamage = 15;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer = 1;

    [Header("Components")]
    public Rigidbody rb;
    public Animator animator;
    public WeaponManager weaponManager;
    public InventoryManager inventory;
    
    public static PlayerController Instance { get; private set; }
    public int CurrentHealth { get; private set; }
    
    private Vector3 movement;
    private Vector3 moveDirection; // CAMERA-RELATIVE direction
    private bool isMoving;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    
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
            return;
        }
    }
    
    void Start()
    {
        CurrentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        // Get or add weapon manager
        weaponManager = GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            weaponManager = gameObject.AddComponent<WeaponManager>();
        }
        
        // Get or add inventory
        inventory = GetComponent<InventoryManager>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<InventoryManager>();
        }
        
        // CRITICAL: Lock rotation to prevent physics from rotating player
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // Reduce drag for smoother movement
        rb.drag = 0f;
        rb.angularDrag = 0f;
        
        // Better physics settings
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        SpawnAtEntrance();
    }
    
    void Update()
    {
        if (isDead) return;
        
        HandleInput();
        HandleCombatInput();
        UpdateAnimations();
        UpdateRoomDetection();
    }
    
    void FixedUpdate()
    {
        if (isDead) return;
        HandleMovement();
    }
    
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Get RAW input vector (not normalized yet)
        Vector3 inputVector = new Vector3(horizontal, 0, vertical);
        
        // Get camera-relative movement direction
        moveDirection = GetCameraRelativeMovement(inputVector);
        
        // Normalize AFTER camera transformation to maintain consistent speed
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        isMoving = moveDirection.magnitude > 0.1f;
    }
    
    private Vector3 GetCameraRelativeMovement(Vector3 input)
    {
        if (input.magnitude < 0.01f) return Vector3.zero;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return input; // Fallback
        
        // Get camera's forward and right vectors (flatten to ground plane)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        // CRITICAL: Project onto horizontal plane
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // CRITICAL: Normalize BEFORE using them
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Calculate movement direction relative to camera
        Vector3 result = (cameraForward * input.z) + (cameraRight * input.x);
        
        return result;
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
        if (isMoving && moveDirection.magnitude > 0.01f)
        {
            // Apply velocity in the EXACT direction of moveDirection
            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            
            // Rotate player to face movement direction INSTANTLY
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // Stop horizontal movement but keep vertical velocity (gravity)
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
        
        // Use weapon manager if available
        if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            weaponManager.Attack(transform.position + Vector3.up, transform.forward);
        }
        else
        {
            // Fallback: basic melee attack
            PerformBasicMeleeAttack();
        }
    }
    
    private void PerformBasicMeleeAttack()
    {
        // Attack in a cone in front of player
        Vector3 attackDirection = transform.forward;
        Vector3 attackCenter = transform.position + attackDirection * (attackRange * 0.5f);
        
        Collider[] hitEnemies = Physics.OverlapSphere(attackCenter, attackRange, enemyLayer);
        
        foreach (Collider enemy in hitEnemies)
        {
            // Check if enemy is in front of player (within 120 degree cone)
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(transform.forward, dirToEnemy);
            
            if (angleToEnemy < 60f) // 120 degree cone (60 degrees each side)
            {
                if (enemy.TryGetComponent<EnemyController>(out var enemyController))
                {
                    enemyController.TakeDamage(playerDamage);
                    Debug.Log($"Hit enemy for {playerDamage} damage!");
                }
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveSpeed", moveDirection.magnitude);
        }
    }
    
    private void UpdateRoomDetection()
    {
        // Only check every 30 frames for performance
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
        if (isDead) return;
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
        
        Debug.Log($"Player took {damage} damage! Health: {CurrentHealth}/{maxHealth}");
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}! Health: {CurrentHealth}/{maxHealth}");
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Stop movement
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        
        Debug.Log("Player died - Game Over!");
        
        // Trigger game over after delay
        Invoke(nameof(TriggerGameOver), 2f);
    }
    
    private void TriggerGameOver()
    {
        // TODO: Show game over UI
        Debug.Log("GAME OVER - Restarting level...");
        
        // For now, just reload the scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        // );
    }
    
    private void SpawnAtEntrance()
    {
        LayoutManager generator = FindObjectOfType<LayoutManager>();
        if (generator != null && generator.CurrentLayout != null)
        {
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            transform.position = spawnPosition;
            
            // Ensure player is on NavMesh (if exists)
            NavMeshGenerator navMeshGen = generator.GetComponent<NavMeshGenerator>();
            if (navMeshGen != null && navMeshGen.IsPositionOnNavMesh(spawnPosition))
            {
                transform.position = navMeshGen.GetNearestNavMeshPosition(spawnPosition);
            }
            
            Debug.Log($"Player spawned at: {transform.position}");
        }
        else
        {
            Debug.LogWarning("Could not find entrance room for spawn");
        }
    }
    
    // ===== MOBILE CONTROLS INTERFACE ===== //
    public void SetMovementInput(Vector2 input)
    {
        Vector3 inputVector = new Vector3(input.x, 0, input.y);
        moveDirection = GetCameraRelativeMovement(inputVector);
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        isMoving = moveDirection.magnitude > 0.1f;
    }
    
    public void OnAttackButtonPressed()
    {
        PerformAttack();
    }
    
    // ===== DEBUG VISUALIZATION ===== //
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(attackCenter, attackRange);
        
        // Draw movement direction
        if (isMoving)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, movement * 2f);
        }
        
        // Draw forward direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);
    }
}