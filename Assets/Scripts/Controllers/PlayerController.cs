// -------------------------------------------------- //
// Scripts/Controllers/PlayerController.cs (CLEAN)
// -------------------------------------------------- //

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    
    [Header("Combat")]
    public int maxHealth = 100;
    public int playerDamage = 15;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer = 1;

    [Header("Components")]
    public Rigidbody rb;
    public Animator animator;
    public WeaponController weaponManager;
    public PowerManager powerManager;
    public InventoryManager inventory;
    
    [Header("Visual")]
    public GameObject visualMesh;
    
    public static PlayerController Instance { get; private set; }
    public int CurrentHealth { get; private set; }
    
    private Vector3 moveDirection;
    private bool isMoving;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    private Camera mainCamera;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
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
        mainCamera = Camera.main;
        
        // Hide visual mesh in first-person
        if (visualMesh != null)
        {
            visualMesh.SetActive(false);
        }
        
        weaponManager = GetComponent<WeaponController>();
        if (weaponManager == null)
        {
            weaponManager = gameObject.AddComponent<WeaponController>();
        }

        powerManager = GetComponent<PowerManager>();
        if (powerManager == null)
        {
            powerManager = gameObject.AddComponent<PowerManager>();
        }
        
        inventory = GetComponent<InventoryManager>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<InventoryManager>();
        }
        
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.drag = 0f;
        rb.angularDrag = 0f;
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
        
        Vector3 inputVector = new(horizontal, 0, vertical);
        moveDirection = GetCameraRelativeMovement(inputVector);
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        isMoving = moveDirection.magnitude > 0.1f;
    }
    
    private Vector3 GetCameraRelativeMovement(Vector3 input)
    {
        if (input.magnitude < 0.01f) return Vector3.zero;
        if (mainCamera == null) return input;
        
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
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
            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
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
        
        // Use camera forward direction for attacks
        Vector3 attackDirection = mainCamera != null ? mainCamera.transform.forward : transform.forward;
        attackDirection.y = 0;
        attackDirection.Normalize();
        
        if (weaponManager != null && weaponManager.currentWeaponModel != null)
        {
            weaponManager.Attack(transform.position + Vector3.up, attackDirection);
        }
        else
        {
            PerformBasicMeleeAttack(attackDirection);
        }
    }
    
    public void PerformBasicMeleeAttack(Vector3 attackDirection)
    {
        Vector3 attackCenter = transform.position + attackDirection * (attackRange * 0.5f);
        
        Collider[] hitEnemies = Physics.OverlapSphere(attackCenter, attackRange, enemyLayer);
        
        foreach (Collider enemy in hitEnemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(attackDirection, dirToEnemy);
            
            if (angleToEnemy < 60f)
            {
                if (enemy.TryGetComponent<EnemyController>(out var enemyController))
                {
                    int finalDamage = playerDamage;
                    if (powerManager != null) finalDamage = powerManager.ModifyDamageDealt(finalDamage);

                    enemyController.TakeDamage(finalDamage);
                    if (powerManager != null) powerManager.OnDamageDealt(finalDamage);
                    
                    Debug.Log($"Hit enemy for {finalDamage} damage!");
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
        
        // Apply power damage reduction
        if (powerManager != null)
        {
            damage = powerManager.ModifyDamageTaken(damage);
        }
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        UIManager.Instance?.ShowDamageDisplay(transform.position + Vector3.up, damage, false, false);
        
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
        
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        
        Debug.Log("Player died - Game Over!");
        
        // PROPER: Let UIManager handle game over screen
        Invoke(nameof(TriggerGameOver), 2f);
    }

    private void TriggerGameOver()
    {
        if (UIManager.Instance != null)
        {
            LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
            int floorReached = layoutManager?.LevelConfig?.FloorLevel ?? 1;
            int goldCollected = inventory?.gold ?? 0;
            
            UIManager.Instance.ShowGameOver(false, floorReached, goldCollected, 0);
        }
        else
        {
            Debug.LogError("PlayerController: UIManager.Instance not found!");
        }
    }
    
    private void SpawnAtEntrance()
    {
        LayoutManager generator = FindObjectOfType<LayoutManager>();
        if (generator != null && generator.CurrentLayout != null)
        {
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            transform.position = spawnPosition;
            
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
    
    // Mobile controls interface
    public void SetMovementInput(Vector2 input)
    {
        Vector3 inputVector = new(input.x, 0, input.y);
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
    
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;
        
        // Draw attack range in camera direction
        Vector3 attackDirection = mainCamera.transform.forward;
        attackDirection.y = 0;
        attackDirection.Normalize();
        
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + attackDirection * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(attackCenter, attackRange);
        
        // Draw camera forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up, attackDirection * 3f);
    }
}