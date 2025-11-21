using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public bool isOpen = false;
    public DoorKey requiredKey = DoorKey.None;
    
    [Header("References")]
    public GameObject doorModel;
    public Collider blockingCollider;
    public Collider triggerCollider;
    
    void Start()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider>();
            
        if (doorModel == null && transform.childCount > 0)
            doorModel = transform.GetChild(0).gameObject;
            
        SetupTriggerCollider();
    }
    
    private void SetupTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
                return;
            }
        }
        
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
        }
        else
        {
            OpenDoor();
        }
    }
    
    private bool PlayerHasRequiredKey()
    {
        // Will be implemented with inventory system
        return true;
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
    
    public void LockDoor(DoorKey keyType)
    {
        isLocked = true;
        requiredKey = keyType;
        CloseDoor();
    }
    
    public void UnlockDoor()
    {
        isLocked = false;
        requiredKey = DoorKey.None;
    }
    
    public void Interact()
    {
        if (isOpen)
            CloseDoor();
        else
            TryOpenDoor();
    }
}