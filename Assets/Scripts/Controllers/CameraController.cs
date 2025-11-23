// -------------------------------------------------- //
// Scripts/Controllers/CameraController.cs
// -------------------------------------------------- //

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 thirdPersonOffset = new(0, 2, -3);
    public Vector3 firstPersonOffset = new(0, 0.5f, 0);
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 2f;
    public CameraMode currentMode = CameraMode.ThirdPerson;
    public float modeSwitchDistance = 1.5f;
    public LayerMask obstacleLayer = 1;
    public float collisionOffset = 0.3f;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    
    public enum CameraMode { FirstPerson, ThirdPerson }
    
    void Start()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            currentRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationX = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
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
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            currentRotationX -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentRotationY += Input.GetAxis("Mouse X") * rotationSpeed;
            
            // Clamp vertical rotation
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
        }
        
        // Optional: Q/E for rotation (alternative to mouse)
        if (Input.GetKey(KeyCode.Q)) currentRotationY -= rotationSpeed * 30f * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) currentRotationY += rotationSpeed * 30f * Time.deltaTime;
    }
    
    private void UpdateCameraMode()
    {
        if (currentMode == CameraMode.ThirdPerson)
        {
            // Check if camera is too close to player (colliding with walls)
            Vector3 desiredPosition = CalculateThirdPersonPosition();
            float distanceToPlayer = Vector3.Distance(desiredPosition, target.position);
            
            if (distanceToPlayer < modeSwitchDistance) currentMode = CameraMode.FirstPerson;
        }
        else
        {
            // Check if we can switch back to third person
            Vector3 desiredPosition = CalculateThirdPersonPosition();
            if (!IsCameraObstructed(desiredPosition)) currentMode = CameraMode.ThirdPerson;
        }
    }
    
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 desiredPosition;
        
        if (currentMode == CameraMode.ThirdPerson)
        {
            desiredPosition = target.position + rotation * thirdPersonOffset;
            desiredPosition = HandleCameraCollision(desiredPosition, target.position);
        }
        else desiredPosition = target.position + rotation * firstPersonOffset;
        
        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Look at target (slightly above for first person)

        transform.LookAt(currentMode == CameraMode.FirstPerson ?
            target.position + target.forward * 5f :
            target.position + Vector3.up * 1f);
    }
    
    private Vector3 CalculateThirdPersonPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        return target.position + rotation * thirdPersonOffset;
    }
    
    private bool IsCameraObstructed(Vector3 desiredPosition) => Physics.Linecast(target.position + Vector3.up, desiredPosition, obstacleLayer);
    
    private Vector3 HandleCameraCollision(Vector3 desiredPosition, Vector3 targetPosition)
    {
        Vector3 direction = desiredPosition - targetPosition;
        float distance = direction.magnitude;
        
        if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayer)) return hit.point - direction.normalized * collisionOffset;
        
        return desiredPosition;
    }
    
    public void SetTarget(Transform newTarget) => target = newTarget;
}