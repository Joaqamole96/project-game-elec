// -------------------------------------------------- //
// Scripts/Controllers/DoorController.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    [Header("Door State")]
    public bool isLocked = false;
    public bool isOpen = false;
    public KeyType requiredKey = KeyType.None;
    
    [Header("References")]
    public GameObject doorModel;
    public Collider blockingCollider;
    public Collider triggerCollider;
    
    [Header("Visual Feedback")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    private Renderer doorRenderer;
    private Color originalColor;
    
    void Start()
    {
        SetupComponents();
        UpdateVisuals();
    }
    
    private void SetupComponents()
    {
        // Find door model (child named "Door")
        if (doorModel == null)
        {
            Transform doorTransform = transform.Find("Door");
            if (doorTransform != null)
            {
                doorModel = doorTransform.gameObject;
            }
        }
        
        // Get renderer for visual feedback
        if (doorModel != null)
        {
            doorRenderer = doorModel.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                originalColor = doorRenderer.material.color;
            }
        }
        
        // Setup colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
            }
            else
            {
                blockingCollider = col;
            }
        }
        
        // If no trigger found, add one
        if (triggerCollider == null)
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2f, 2f, 2f);
            trigger.center = Vector3.zero;
            triggerCollider = trigger;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked && !isOpen)
        {
            OpenDoor();
        }
    }
    
    private void OpenDoor()
    {
        if (isOpen) return;
        
        isOpen = true;
        
        // Disable blocking collider
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }
        
        // Hide door model
        if (doorModel != null)
        {
            doorModel.SetActive(false);
        }
        
        Debug.Log("Door opened");
    }
    
    public void CloseDoor()
    {
        if (!isOpen) return;
        
        isOpen = false;
        
        // Enable blocking collider
        if (blockingCollider != null)
        {
            blockingCollider.enabled = true;
        }
        
        // Show door model
        if (doorModel != null)
        {
            doorModel.SetActive(true);
        }
        
        Debug.Log("Door closed");
    }
    
    public void LockDoor()
    {
        isLocked = true;
        CloseDoor();
        UpdateVisuals();
        Debug.Log("Door locked");
    }
    
    public void UnlockDoor()
    {
        isLocked = false;
        UpdateVisuals();
        Debug.Log("Door unlocked");
    }
    
    private void UpdateVisuals()
    {
        if (doorRenderer != null)
        {
            doorRenderer.material.color = isLocked ? lockedColor : originalColor;
        }
    }
}