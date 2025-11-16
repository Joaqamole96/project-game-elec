using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Combat")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public GameObject weapon;
    
    private Vector3 movement;
    private bool isMoving;
    
    // Singleton for easy access
    public static PlayerController Instance { get; private set; }
    
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
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        // Spawn at dungeon entrance
        SpawnAtEntrance();
    }
    
    void Update()
    {
        HandleInput();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleInput()
    {
        // Keyboard input (we'll add touch later)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        movement = new Vector3(horizontal, 0, vertical).normalized;
        isMoving = movement.magnitude > 0.1f;
        
        // Simple attack input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformAttack();
        }
    }
    
    private void HandleMovement()
    {
        if (isMoving)
        {
            // Move the player
            Vector3 moveVelocity = movement * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
            
            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Stop movement but maintain Y velocity for gravity
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
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
    
    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Simple attack logic - we'll expand this later
        Debug.Log("Player attacked!");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        // We'll add respawn/Game Over logic later
        gameObject.SetActive(false);
    }
    
    private void SpawnAtEntrance()
    {
        DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
        if (generator != null)
        {
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            transform.position = spawnPosition;
            Debug.Log($"Player spawned at: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("DungeonGenerator not found! Spawning at default position.");
            transform.position = new Vector3(0, 1, 0);
        }
    }
    
    // For mobile controls (we'll implement tomorrow)
    public void SetMovementInput(Vector2 input)
    {
        movement = new Vector3(input.x, 0, input.y);
        isMoving = movement.magnitude > 0.1f;
    }
    
    public void OnAttackButtonPressed()
    {
        PerformAttack();
    }
}