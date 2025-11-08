using UnityEngine;

public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float WalkSpeed = 5f;
    public float SprintSpeed = 8f;
    public float JumpForce = 7f;
    public float AirControl = 0.5f;
    
    [Header("Mouse Look Settings")]
    public float MouseSensitivity = 2f;
    public float MaxLookAngle = 90f;
    
    [Header("First Person Camera")]
    public Camera PlayerCamera;
    public float CameraHeight = 0.8f;
    
    [Header("Ground Detection")]
    public LayerMask GroundMask = 1;
    public float GroundCheckDistance = 0.2f;
    public Transform GroundCheckPoint;
    
    private Rigidbody _rb;
    private bool _isGrounded;
    
    // Mouse look variables
    private float _xRotation = 0f;
    
    // Input caching
    private float _currentSpeed;
    private bool _isSprinting = false;

    // Movement
    private Vector3 _moveDirection;
    private float _horizontal;
    private float _vertical;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.mass = 1f;
            _rb.drag = 5f; // Increased drag for better stopping
            _rb.angularDrag = 0f;
        }
        
        // Create ground check point if missing
        if (GroundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.9f, 0);
            GroundCheckPoint = groundCheck.transform;
        }
        
        // Setup camera
        if (PlayerCamera == null)
        {
            PlayerCamera = GetComponentInChildren<Camera>();
            if (PlayerCamera == null)
            {
                GameObject cameraObj = new GameObject("FirstPersonCamera");
                cameraObj.transform.SetParent(transform);
                PlayerCamera = cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
            }
        }
        
        // Position camera at eye level
        PlayerCamera.transform.localPosition = new Vector3(0, CameraHeight, 0);
        PlayerCamera.transform.localRotation = Quaternion.identity;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _currentSpeed = WalkSpeed;
    }

    void Update()
    {
        HandleInput();
        HandleMouseLook();
        HandleGroundDetection();
        HandleJump();
        HandleSprint();
        HandleCursor();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        // Get input in Update for responsiveness
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction relative to player's orientation
        _moveDirection = (transform.right * _horizontal + transform.forward * _vertical).normalized;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -MaxLookAngle, MaxLookAngle);
        
        PlayerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    private void HandleGroundDetection()
    {
        // Raycast for ground detection
        RaycastHit hit;
        _isGrounded = Physics.Raycast(
            GroundCheckPoint.position, 
            Vector3.down, 
            out hit, 
            GroundCheckDistance, 
            GroundMask
        );

        // Debug ground detection
        Debug.DrawRay(GroundCheckPoint.position, Vector3.down * GroundCheckDistance, _isGrounded ? Color.green : Color.red);
    }

    private void HandleMovement()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            // Calculate target velocity
            Vector3 targetVelocity = _moveDirection * _currentSpeed;
            
            // Only control horizontal movement
            Vector3 currentVelocity = _rb.velocity;
            Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);
            
            // Apply less force in air for air control
            float forceMultiplier = _isGrounded ? 1f : AirControl;
            
            // Apply force
            _rb.AddForce(velocityChange * forceMultiplier, ForceMode.VelocityChange);
        }
        
        // Limit maximum horizontal speed
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (horizontalVelocity.magnitude > _currentSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * _currentSpeed;
            _rb.velocity = new Vector3(horizontalVelocity.x, _rb.velocity.y, horizontalVelocity.z);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // Reset vertical velocity
            _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    private void HandleSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isSprinting = true;
            _currentSpeed = SprintSpeed;
        }
        
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _isSprinting = false;
            _currentSpeed = WalkSpeed;
        }
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (GroundCheckPoint != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GroundCheckPoint.position, 0.1f);
            Gizmos.DrawLine(GroundCheckPoint.position, GroundCheckPoint.position + Vector3.down * GroundCheckDistance);
        }
    }
}