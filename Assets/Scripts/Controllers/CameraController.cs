// -------------------------------------------------- //
// Scripts/Controllers/CameraController.cs
// -------------------------------------------------- //

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Third Person Settings")]
    public Vector3 thirdPersonOffset = new(0, 2, -3);
    public float smoothSpeed = 0.2f;
    public float rotationSpeed = 2f;
    
    [Header("First Person Settings")]
    public Vector3 firstPersonOffset = new(0, 0.5f, 0);
    public float firstPersonMouseSensitivity = 3f;
    public KeyCode firstPersonToggleKey = KeyCode.V;
    
    [Header("Collision & Tight Spaces")]
    public LayerMask obstacleLayer = 1;
    public float collisionOffset = 0.3f;
    public float collisionCheckRadius = 0.2f;
    public float tightSpaceThreshold = 1.0f;
    public float openSpaceThreshold = 3.0f;
    
    [Header("Current State")]
    public CameraMode currentMode = CameraMode.ThirdPerson;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float targetDistance;
    private bool manualFirstPerson = false;
    
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
        if (Input.GetKeyDown(firstPersonToggleKey))
        {
            manualFirstPerson = !manualFirstPerson;
            currentMode = manualFirstPerson ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            Debug.Log($"Manual camera mode: {currentMode}");
        }
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        if (currentMode == CameraMode.ThirdPerson)
        {
            currentRotationY += mouseX * rotationSpeed;
            currentRotationX -= mouseY * rotationSpeed;
            
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
        }
        else
        {
            currentRotationY += mouseX * firstPersonMouseSensitivity;
            currentRotationX -= mouseY * firstPersonMouseSensitivity;
            
            // Clamp vertical rotation
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
        }
    }
    
    private void UpdateCameraMode()
    {
        if (manualFirstPerson) return;
        
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 idealPosition = target.position + rotation * thirdPersonOffset;
        
        Vector3 adjustedPosition = HandleCameraCollision(idealPosition, target.position, rotation);
        float actualDistance = Vector3.Distance(adjustedPosition, target.position);
        
        bool inTightSpace = IsInTightSpace(target.position);
        
        if (currentMode == CameraMode.ThirdPerson)
            if (inTightSpace && actualDistance < tightSpaceThreshold)
            {
                currentMode = CameraMode.FirstPerson;
                Debug.Log("Switched to First Person (tight space detected)");
            }
        else if (currentMode == CameraMode.FirstPerson)
            if (!inTightSpace && actualDistance > openSpaceThreshold)
            {
                currentMode = CameraMode.ThirdPerson;
                Debug.Log("Switched to Third Person (open space)");
            }
    }
    
    private bool IsInTightSpace(Vector3 position)
    {
        float checkDistance = 2f;
        int wallsNearby = 0;
        
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        
        foreach (var dir in directions)
            if (Physics.Raycast(position + Vector3.up, dir, checkDistance, obstacleLayer)) wallsNearby++;
        
        return wallsNearby >= 2;
    }
    
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 desiredPosition;
        Vector3 lookAtTarget;
        
        if (currentMode == CameraMode.ThirdPerson)
        {
            Vector3 idealPosition = target.position + rotation * thirdPersonOffset;
            
            desiredPosition = HandleCameraCollision(idealPosition, target.position, rotation);
            
            lookAtTarget = target.position + Vector3.up * 1f;
        }
        else
        {
            desiredPosition = target.position + Vector3.up * firstPersonOffset.y;
            
            Vector3 forwardDir = rotation * Vector3.forward;
            lookAtTarget = desiredPosition + forwardDir * 10f;
        }
        
        float positionSmoothing = smoothSpeed * 60f * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothing);
        
        Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget - transform.position);
        float rotationSmoothing = smoothSpeed * 60f * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing);
    }
    
    private Vector3 HandleCameraCollision(Vector3 idealPosition, Vector3 targetPosition, Quaternion rotation)
    {
        Vector3 direction = idealPosition - targetPosition;
        float idealDistance = direction.magnitude;
        
        if (Physics.SphereCast(
            targetPosition + Vector3.up * 0.5f, 
            collisionCheckRadius, 
            direction.normalized, 
            out RaycastHit hit, 
            idealDistance, 
            obstacleLayer))
        {
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
            Vector3 direction = (newTarget.position - transform.position).normalized;
            currentRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationX = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}