using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -6); // Adjusted for smaller player
    public float smoothSpeed = 0.125f;
    
    [Header("Look Settings")]
    public float lookAtHeight = 2f;
    
    [Header("Collision Avoidance")]
    public LayerMask obstacleLayer = 1; // Default layer
    public float collisionOffset = 0.5f;
    
    void LateUpdate()
    {
        if (target != null)
        {
            // Calculate desired camera position
            Vector3 desiredPosition = target.position + offset;
            
            // Handle wall collision
            Vector3 adjustedPosition = HandleCameraCollision(desiredPosition, target.position);
            
            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, adjustedPosition, smoothSpeed);
            
            // Look at player (slightly above feet)
            Vector3 lookTarget = target.position + Vector3.up * lookAtHeight;
            transform.LookAt(lookTarget);
        }
    }
    
    private Vector3 HandleCameraCollision(Vector3 desiredPosition, Vector3 targetPosition)
    {
        Vector3 direction = desiredPosition - targetPosition;
        float distance = direction.magnitude;
        
        // Check for walls between camera and player
        if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayer))
        {
            // Move camera closer if it hits a wall
            return hit.point - direction.normalized * collisionOffset;
        }
        
        return desiredPosition;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}