// -------------------------------------------------- //
// Scripts/Managers/EntityManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Manages all game entities (players, enemies, NPCs, etc.)
/// Handles spawning, despawning, and tracking of all living entities.
/// Replaces the old PlayerManager functionality.
/// </summary>
public class EntityManager : MonoBehaviour
{
    [Header("Entity Containers")]
    public Transform entitiesContainer;
    public Transform playersContainer;
    public Transform enemiesContainer;
    public Transform npcsContainer;
    
    [Header("Current References")]
    public GameObject currentPlayer;
    public CameraController currentCamera;
    
    [Header("Entity Tracking")]
    public List<GameObject> allEnemies = new List<GameObject>();
    public List<GameObject> allNPCs = new List<GameObject>();
    
    [Header("Spawn Settings")]
    public float spawnHeight = 1f;
    
    // Enemy prefabs loaded from Resources based on biome
    private string currentBiome = ResourceService.BIOME_DEFAULT;
    
    // ------------------------- //
    
    void Awake()
    {
        InitializeContainers();
    }
    
    void Start()
    {
        // Auto-spawn enemies after level generation
        StartCoroutine(SpawnEnemiesAfterGeneration());
    }
    
    private IEnumerator SpawnEnemiesAfterGeneration()
    {
        // Wait for level to be fully generated
        yield return new WaitForSeconds(1f);
        
        SpawnEnemiesInAllCombatRooms();
    }
    
    // ------------------------- //
    // CONTAINER SETUP
    // ------------------------- //
    
    private void InitializeContainers()
    {
        if (entitiesContainer == null)
        {
            Debug.LogWarning("EntityManager: entitiesContainer not set");
            return;
        }
        
        playersContainer = CreateOrGetChildContainer("Players");
        enemiesContainer = CreateOrGetChildContainer("Enemies");
        npcsContainer = CreateOrGetChildContainer("NPCs");
        
        Debug.Log("EntityManager: Containers initialized");
    }
    
    private Transform CreateOrGetChildContainer(string containerName)
    {
        Transform existing = entitiesContainer.Find(containerName);
        if (existing != null) return existing;
        
        GameObject container = new GameObject(containerName);
        container.transform.SetParent(entitiesContainer);
        container.transform.localPosition = Vector3.zero;
        return container.transform;
    }
    
    public void SetEntitiesContainer(Transform container)
    {
        entitiesContainer = container;
        InitializeContainers();
    }
    
    // ------------------------- //
    // PLAYER MANAGEMENT
    // ------------------------- //
    
