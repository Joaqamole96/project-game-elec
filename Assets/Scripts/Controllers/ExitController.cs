// -------------------------------------------------- //
// Scripts/Controllers/ExitController.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Exit portal that triggers floor transition
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitController : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public GameObject visualEffect;
    
    private bool hasBeenUsed = false;
    
    void Start()
    {
        // Ensure trigger collider exists
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 1.5f;
        }
    }
    
    void Update()
    {
        // Rotate portal for visual effect
        if (visualEffect != null)
        {
            visualEffect.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            hasBeenUsed = true;
            
            ProgressionManager progressionManager = ProgressionManager.Instance;
            if (progressionManager != null)
            {
                progressionManager.OnPlayerEnterExitPortal();
            }
            else
            {
                Debug.LogWarning("ExitController: ProgressionManager not found!");
            }
        }
    }
}