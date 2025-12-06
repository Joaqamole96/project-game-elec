// ================================================== //
// ProgressionManager - FIXED VERSION
// ================================================== //

using UnityEngine;
using System.Collections;

public class ProgressionManager : MonoBehaviour
{
    [Header("Current Progress")]
    public int currentFloor = 1;
    public int totalFloorsCleared = 0;
    
    [Header("Difficulty Scaling")]
    public float enemyHealthScaling = 1.1f;
    public float enemyDamageScaling = 1.05f;
    public int goldRewardScaling = 10;
    
    [Header("Exit Portal")]
    public GameObject exitPortalPrefab;
    private GameObject currentExitPortal;
    
    public static ProgressionManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ProgressionManager: Instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Spawn portal after delay
        StartCoroutine(SpawnExitPortalDelayed());
    }
    
    private IEnumerator SpawnExitPortalDelayed()
    {
        yield return new WaitForSeconds(2f);
        SpawnExitPortal();
    }
    
    // ==========================================
    // PORTAL MANAGEMENT
    // ==========================================
    
    private void SpawnExitPortal()
    {
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager == null || layoutManager.CurrentLayout == null)
        {
            Debug.LogWarning("ProgressionManager: Cannot spawn portal - no layout");
            return;
        }
        
        // Find exit room
        RoomModel exitRoom = layoutManager.CurrentLayout.Rooms.Find(r => r.Type == RoomType.Exit);
        if (exitRoom == null)
        {
            Debug.LogWarning("ProgressionManager: No exit room found");
            return;
        }
        
        // Check if portal already exists
        if (currentExitPortal != null)
        {
            Debug.Log("ProgressionManager: Exit portal already exists");
            return;
        }
        
        // Spawn at exit room center
        Vector2Int centerTile = exitRoom.Center;
        Vector3 portalPosition = new Vector3(centerTile.x + 0.5f, 1f, centerTile.y + 0.5f);
        
        // Load portal prefab
        GameObject portalPrefab = exitPortalPrefab;
        if (portalPrefab == null)
        {
            portalPrefab = ResourceService.LoadExitPrefab();
        }
        
        if (portalPrefab != null)
        {
            currentExitPortal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);
            currentExitPortal.name = "ExitPortal";
            
            // Ensure ExitController exists
            ExitController controller = currentExitPortal.GetComponent<ExitController>();
            if (controller == null)
            {
                controller = currentExitPortal.AddComponent<ExitController>();
            }
            
            Debug.Log($"ProgressionManager: Exit portal spawned at {portalPosition}");
        }
        else
        {
            Debug.LogError("ProgressionManager: No exit portal prefab available!");
            
            // Create fallback portal
            CreateFallbackPortal(portalPosition);
        }
    }
    
    private void CreateFallbackPortal(Vector3 position)
    {
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "ExitPortal_Fallback";
        portal.transform.position = position;
        portal.transform.localScale = new Vector3(2f, 0.5f, 2f);
        
        // Visual
        Renderer renderer = portal.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.cyan;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 2f);
        renderer.material = mat;
        
        // Collider
        Collider col = portal.GetComponent<Collider>();
        col.isTrigger = true;
        
        // Controller
        portal.AddComponent<ExitController>();
        
        currentExitPortal = portal;
        
        Debug.Log("ProgressionManager: Created fallback exit portal");
    }
    
    // ==========================================
    // FLOOR TRANSITION
    // ==========================================
    
    public void OnPlayerEnterExitPortal()
    {
        Debug.Log($"ProgressionManager: Player entered exit portal on floor {currentFloor}");
        
        if (currentExitPortal != null)
        {
            ExitController controller = currentExitPortal.GetComponent<ExitController>();
            if (controller != null && !controller.isActive)
            {
                Debug.Log("ProgressionManager: Portal is inactive, ignoring");
                return;
            }
        }
        
        StartCoroutine(TransitionToNextFloor());
    }
    
    private IEnumerator TransitionToNextFloor()
    {
        Debug.Log("=== FLOOR TRANSITION STARTED ===");
        
        // Show loading (optional)
        yield return new WaitForSeconds(0.5f);
        
        // Clear current floor
        ClearCurrentFloor();
        yield return new WaitForSeconds(0.5f);
        
        // Increment floor
        currentFloor++;
        totalFloorsCleared++;
        
        Debug.Log($"=== NOW ON FLOOR {currentFloor} ===");
        
        // Save progress
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnFloorCompleted();
        }
        
        // Generate next floor
        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.NextFloor();
        }
        else
        {
            Debug.LogError("ProgressionManager: GameDirector not found!");
        }
        
        yield return new WaitForSeconds(1f);
        
        // Respawn player
        EntityManager entityManager = GameDirector.Instance?.entityManager;
        if (entityManager != null)
        {
            entityManager.RespawnPlayerAtEntrance();
        }
        
        // Spawn new portal
        yield return new WaitForSeconds(1f);
        SpawnExitPortal();
        
        Debug.Log("=== FLOOR TRANSITION COMPLETE ===");
    }
    
    private void ClearCurrentFloor()
    {
        Debug.Log("ProgressionManager: Clearing current floor...");
        
        // Destroy portal
        if (currentExitPortal != null)
        {
            Destroy(currentExitPortal);
            currentExitPortal = null;
        }
        
        // Clear enemies
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
    
    // ==========================================
    // DIFFICULTY SCALING
    // ==========================================
    
    public int GetScaledEnemyHealth(int baseHealth)
    {
        float scaled = baseHealth * Mathf.Pow(enemyHealthScaling, currentFloor - 1);
        return Mathf.RoundToInt(scaled);
    }
    
    public int GetScaledEnemyDamage(int baseDamage)
    {
        float scaled = baseDamage * Mathf.Pow(enemyDamageScaling, currentFloor - 1);
        return Mathf.RoundToInt(scaled);
    }
    
    public int GetScaledGoldReward(int baseReward)
    {
        return baseReward + (goldRewardScaling * (currentFloor - 1));
    }
}