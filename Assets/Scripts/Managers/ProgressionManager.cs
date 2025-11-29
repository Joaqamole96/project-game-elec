// -------------------------------------------------- //
// Scripts/Managers/ProgressionManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections;

/// <summary>
/// Manages progression between floors and scaling difficulty
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    [Header("Current Progress")]
    public int currentFloor = 1;
    public int totalFloorsCleared = 0;
    
    [Header("Difficulty Scaling")]
    public float enemyHealthScaling = 1.1f; // +10% per floor
    public float enemyDamageScaling = 1.05f; // +5% per floor
    public int goldRewardScaling = 10; // +10 gold per floor
    
    [Header("Exit Portal")]
    public GameObject exitPortalPrefab;
    private GameObject currentExitPortal;
    
    public static ProgressionManager Instance { get; private set; }
    
    // ------------------------- //
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Spawn exit portal after level generation
        StartCoroutine(SpawnExitPortalDelayed());
    }
    
    private IEnumerator SpawnExitPortalDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnExitPortal();
    }
    
    // ------------------------- //
    // PORTAL MANAGEMENT
    // ------------------------- //
    
    private void SpawnExitPortal()
    {
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager == null || layoutManager.CurrentLayout == null)
        {
            Debug.LogWarning("FloorProgression: Cannot spawn exit portal - no layout");
            return;
        }
        
        // Find exit room
        RoomModel exitRoom = layoutManager.CurrentLayout.Rooms.Find(r => r.Type == RoomType.Exit);
        if (exitRoom == null)
        {
            Debug.LogWarning("FloorProgression: No exit room found");
            return;
        }
        
        // Spawn portal at exit room center
        Vector2Int centerTile = exitRoom.Center;
        Vector3 portalPosition = new(centerTile.x + 0.5f, 1f, centerTile.y + 0.5f);
        
        // Load or create portal
        GameObject portalPrefab = exitPortalPrefab;
        if (portalPrefab == null)
        {
            portalPrefab = ResourceService.LoadLandmarkPrefab(RoomType.Exit);
        }
        
        if (portalPrefab != null)
        {
            currentExitPortal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);
            currentExitPortal.name = "ExitController";
            
            // Add portal trigger
            if (!currentExitPortal.GetComponent<ExitController>())
            {
                currentExitPortal.AddComponent<ExitController>();
            }
            
            Debug.Log($"FloorProgression: Exit portal spawned at {portalPosition}");
        }
        else
        {
            Debug.LogWarning("FloorProgression: No exit portal prefab available");
        }
    }
    
    // ------------------------- //
    // FLOOR TRANSITION
    // ------------------------- //
    
    public void OnPlayerEnterExitPortal()
    {
        Debug.Log($"FloorProgression: Player entered exit portal on floor {currentFloor}");
        
        StartCoroutine(TransitionToNextFloor());
    }
    
    private IEnumerator TransitionToNextFloor()
    {
        // Show transition effect
        Debug.Log("=== TRANSITIONING TO NEXT FLOOR ===");
        
        // TODO: Show loading screen
        yield return new WaitForSeconds(0.5f);
        
        // Clear current floor
        ClearCurrentFloor();
        yield return new WaitForSeconds(0.5f);
        
        // Increment floor
        currentFloor++;
        totalFloorsCleared++;
        
        Debug.Log($"=== NOW ON FLOOR {currentFloor} ===");
        
        // Generate next floor
        GameDirector.Instance?.NextFloor();
        
        yield return new WaitForSeconds(1f);

        EntityManager entityManager = GameDirector.Instance?.entityManager;
        if (entityManager != null)
        {
            entityManager.RespawnPlayerAtEntrance();
        }
        
        // Spawn new portal
        SpawnExitPortal();
        
        // TODO: Hide loading screen
    }
    
    private void ClearCurrentFloor()
    {
        Debug.Log("FloorProgression: Clearing current floor...");
        
        // Destroy exit portal
        if (currentExitPortal != null)
        {
            Destroy(currentExitPortal);
        }
        
        // Clear all enemies
        EntityManager entityManager = GameDirector.Instance?.entityManager;
        if (entityManager != null)
        {
            entityManager.ClearAllEnemies();
        }
        
        // Clear layout
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null)
        {
            layoutManager.ClearRendering();
        }
    }
    
    // ------------------------- //
    // DIFFICULTY SCALING
    // ------------------------- //
    
    public int GetScaledEnemyHealth(int baseHealth)
    {
        float scaledHealth = baseHealth * Mathf.Pow(enemyHealthScaling, currentFloor - 1);
        return Mathf.RoundToInt(scaledHealth);
    }
    
    public int GetScaledEnemyDamage(int baseDamage)
    {
        float scaledDamage = baseDamage * Mathf.Pow(enemyDamageScaling, currentFloor - 1);
        return Mathf.RoundToInt(scaledDamage);
    }
    
    public int GetScaledGoldReward(int baseReward)
    {
        return baseReward + (goldRewardScaling * (currentFloor - 1));
    }
    
    // ------------------------- //
    // SAVE/LOAD
    // ------------------------- //
    
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentFloor", currentFloor);
        PlayerPrefs.SetInt("TotalFloorsCleared", totalFloorsCleared);
        PlayerPrefs.Save();
        Debug.Log($"FloorProgression: Progress saved (Floor {currentFloor})");
    }
    
    public void LoadProgress()
    {
        currentFloor = PlayerPrefs.GetInt("CurrentFloor", 1);
        totalFloorsCleared = PlayerPrefs.GetInt("TotalFloorsCleared", 0);
        Debug.Log($"FloorProgression: Progress loaded (Floor {currentFloor})");
    }
    
    public void ResetProgress()
    {
        currentFloor = 1;
        totalFloorsCleared = 0;
        PlayerPrefs.DeleteKey("CurrentFloor");
        PlayerPrefs.DeleteKey("TotalFloorsCleared");
        Debug.Log("FloorProgression: Progress reset");
    }
}

