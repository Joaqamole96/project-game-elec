// -------------------------------------------------- //
// Scripts/Core/GameDirector.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections;

/// <summary>
/// The God Object. Manages all major systems in the game.
/// Only one should exist per scene - creates and orchestrates all managers.
/// </summary>
public class GameDirector : MonoBehaviour
{
    [Header("Director Settings")]
    public bool autoInitializeOnStart = true;
    public float initializationDelay = 0.1f;
    public string currentBiome = "Default";
    
    [Header("Resource Paths - Leave Empty to Use ResourceService")]
    public GameObject layoutManagerPrefab;
    public GameObject uiManagerPrefab;
    public GameObject entityManagerPrefab;
    public GameObject audioManagerPrefab;
    
    [Header("Manager References (Auto-Created)")]
    public LayoutManager layoutManager;
    public UIManager uiManager;
    public EntityManager entityManager;
    public AudioManager audioManager;
    public static GameDirector Instance { get; private set; }
    public bool IsInitialized { get; private set; }
    
    // Container GameObjects
    private GameObject managersContainer;
    private GameObject entitiesContainer;
    private GameObject cameraContainer;
    
    // ------------------------- //
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameDirector: Singleton instance created");
        }
        else
        {
            Debug.LogWarning("GameDirector: Duplicate instance detected, destroying...");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (autoInitializeOnStart)
        {
            StartCoroutine(InitializeGameSystems());
        }
    }
    
    // ------------------------- //
    // INITIALIZATION PIPELINE
    // ------------------------- //
    
    /// <summary>
    /// Master initialization sequence - creates all game systems in proper order.
    /// </summary>
    [ContextMenu("Initialize Game Systems")]
    public void Initialize()
    {
        StartCoroutine(InitializeGameSystems());
    }
    
    private IEnumerator InitializeGameSystems()
    {
        if (IsInitialized)
        {
            Debug.LogWarning("GameDirector: Already initialized, skipping...");
            yield break;
        }
        
        Debug.Log("=== GameDirector: Initializing Game Systems ===");
        
        // Phase 1: Create container structure
        CreateContainerHierarchy();
        yield return new WaitForSeconds(initializationDelay);
        
        // Phase 2: Destroy any existing rogue objects
        CleanupScene();
        yield return new WaitForSeconds(initializationDelay);
        
        // Phase 3: Initialize managers in dependency order
        InitializeLayoutManager();
        yield return new WaitForSeconds(initializationDelay);
        
        InitializeEntityManager();
        yield return new WaitForSeconds(initializationDelay);
        
        InitializeUIManager();
        yield return new WaitForSeconds(initializationDelay);
        
        InitializeAudioManager();
        yield return new WaitForSeconds(initializationDelay);
        
        // Phase 4: Create camera system
        InitializeCameraSystem();
        yield return new WaitForSeconds(initializationDelay);
        
        // Phase 5: Generate initial dungeon
        GenerateInitialDungeon();
        yield return new WaitForSeconds(initializationDelay);
        
        // Phase 6: Spawn player
        SpawnPlayer();
        
        IsInitialized = true;
        Debug.Log("=== GameDirector: Initialization Complete ===");
    }
    
    // ------------------------- //
    // CONTAINER MANAGEMENT
    // ------------------------- //
    
    private void CreateContainerHierarchy()
    {
        Debug.Log("GameDirector: Creating container hierarchy...");
        
        // Create main containers as children of GameDirector
        managersContainer = CreateOrGetContainer("Managers");
        entitiesContainer = CreateOrGetContainer("Entities");
        cameraContainer = CreateOrGetContainer("Camera");
        
        Debug.Log("GameDirector: Container hierarchy created");
    }
    
    private GameObject CreateOrGetContainer(string containerName)
    {
        // Check if container already exists as child
        Transform existingContainer = transform.Find(containerName);
        if (existingContainer != null)
        {
            Debug.Log($"GameDirector: Found existing container '{containerName}'");
            return existingContainer.gameObject;
        }
        
        // Create new container
        GameObject container = new(containerName);
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.zero;
        Debug.Log($"GameDirector: Created container '{containerName}'");
        return container;
    }
    
    private void CleanupScene()
    {
        Debug.Log("GameDirector: Cleaning up rogue objects...");
        
        // Find and destroy any MainCamera not under our control
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (var cam in cameras)
        {
            if (cam.gameObject.name == "Main Camera" && cam.transform.parent == null)
            {
                Debug.Log($"GameDirector: Destroying rogue camera: {cam.gameObject.name}");
                Destroy(cam.gameObject);
            }
        }
        
        // Find and destroy any Player not under EntityManager
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            if (player.transform.parent == null || player.transform.parent.name != "Entities")
            {
                Debug.Log($"GameDirector: Destroying rogue player: {player.gameObject.name}");
                Destroy(player.gameObject);
            }
        }
        
        Debug.Log("GameDirector: Scene cleanup complete");
    }
    
    // ------------------------- //
    // MANAGER INITIALIZATION
    // ------------------------- //
    
    private void InitializeLayoutManager()
    {
        Debug.Log("GameDirector: Initializing LayoutManager...");
        
        if (layoutManager != null)
        {
            Debug.Log("GameDirector: LayoutManager already exists");
            return;
        }
        
        if (layoutManagerPrefab != null)
        {
            GameObject layoutObj = Instantiate(layoutManagerPrefab, managersContainer.transform);
            layoutObj.name = "LayoutManager";
            layoutManager = layoutObj.GetComponent<LayoutManager>();
        }
        else
        {
            // Create from scratch if no prefab
            GameObject layoutObj = new("LayoutManager");
            layoutObj.transform.SetParent(managersContainer.transform);
            layoutManager = layoutObj.AddComponent<LayoutManager>();
        }
        
        if (layoutManager != null)
        {
            Debug.Log("GameDirector: LayoutManager initialized");
        }
        else
        {
            Debug.LogError("GameDirector: Failed to initialize LayoutManager!");
        }
    }
    
    private void InitializeEntityManager()
    {
        Debug.Log("GameDirector: Initializing EntityManager...");
        
        if (entityManager != null)
        {
            Debug.Log("GameDirector: EntityManager already exists");
            return;
        }
        
        if (entityManagerPrefab != null)
        {
            GameObject entityObj = Instantiate(entityManagerPrefab, managersContainer.transform);
            entityObj.name = "EntityManager";
            entityManager = entityObj.GetComponent<EntityManager>();
        }
        else
        {
            GameObject entityObj = new("EntityManager");
            entityObj.transform.SetParent(managersContainer.transform);
            entityManager = entityObj.AddComponent<EntityManager>();
        }
        
        // Set the entities container for EntityManager to use
        if (entityManager != null)
        {
            entityManager.SetEntitiesContainer(entitiesContainer.transform);
            Debug.Log("GameDirector: EntityManager initialized");
        }
        else
        {
            Debug.LogError("GameDirector: Failed to initialize EntityManager!");
        }
    }
    
    private void InitializeUIManager()
    {
        Debug.Log("GameDirector: Initializing UIManager...");
        
        if (uiManager != null)
        {
            Debug.Log("GameDirector: UIManager already exists");
            return;
        }
        
        if (uiManagerPrefab != null)
        {
            GameObject uiObj = Instantiate(uiManagerPrefab, managersContainer.transform);
            uiObj.name = "UIManager";
            uiManager = uiObj.GetComponent<UIManager>();
        }
        else
        {
            GameObject uiObj = new("UIManager");
            uiObj.transform.SetParent(managersContainer.transform);
            uiManager = uiObj.AddComponent<UIManager>();
        }
        
        if (uiManager != null)
        {
            Debug.Log("GameDirector: UIManager initialized");
        }
        else
        {
            Debug.LogWarning("GameDirector: UIManager component not found (may be implemented later)");
        }
    }
    
    private void InitializeAudioManager()
    {
        Debug.Log("GameDirector: Initializing AudioManager...");
        
        if (audioManager != null)
        {
            Debug.Log("GameDirector: AudioManager already exists");
            return;
        }
        
        if (audioManagerPrefab != null)
        {
            GameObject audioObj = Instantiate(audioManagerPrefab, managersContainer.transform);
            audioObj.name = "AudioManager";
            audioManager = audioObj.GetComponent<AudioManager>();
        }
        else
        {
            GameObject audioObj = new("AudioManager");
            audioObj.transform.SetParent(managersContainer.transform);
            audioManager = audioObj.AddComponent<AudioManager>();
        }
        
        if (audioManager != null)
        {
            Debug.Log("GameDirector: AudioManager initialized");
        }
        else
        {
            Debug.LogWarning("GameDirector: AudioManager component not found (may be implemented later)");
        }
    }
    
    // ------------------------- //
    // CAMERA SYSTEM
    // ------------------------- //
    
    private void InitializeCameraSystem()
    {
        Debug.Log("GameDirector: Initializing camera system...");
        
        // Destroy any existing cameras
        foreach (Transform child in cameraContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        GameObject cameraObj;
        GameObject mainCameraPrefab = ResourceService.LoadCameraPrefab();
        
        if (mainCameraPrefab != null)
        {
            cameraObj = Instantiate(mainCameraPrefab, cameraContainer.transform);
            cameraObj.name = "MainCamera";
        }
        else
        {
            // Create default camera
            cameraObj = new GameObject("MainCamera");
            cameraObj.transform.SetParent(cameraContainer.transform);
            
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            
            cameraObj.AddComponent<AudioListener>();
            cameraObj.AddComponent<CameraController>();
            
            Debug.Log("GameDirector: Created default camera (prefab not found in Resources)");
        }
        
        Debug.Log("GameDirector: Camera system initialized");
    }
    
    // ------------------------- //
    // DUNGEON GENERATION
    // ------------------------- //
    
    private void GenerateInitialDungeon()
    {
        Debug.Log("GameDirector: Generating initial dungeon...");
        
        if (layoutManager != null)
        {
            layoutManager.GenerateDungeon();
            Debug.Log("GameDirector: Initial dungeon generated");
        }
        else
        {
            Debug.LogError("GameDirector: Cannot generate dungeon - LayoutManager is null!");
        }
    }
    
    // ------------------------- //
    // PLAYER SPAWNING
    // ------------------------- //
    
    private void SpawnPlayer()
    {
        Debug.Log("GameDirector: Spawning player...");
        
        if (entityManager != null)
        {
            GameObject playerPrefab = ResourceService.LoadPlayerPrefab();
            
            if (playerPrefab != null)
            {
                entityManager.SpawnPlayer(playerPrefab);
            }
            else
            {
                Debug.LogError("GameDirector: Cannot spawn player - failed to load from Resources/Default/Entities/Player");
            }
        }
        else
        {
            Debug.LogError("GameDirector: Cannot spawn player - EntityManager is null!");
        }
    }
    
    // ------------------------- //
    // PUBLIC API
    // ------------------------- //
    
    public void RegenerateLevel()
    {
        if (layoutManager != null)
        {
            Debug.Log("GameDirector: Regenerating level...");
            layoutManager.GenerateDungeon();
        }
    }
    
    public void NextFloor()
    {
        if (layoutManager != null)
        {
            Debug.Log("GameDirector: Advancing to next floor...");
            layoutManager.GenerateNextFloor();
            
            // Respawn player at new entrance
            if (entityManager != null)
            {
                entityManager.RespawnPlayerAtEntrance();
            }
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("GameDirector: Restarting game...");
        
        IsInitialized = false;
        
        // Clear all entities
        if (entityManager != null)
        {
            entityManager.ClearAllEntities();
        }
        
        // Clear layout
        if (layoutManager != null)
        {
            layoutManager.ClearRendering();
        }
        
        // Restart initialization
        StartCoroutine(InitializeGameSystems());
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    [ContextMenu("Print Hierarchy")]
    public void PrintHierarchy()
    {
        Debug.Log("=== GameDirector Hierarchy ===");
        Debug.Log($"GameDirector: {gameObject.name}");
        
        foreach (Transform child in transform)
        {
            Debug.Log($"  ├─ {child.name}");
            foreach (Transform grandchild in child)
            {
                Debug.Log($"      ├─ {grandchild.name}");
            }
        }
        
        Debug.Log("=============================");
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}