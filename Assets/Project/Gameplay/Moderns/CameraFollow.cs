using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -6);
    public float smoothSpeed = 0.125f;
    
    [Header("Collision Avoidance")]
    public LayerMask obstacleLayer = 1;
    public float collisionOffset = 0.5f;
    
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 adjustedPosition = HandleCameraCollision(desiredPosition, target.position);
            
            adjustedPosition.y = Mathf.Clamp(adjustedPosition.y, 5f, 50f);
            
            transform.position = Vector3.Lerp(transform.position, adjustedPosition, smoothSpeed);
            
            Vector3 lookTarget = target.position + Vector3.up * 1f;
            transform.LookAt(lookTarget);
        }
    }
    
    private Vector3 HandleCameraCollision(Vector3 desiredPosition, Vector3 targetPosition)
    {
        Vector3 direction = desiredPosition - targetPosition;
        float distance = direction.magnitude;
        
        if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayer))
        {
            return hit.point - direction.normalized * collisionOffset;
        }
        
        return desiredPosition;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}