using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0, 100)] public float mouseSensitivity = 50f;
    [Range(0f, 200f)] private float snappiness = 100f;
    [Range(0f, 20f)] public float walkSpeed = 3f;
    [Range(0f, 30f)] public float sprintSpeed = 5f;
    [Range(0f, 10f)] public float crouchSpeed = 1.5f;
    public float crouchHeight = 1f;
    public float crouchCameraHeight = 1f;
    public float slideSpeed = 8f;
    public float slideDuration = 0.7f;
    public float slideFovBoost = 5f;
    public float slideTiltAngle = 5f;
    [Range(0f, 15f)] public float jumpSpeed = 8f; // Increased for better feel
    [Range(0f, 50f)] public float gravity = 30f; // Increased gravity
    public bool coyoteTimeEnabled = true;
    [Range(0.01f, 0.3f)] public float coyoteTimeDuration = 0.2f;
    
    [Header("Visual Settings")]
    public float normalFov = 60f;
    public float sprintFov = 70f;
    public float fovChangeSpeed = 5f;
    public float walkingBobbingSpeed = 10f;
    public float bobbingAmount = 0.05f;
    private float sprintBobMultiplier = 1.5f;
    private float recoilReturnSpeed = 8f;
    
    [Header("Ability Toggles")]
    public bool canSlide = true;
    public bool canJump = true;
    public bool canSprint = true;
    public bool canCrouch = true;
    
    [Header("Collision Settings")]
    public QueryTriggerInteraction ceilingCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
    public QueryTriggerInteraction groundCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask = 1; // Default layer
    
    [Header("Camera References")]
    public Transform playerCamera;
    public Transform cameraParent;

    // Private variables
    private float rotX, rotY;
    private float xVelocity, yVelocity;
    private CharacterController characterController;
    private Vector3 velocity = Vector3.zero; // Renamed from moveDirection for clarity
    private bool isGrounded;
    private Vector2 moveInput;
    public bool isSprinting;
    public bool isCrouching;
    public bool isSliding;
    private float slideTimer;
    private float postSlideCrouchTimer;
    private Vector3 slideDirection;
    private float originalHeight;
    private float originalCameraParentHeight;
    private float coyoteTimer;
    private Camera cam;
    private AudioSource slideAudioSource;
    private float bobTimer;
    private float defaultPosY;
    private Vector3 recoil = Vector3.zero;
    private bool isLook = true, isMove = true;
    private float currentCameraHeight;
    private float currentBobOffset;
    private float currentFov;
    private float fovVelocity;
    private float currentSlideSpeed;
    private float slideSpeedVelocity;
    private float currentTiltAngle;
    private float tiltVelocity;

    public float CurrentCameraHeight => isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

    private void Awake()
    {
        // Get or add CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.3f;
            characterController.center = new Vector3(0, 1f, 0);
        }

        // Get camera components
        cam = playerCamera.GetComponent<Camera>();
        if (cam == null)
        {
            cam = playerCamera.gameObject.AddComponent<Camera>();
            playerCamera.gameObject.AddComponent<AudioListener>();
        }

        // Setup ground check if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheck = groundCheckObj.transform;
        }

        // Setup camera parent if not assigned
        if (cameraParent == null)
        {
            GameObject cameraParentObj = new GameObject("CameraParent");
            cameraParentObj.transform.SetParent(transform);
            cameraParentObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraParent = cameraParentObj.transform;
            
            // Make sure playerCamera is child of cameraParent
            if (playerCamera != null && playerCamera.parent != cameraParent)
            {
                playerCamera.SetParent(cameraParent);
                playerCamera.localPosition = Vector3.zero;
                playerCamera.localRotation = Quaternion.identity;
            }
        }

        originalHeight = characterController.height;
        originalCameraParentHeight = cameraParent.localPosition.y;
        defaultPosY = cameraParent.localPosition.y;
        
        slideAudioSource = gameObject.AddComponent<AudioSource>();
        slideAudioSource.playOnAwake = false;
        slideAudioSource.loop = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentCameraHeight = originalCameraParentHeight;
        currentBobOffset = 0f;
        currentFov = normalFov;
        currentSlideSpeed = 0f;
        currentTiltAngle = 0f;

        // Initialize rotation
        rotX = transform.rotation.eulerAngles.y;
        rotY = playerCamera.localRotation.eulerAngles.x;
        xVelocity = rotX;
        yVelocity = rotY;
    }

    private void Update()
    {
        HandleGroundDetection();
        HandleMouseLook();
        HandleHeadBob();
        HandleCrouchAndSlide();
        HandleMovementInput();
        
        // Apply gravity regardless of movement state
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        
        // Final movement application
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleGroundDetection()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, groundCheckQueryTriggerInteraction);
        
        if (isGrounded)
        {
            coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
            
            // Reset vertical velocity when grounded
            if (velocity.y < 0)
            {
                velocity.y = -2f; // Small negative value to stick to ground
            }
        }
        else if (coyoteTimeEnabled)
        {
            coyoteTimer -= Time.deltaTime;
        }
        
        // DEBUG: Visualize ground check
        Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, isGrounded ? Color.green : Color.red);
    }

    private void HandleMouseLook()
    {
        if (!isLook) return;

        float mouseX = Input.GetAxis("Mouse X") * 10 * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 10 * mouseSensitivity * Time.deltaTime;

        rotX += mouseX;
        rotY -= mouseY;
        rotY = Mathf.Clamp(rotY, -90f, 90f);

        xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
        yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

        float targetTiltAngle = isSliding ? slideTiltAngle : 0f;
        currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTiltAngle, ref tiltVelocity, 0.2f);
        
        playerCamera.localRotation = Quaternion.Euler(yVelocity, 0f, currentTiltAngle);
        transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
    }

    private void HandleCrouchAndSlide()
    {
        bool wantsToCrouch = canCrouch && Input.GetKey(KeyCode.LeftControl) && !isSliding;
        
        // Ceiling check
        Vector3 point1 = transform.position + characterController.center - Vector3.up * (characterController.height * 0.5f);
        Vector3 point2 = point1 + Vector3.up * characterController.height * 0.6f;
        float capsuleRadius = characterController.radius * 0.95f;
        float castDistance = isSliding ? originalHeight + 0.2f : originalHeight - crouchHeight + 0.2f;
        bool hasCeiling = Physics.CapsuleCast(point1, point2, capsuleRadius, Vector3.up, castDistance, groundMask, ceilingCheckQueryTriggerInteraction);
        
        if (isSliding)
        {
            postSlideCrouchTimer = 0.3f;
        }
        
        if (postSlideCrouchTimer > 0)
        {
            postSlideCrouchTimer -= Time.deltaTime;
            isCrouching = canCrouch;
        }
        else
        {
            isCrouching = canCrouch && (wantsToCrouch || (hasCeiling && !isSliding));
        }

        // Handle sliding
        if (canSlide && isSprinting && Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
        {
            isSliding = true;
            slideTimer = slideDuration;
            slideDirection = moveInput.magnitude > 0.1f ? (transform.right * moveInput.x + transform.forward * moveInput.y).normalized : transform.forward;
            currentSlideSpeed = sprintSpeed;
        }

        float slideProgress = slideTimer / slideDuration;
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f || !isGrounded)
            {
                isSliding = false;
            }
            float targetSlideSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideProgress);
            currentSlideSpeed = Mathf.SmoothDamp(currentSlideSpeed, targetSlideSpeed, ref slideSpeedVelocity, 0.2f);
            
            // Apply slide movement directly
            Vector3 slideVelocity = slideDirection * currentSlideSpeed;
            characterController.Move(slideVelocity * Time.deltaTime);
        }

        // Adjust character controller height
        float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * 10f);
        characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);

        // Handle FOV changes
        float targetFov = isSprinting ? sprintFov : (isSliding ? sprintFov + (slideFovBoost * Mathf.Lerp(0f, 1f, 1f - slideProgress)) : normalFov);
        currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
        cam.fieldOfView = currentFov;
    }

    private void HandleHeadBob()
    {
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        bool isMovingEnough = horizontalVelocity.magnitude > 0.1f;

        float targetBobOffset = isMovingEnough ? Mathf.Sin(bobTimer) * bobbingAmount : 0f;
        currentBobOffset = Mathf.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * walkingBobbingSpeed);

        if (!isGrounded || isSliding || isCrouching)
        {
            bobTimer = 0f;
            float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
            cameraParent.localPosition = new Vector3(
                cameraParent.localPosition.x,
                currentCameraHeight + currentBobOffset,
                cameraParent.localPosition.z);
            recoil = Vector3.zero;
            cameraParent.localRotation = Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil), recoilReturnSpeed * Time.deltaTime);
            return;
        }

        if (isMovingEnough)
        {
            float bobSpeed = walkingBobbingSpeed * (isSprinting ? sprintBobMultiplier : 1f);
            bobTimer += Time.deltaTime * bobSpeed;
            float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
            cameraParent.localPosition = new Vector3(
                cameraParent.localPosition.x,
                currentCameraHeight + currentBobOffset,
                cameraParent.localPosition.z);
            recoil.z = moveInput.x * -2f;
        }
        else
        {
            bobTimer = 0f;
            float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
            cameraParent.localPosition = new Vector3(
                cameraParent.localPosition.x,
                currentCameraHeight + currentBobOffset,
                cameraParent.localPosition.z);
            recoil = Vector3.zero;
        }

        cameraParent.localRotation = Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil), recoilReturnSpeed * Time.deltaTime);
    }

    private void HandleMovementInput()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        isSprinting = canSprint && Input.GetKey(KeyCode.LeftShift) && moveInput.y > 0.1f && isGrounded && !isCrouching && !isSliding;

        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        if (!isMove) currentSpeed = 0f;

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
        moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

        // Handle jumping
        if (canJump && Input.GetKeyDown(KeyCode.Space) && (isGrounded || coyoteTimer > 0f) && !isSliding)
        {
            velocity.y = jumpSpeed;
        }

        // Apply horizontal movement to velocity
        velocity.x = moveVector.x;
        velocity.z = moveVector.z;
        
        // DEBUG: Log velocity to see what's happening
        if (Time.frameCount % 30 == 0) // Log every half second
        {
            Debug.Log($"Player - Grounded: {isGrounded}, Velocity: {velocity}, Gravity: {gravity}");
        }
    }

    // Public methods for external control
    public void SetControl(bool newState)
    {
        SetLookControl(newState);
        SetMoveControl(newState);
    }

    public void SetLookControl(bool newState)
    {
        isLook = newState;
    }

    public void SetMoveControl(bool newState)
    {
        isMove = newState;
    }

    public void SetCursorVisibility(bool newVisibility)
    {
        Cursor.lockState = newVisibility ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = newVisibility;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}