    public void SpawnPlayer(GameObject playerPrefab)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("EntityManager: Cannot spawn player - prefab is null!");
            return;
        }
        
        // Destroy existing player if any
        if (currentPlayer != null)
        {
            Debug.Log("EntityManager: Destroying existing player");
            Destroy(currentPlayer);
        }
        
        // Get spawn position from LayoutManager
        Vector3 spawnPosition = GetPlayerSpawnPosition();
        
        // Instantiate player
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, playersContainer);
        currentPlayer.name = "Player";
        
        Debug.Log($"EntityManager: Player spawned at {spawnPosition}");
        
        // Setup camera to follow player
        SetupCameraForPlayer();
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        
        if (layoutManager != null && layoutManager.CurrentLayout != null)
        {
            try
            {
                Vector3 entrancePos = layoutManager.GetEntranceRoomPosition();
                return new Vector3(entrancePos.x, spawnHeight, entrancePos.z);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"EntityManager: Could not get entrance position: {e.Message}");
            }
        }
        
        Debug.LogWarning("EntityManager: Using fallback spawn position");
        return new Vector3(0, spawnHeight, 0);
    }
    
    private void SetupCameraForPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("EntityManager: Cannot setup camera - player is null");
            return;
        }
        
        // Find main camera
        if (currentCamera == null)
        {
            currentCamera = Camera.main?.GetComponent<CameraController>();
        }
        
        if (currentCamera != null)
        {
            currentCamera.SetTarget(currentPlayer.transform);
            Debug.Log("EntityManager: Camera target set to player");
        }
        else
        {
            Debug.LogWarning("EntityManager: Could not find CameraController");
        }
    }
    
    public void RespawnPlayerAtEntrance()
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("EntityManager: Cannot respawn player - no player exists");
            return;
        }
        
        Vector3 spawnPosition = GetPlayerSpawnPosition();
        currentPlayer.transform.position = spawnPosition;
        
        // Reset player health if needed
        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Add reset method to PlayerController if needed
            Debug.Log($"EntityManager: Player respawned at {spawnPosition}");
        }
    }
    
    // ------------------------- //
    // ENEMY MANAGEMENT
    // ------------------------- //
    
    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EntityManager: Cannot spawn enemy - prefab is null!");
            return null;
        }
        
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, enemiesContainer);
        allEnemies.Add(enemy);
        
        Debug.Log($"EntityManager: Enemy spawned at {position}");
        return enemy;
    }
    
    public void SpawnEnemiesInRoom(GameObject enemyPrefab, RoomModel room, int count)
    {
        if (enemyPrefab == null || room == null)
        {
            Debug.LogWarning("EntityManager: Cannot spawn enemies - invalid parameters");
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            Vector2Int spawnTile = room.GetRandomSpawnPosition();
            Vector3 spawnPosition = new Vector3(spawnTile.x + 0.5f, spawnHeight, spawnTile.y + 0.5f);
            SpawnEnemy(enemyPrefab, spawnPosition);
        }
        
        Debug.Log($"EntityManager: Spawned {count} enemies in room {room.ID}");
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        allEnemies.Clear();
        Debug.Log("EntityManager: All enemies cleared");
    }
    
    public void SpawnEnemiesInAllCombatRooms()
    {
        BiomeManager biomeManager = GameDirector.Instance?.layoutManager?.GetComponent<BiomeManager>();
        if (biomeManager != null)
        {
            currentBiome = biomeManager.CurrentBiome;
        }
        
        GameObject enemyPrefab = ResourceService.LoadBasicEnemyPrefab(currentBiome);
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"EntityManager: Cannot spawn enemies - no BasicEnemy prefab in {currentBiome} biome");
            return;
        }
        
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager == null || layoutManager.CurrentLayout == null)
        {
            Debug.LogWarning("EntityManager: Cannot spawn enemies - no layout available");
            return;
        }
        
        int totalEnemiesSpawned = 0;
        
        foreach (RoomModel room in layoutManager.CurrentLayout.Rooms)
        {
            if (room.Type == RoomType.Combat)
            {
                int enemyCount = Random.Range(2, 5);
                SpawnEnemiesInRoom(enemyPrefab, room, enemyCount);
                totalEnemiesSpawned += enemyCount;
            }
            else if (room.Type == RoomType.Boss)
            {
                // Load boss prefab
                GameObject bossPrefab = ResourceService.LoadBossEnemyPrefab(currentBiome);
                if (bossPrefab != null)
                {
                    SpawnEnemiesInRoom(bossPrefab, room, 1);
                    totalEnemiesSpawned += 1;
                }
                else
                {
                    // Fallback to basic enemy
                    SpawnEnemiesInRoom(enemyPrefab, room, 1);
                    totalEnemiesSpawned += 1;
                }
            }
        }
        
        Debug.Log($"EntityManager: Spawned {totalEnemiesSpawned} enemies total in biome '{currentBiome}'");
    }
    
    // ------------------------- //
    // NPC MANAGEMENT
    // ------------------------- //
    
    public GameObject SpawnNPC(GameObject npcPrefab, Vector3 position)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("EntityManager: Cannot spawn NPC - prefab is null!");
            return null;
        }
        
        GameObject npc = Instantiate(npcPrefab, position, Quaternion.identity, npcsContainer);
        allNPCs.Add(npc);
        
        Debug.Log($"EntityManager: NPC spawned at {position}");
        return npc;
    }
    
    public void ClearAllNPCs()
    {
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        
        allNPCs.Clear();
        Debug.Log("EntityManager: All NPCs cleared");
    }
    
    // ------------------------- //
    // ENTITY QUERIES
    // ------------------------- //
    
    public int GetAliveEnemyCount()
    {
        // Remove null references (destroyed enemies)
        allEnemies.RemoveAll(e => e == null);
        return allEnemies.Count;
    }
    
    public List<GameObject> GetEnemiesInRadius(Vector3 center, float radius)
    {
        List<GameObject> nearbyEnemies = new List<GameObject>();
        
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy != null && Vector3.Distance(enemy.transform.position, center) <= radius)
            {
                nearbyEnemies.Add(enemy);
            }
        }
        
        return nearbyEnemies;
    }
    
    // ------------------------- //
    // CLEANUP
    // ------------------------- //
    
    public void ClearAllEntities()
    {
        Debug.Log("EntityManager: Clearing all entities...");
        
        // Clear player
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
        
        // Clear enemies
        ClearAllEnemies();
        
        // Clear NPCs
        ClearAllNPCs();
        
        Debug.Log("EntityManager: All entities cleared");
    }
    
    // ------------------------- //
    // DEBUG
    // ------------------------- //
    
    [ContextMenu("Print Entity Stats")]
    public void PrintEntityStats()
    {
        Debug.Log("=== Entity Manager Stats ===");
        Debug.Log($"Player: {(currentPlayer != null ? "Active" : "None")}");
        Debug.Log($"Enemies: {GetAliveEnemyCount()}");
        Debug.Log($"NPCs: {allNPCs.Count}");
        Debug.Log("===========================");
    }
}