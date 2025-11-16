using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public bool isOpen = false; // Made public for debugging
    public KeyType requiredKey = KeyType.None;
    
    [Header("References")]
    public GameObject doorModel;
    public Collider blockingCollider;
    public Collider triggerCollider;
    
    void Start()
    {
        // Ensure we have references
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider>();
            
        if (doorModel == null)
            doorModel = transform.GetChild(0).gameObject;
            
        SetupTriggerCollider();
    }
    
    private void SetupTriggerCollider()
    {
        // Check if we already have a trigger collider
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
                return;
            }
        }
        
        // Create a trigger collider if none exists
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 2f, 1.5f);
        trigger.center = Vector3.zero;
        triggerCollider = trigger;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryOpenDoor();
        }
    }
    
    public void TryOpenDoor()
    {
        if (isLocked)
        {
            if (PlayerHasRequiredKey())
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("Door is locked! Need key: " + requiredKey);
            }
        }
        else
        {
            OpenDoor();
        }
    }
    
    private bool PlayerHasRequiredKey()
    {
        return true; // Temporary - we'll implement inventory later
    }
    
    private void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            
            if (blockingCollider != null)
                blockingCollider.enabled = false;
                
            if (doorModel != null)
                doorModel.SetActive(false);
        }
    }
    
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            
            if (blockingCollider != null)
                blockingCollider.enabled = true;
                
            if (doorModel != null)
                doorModel.SetActive(true);
        }
    }
    
    public void LockDoor(KeyType keyType)
    {
        isLocked = true;
        requiredKey = keyType;
        CloseDoor();
    }
    
    public void UnlockDoor()
    {
        isLocked = false;
        requiredKey = KeyType.None;
    }
}