using UnityEngine;

// ============================================
// EntranceController.cs
// ============================================
public class EntranceController : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem portalEffect;
    public Light portalLight;
    
    void Start()
    {
        Debug.Log("Entrance portal initialized");
        
        if (portalEffect != null)
            portalEffect.Play();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player returned to entrance");
            // Future: Could teleport to previous floor or hub
        }
    }
}

// ============================================
// ExitController.cs
// ============================================
public class ExitController : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem portalEffect;
    public Light portalLight;
    
    [Header("Settings")]
    public bool isLocked = true;
    
    void Start()
    {
        Debug.Log("Exit portal initialized");
        UpdateVisuals();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            Debug.Log("Player entering exit portal - loading next floor");
            LoadNextFloor();
        }
        else if (isLocked)
        {
            Debug.Log("Exit is locked - clear all rooms first!");
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        UpdateVisuals();
        Debug.Log("Exit portal unlocked!");
    }
    
    private void UpdateVisuals()
    {
        if (portalEffect != null)
        {
            if (isLocked)
                portalEffect.Stop();
            else
                portalEffect.Play();
        }
    }
    
    private void LoadNextFloor()
    {
        var dungeonManager = FindObjectOfType<DungeonManager>();
        if (dungeonManager != null)
        {
            dungeonManager.GenerateNextFloor();
        }
    }
}

// ============================================
// ShopController.cs
// ============================================
public class ShopController : MonoBehaviour
{
    [Header("Shop Settings")]
    public int itemCount = 3;
    public GameObject shopUIPanel;
    
    private bool playerInRange = false;
    
    void Start()
    {
        Debug.Log("Shop initialized");
        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Press E to open shop");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUIPanel != null && shopUIPanel.activeSelf)
                shopUIPanel.SetActive(false);
        }
    }
    
    private void ToggleShop()
    {
        if (shopUIPanel != null)
        {
            shopUIPanel.SetActive(!shopUIPanel.activeSelf);
            Debug.Log(shopUIPanel.activeSelf ? "Shop opened" : "Shop closed");
        }
    }
}

// ============================================
// TreasureController.cs
// ============================================
public class TreasureController : MonoBehaviour
{
    [Header("Treasure Settings")]
    public bool isOpened = false;
    public GameObject treasureChest;
    public GameObject glowEffect;
    
    void Start()
    {
        Debug.Log("Treasure initialized");
        UpdateVisuals();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            OpenTreasure();
        }
    }
    
    private void OpenTreasure()
    {
        isOpened = true;
        Debug.Log("Treasure opened! Awarding loot...");
        
        // TODO: Grant random item/weapon/upgrade
        
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (glowEffect != null)
            glowEffect.SetActive(!isOpened);
            
        // Animate chest opening if animator available
        var animator = GetComponent<Animator>();
        if (animator != null && isOpened)
            animator.SetTrigger("Open");
    }
}

// ============================================
// BossRoomController.cs
// ============================================
public class BossRoomController : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    
    [Header("Room Settings")]
    public bool bossSpawned = false;
    public bool bossDefeated = false;
    
    private RoomModel associatedRoom;
    
    void Start()
    {
        Debug.Log("Boss room initialized");
        
        if (bossSpawnPoint == null)
            bossSpawnPoint = transform;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !bossSpawned && !bossDefeated)
        {
            SpawnBoss();
        }
    }
    
    private void SpawnBoss()
    {
        bossSpawned = true;
        Debug.Log("Boss fight starting!");
        
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            var boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            Debug.Log($"Boss spawned at {bossSpawnPoint.position}");
            
            // TODO: Lock room doors
        }
        else
        {
            Debug.LogWarning("Boss prefab or spawn point not assigned!");
        }
    }
    
    public void OnBossDefeated()
    {
        bossDefeated = true;
        Debug.Log("Boss defeated! Room cleared.");
        
        // TODO: Unlock room doors
        // TODO: Unlock exit portal
        
        var exitController = FindObjectOfType<ExitController>();
        if (exitController != null)
            exitController.Unlock();
    }
}