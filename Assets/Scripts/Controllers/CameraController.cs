// -------------------------------------------------- //
// Scripts/Controllers/CameraController.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Third Person Settings")]
    public Vector3 thirdPersonOffset = new(0, 2, -3);
    public float smoothSpeed = 0.2f; // Increased default for less jitter
    public float rotationSpeed = 2f;
    
    [Header("First Person Settings")]
    public Vector3 firstPersonOffset = new(0, 0.5f, 0);
    public float firstPersonMouseSensitivity = 3f;
    public KeyCode firstPersonToggleKey = KeyCode.V;
    
    [Header("Collision & Tight Spaces")]
    public LayerMask obstacleLayer = 1;
    public float collisionOffset = 0.3f;
    public float collisionCheckRadius = 0.2f;
    public float tightSpaceThreshold = 1.0f; // Distance to trigger first-person
    public float openSpaceThreshold = 3.0f;  // Distance to exit first-person
    
    [Header("Current State")]
    public CameraMode currentMode = CameraMode.ThirdPerson;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float targetDistance;
    private bool manualFirstPerson = false; // Toggled by key press
    
    public enum CameraMode { FirstPerson, ThirdPerson }
    
    void Start()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            currentRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationX = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
        
        targetDistance = thirdPersonOffset.magnitude;
        
        // Lock and hide cursor for better gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraMode();
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        // Toggle first-person mode with key
        if (Input.GetKeyDown(firstPersonToggleKey))
        {
            manualFirstPerson = !manualFirstPerson;
            currentMode = manualFirstPerson ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            Debug.Log($"Manual camera mode: {currentMode}");
        }
        
        // Mouse input for camera rotation (ALWAYS ACTIVE in both modes)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        if (currentMode == CameraMode.ThirdPerson)
        {
            // Third person: Only rotate with RIGHT MOUSE or always if no button check
            // For better FPS-like feel, REMOVE the button check:
            currentRotationY += mouseX * rotationSpeed;
            currentRotationX -= mouseY * rotationSpeed;
            
            // Clamp vertical rotation
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
            
            // Optional: Q/E for rotation (can remove if not needed)
            if (Input.GetKey(KeyCode.Q)) currentRotationY -= rotationSpeed * 30f * Time.deltaTime;
            if (Input.GetKey(KeyCode.E)) currentRotationY += rotationSpeed * 30f * Time.deltaTime;
        }
        else
        {
            // First person: FREE MOUSE LOOK (like FPS games)
            currentRotationY += mouseX * firstPersonMouseSensitivity;
            currentRotationX -= mouseY * firstPersonMouseSensitivity;
            
            // Clamp vertical rotation
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
        }
    }
    
    private void UpdateCameraMode()
    {
        // Don't auto-switch if manually toggled
        if (manualFirstPerson) return;
        
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 idealPosition = target.position + rotation * thirdPersonOffset;
        
        // Calculate distance considering collision
        Vector3 adjustedPosition = HandleCameraCollision(idealPosition, target.position, rotation);
        float actualDistance = Vector3.Distance(adjustedPosition, target.position);
        
        // Check if in tight space (walls very close on multiple sides)
        bool inTightSpace = IsInTightSpace(target.position);
        
        // Switch to first person ONLY if in genuinely tight space
        if (currentMode == CameraMode.ThirdPerson)
        {
            if (inTightSpace && actualDistance < tightSpaceThreshold)
            {
                currentMode = CameraMode.FirstPerson;
                Debug.Log("Switched to First Person (tight space detected)");
            }
        }
        // Switch back to third person when in open space
        else if (currentMode == CameraMode.FirstPerson)
        {
            if (!inTightSpace && actualDistance > openSpaceThreshold)
            {
                currentMode = CameraMode.ThirdPerson;
                Debug.Log("Switched to Third Person (open space)");
            }
        }
    }
    
    /// <summary>
    /// Checks if player is in a tight space (walls close on multiple sides)
    /// </summary>
    private bool IsInTightSpace(Vector3 position)
    {
        float checkDistance = 2f;
        int wallsNearby = 0;
        
        // Check in 4 cardinal directions
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        
        foreach (var dir in directions)
        {
            if (Physics.Raycast(position + Vector3.up, dir, checkDistance, obstacleLayer))
            {
                wallsNearby++;
            }
        }
        
        // Tight space = walls on at least 2 sides
        return wallsNearby >= 2;
    }
    
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 desiredPosition;
        Vector3 lookAtTarget;
        
        if (currentMode == CameraMode.ThirdPerson)
        {
            // Calculate ideal third person position
            Vector3 idealPosition = target.position + rotation * thirdPersonOffset;
            
            // Handle collision by pulling camera closer
            desiredPosition = HandleCameraCollision(idealPosition, target.position, rotation);
            
            // Look slightly above player center
            lookAtTarget = target.position + Vector3.up * 1f;
        }
        else
        {
            // First person: position at head height
            desiredPosition = target.position + Vector3.up * firstPersonOffset.y;
            
            // Look in the direction of camera rotation
            Vector3 forwardDir = rotation * Vector3.forward;
            lookAtTarget = desiredPosition + forwardDir * 10f;
        }
        
        // Smooth position transition to reduce jitter
        float positionSmoothing = smoothSpeed * 60f * Time.deltaTime; // Frame-rate independent
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothing);
        
        // Smooth rotation to reduce jitter
        Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget - transform.position);
        float rotationSmoothing = smoothSpeed * 60f * Time.deltaTime; // Frame-rate independent
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing);
    }
    
    private Vector3 HandleCameraCollision(Vector3 idealPosition, Vector3 targetPosition, Quaternion rotation)
    {
        Vector3 direction = idealPosition - targetPosition;
        float idealDistance = direction.magnitude;
        
        // Cast from player to ideal camera position
        if (Physics.SphereCast(
            targetPosition + Vector3.up * 0.5f, 
            collisionCheckRadius, 
            direction.normalized, 
            out RaycastHit hit, 
            idealDistance, 
            obstacleLayer))
        {
            // Pull camera closer, but not too close
            float safeDistance = Mathf.Max(hit.distance - collisionOffset, 0.5f);
            return targetPosition + direction.normalized * safeDistance;
        }
        
        return idealPosition;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (newTarget != null)
        {
            // Initialize camera behind player
            Vector3 direction = (newTarget.position - transform.position).normalized;
            currentRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationX = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
    }
    
    // Unlock cursor when application loses focus
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}