// -------------------------------------------------- //
// Scripts/Controllers/PlayerController.cs
// -------------------------------------------------- //

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
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
    
    [Header("Configuration Settings")]
    public bool autoConfigureOnStart = true;
    public float capsuleHeight = 2f;
    public float capsuleRadius = 0.5f;
    public float mass = 1f;
    public float drag = 0f;
    public float angularDrag = 0f;
    
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
        // Configure player components first
        if (autoConfigureOnStart)
        {
            ConfigurePlayer();
        }
        
        CurrentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        // Ensure configuration is applied (in case autoConfigureOnStart is false)
        EnsureBasicConfiguration();
        
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
    
    // ===== PLAYER CONFIGURATION METHODS ===== //
    
    [ContextMenu("Configure Player")]
    public void ConfigurePlayer()
    {
        Debug.Log("Configuring player components...");
        
        ConfigureRigidbody();
        ConfigureCollider();
        ConfigureLayer();
        ConfigureTag();
        
        Debug.Log("Player configuration complete!");
    }
    
    private void ConfigureRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Critical settings for smooth movement
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // CRITICAL: Freeze rotation to prevent tipping over
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        Debug.Log("✓ Rigidbody configured");
    }
    
    private void ConfigureCollider()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
        }
        
        capsule.height = capsuleHeight;
        capsule.radius = capsuleRadius;
        capsule.center = new Vector3(0, capsuleHeight / 2f, 0);
        
        Debug.Log("✓ Collider configured");
    }
    
    private void ConfigureLayer()
    {
        // Try to set to Player layer if it exists
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            gameObject.layer = playerLayer;
            Debug.Log("✓ Layer set to 'Player'");
        }
        else
        {
            Debug.LogWarning("'Player' layer not found. Using Default layer.");
        }
    }
    
    private void ConfigureTag()
    {
        if (!CompareTag("Player"))
        {
            try
            {
                gameObject.tag = "Player";
                Debug.Log("✓ Tag set to 'Player'");
            }
            catch
            {
                Debug.LogWarning("'Player' tag not found. Please create it in Tags & Layers.");
            }
        }
    }
    
    private void EnsureBasicConfiguration()
    {
        // Ensure critical physics settings are applied even if auto-configure is off
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    
    [ContextMenu("Validate Configuration")]
    public void ValidateConfiguration()
    {
        Debug.Log("=== Player Configuration Validation ===");
        
        // Check Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"✓ Rigidbody: Mass={rb.mass}, UseGravity={rb.useGravity}, IsKinematic={rb.isKinematic}");
            Debug.Log($"  Constraints: {rb.constraints}");
            
            if (rb.constraints != RigidbodyConstraints.FreezeRotation)
            {
                Debug.LogWarning("⚠ Rigidbody rotation should be frozen!");
            }
        }
        else
        {
            Debug.LogError("✗ Missing Rigidbody!");
        }
        
        // Check Collider
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            Debug.Log($"✓ Collider: Height={collider.height}, Radius={collider.radius}");
        }
        else
        {
            Debug.LogError("✗ Missing CapsuleCollider!");
        }
        
        // Check Controller
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log($"✓ PlayerController: Speed={controller.moveSpeed}, Health={controller.maxHealth}");
        }
        else
        {
            Debug.LogError("✗ Missing PlayerController!");
        }
        
        // Check Tag
        if (CompareTag("Player"))
        {
            Debug.Log("✓ Tag: Player");
        }
        else
        {
            Debug.LogWarning($"⚠ Tag is '{tag}' but should be 'Player'");
        }
        
        // Check Layer
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
        
        Debug.Log("=== Validation Complete ===");
    }
    
    // ===== ORIGINAL PLAYERCONTROLLER METHODS ===== //
    
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