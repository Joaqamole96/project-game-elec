// -------------------------------------------------- //
// Scripts/Controllers/DoorController.cs (ENHANCED)
// -------------------------------------------------- //

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    public bool isLocked = false;
    public bool isOpen = false;
    public KeyType requiredKey = KeyType.None;
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
        if (blockingCollider == null) blockingCollider = GetComponent<Collider>();
            
        if (doorModel == null && transform.childCount > 0) doorModel = transform.GetChild(0).gameObject;
        
        // Get renderer for visual feedback
        if (doorModel != null)
        {
            doorRenderer = doorModel.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                originalColor = doorRenderer.material.color;
            }
        }
            
        SetupTriggerCollider();
        UpdateVisuals();
    }
    
    private void SetupTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider col in colliders)
            if (col.isTrigger)
            {
                triggerCollider = col;
                return;
            }
        
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 2f, 1.5f);
        trigger.center = Vector3.zero;
        triggerCollider = trigger;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TryOpenDoor();
    }
    
    public void TryOpenDoor()
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            // TODO: Play locked sound
            return;
        }
        
        if (requiredKey != KeyType.None && !PlayerHasRequiredKey())
        {
            Debug.Log($"Door requires {requiredKey}!");
            // TODO: Play locked sound
            return;
        }
        
        OpenDoor();
    }
    
    private bool PlayerHasRequiredKey()
    {
        // TODO: Check player inventory
        return false;
    }
    
    private void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            
            if (blockingCollider != null) blockingCollider.enabled = false;
                
            if (doorModel != null) doorModel.SetActive(false);
            
            Debug.Log("Door opened");
        }
    }
    
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            
            if (blockingCollider != null) blockingCollider.enabled = true;
                
            if (doorModel != null) doorModel.SetActive(true);
            
            Debug.Log("Door closed");
        }
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
            if (isLocked)
            {
                doorRenderer.material.color = lockedColor;
            }
            else
            {
                doorRenderer.material.color = originalColor;
            }
        }
    }
}