// ================================================== //
// Scripts/Managers/EntityManager.cs (ENHANCED FOR BIOMES)
// ================================================== //

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

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
    public List<GameObject> allEnemies = new();
    public List<GameObject> allNPCs = new();
    
    [Header("Spawn Settings")]
    public float spawnHeight = 1f;
    
    [Header("Enemy Spawning Rules")]
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 4;
    public int bossRoomEnemies = 1; // Just the boss
    
    private string currentBiome;
    private System.Random spawnRandom;
    private bool navMeshReady = false;
    
    // ------------------------- //
    
    void Awake()
    {
        if (entitiesContainer == null)
        {
            GameObject tempContainer = GameObject.Find("Entities");
            if (tempContainer != null) entitiesContainer = tempContainer.transform;
        }
        spawnRandom = new System.Random();
    }
    
    void Start()
    {
        // Subscribe to NavMesh ready event
        LayoutManager.OnNavMeshReady += OnNavMeshReady;
        StartCoroutine(SpawnEntitiesAfterGeneration());
    }

    private void OnNavMeshReady()
    {
        Debug.Log("EntityManager: Received NavMesh ready signal");
        navMeshReady = true;
        StartCoroutine(SpawnEntitiesAfterGeneration());
    }
    
    private IEnumerator SpawnEntitiesAfterGeneration()
    {
        if (!navMeshReady)
        {
            Debug.LogWarning("EntityManager: NavMesh not ready, waiting...");
            yield return new WaitUntil(() => navMeshReady);
        }
        
        // Additional safety wait
        yield return new WaitForSeconds(1f);
        
        // Triple-check NavMesh exists
        if (!IsNavMeshReady())
        {
            Debug.LogError("EntityManager: NavMesh still not ready after event!");
            yield break;
        }
        
        Debug.Log("EntityManager: NavMesh confirmed ready, spawning entities...");
        SpawnAllEntitiesInDungeon();
    }

    private bool IsNavMeshReady()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        bool ready = triangulation.vertices.Length > 0;
        
        if (!ready)
        {
            Debug.LogWarning("EntityManager: NavMesh has 0 vertices!");
        }
        
        return ready;
    }
    
    // ------------------------- //
    // CONTAINER SETUP
    // ------------------------- //
    
    private void InitializeContainers()
    {
        if (entitiesContainer == null)
        {
            Debug.LogWarning("EntityManager: entitiesContainer not set, creating temporary");
            GameObject container = new("Entities");
            entitiesContainer = container.transform;
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
        
        GameObject container = new(containerName);
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
        
        if (currentPlayer != null)
        {
            Debug.Log("EntityManager: Destroying existing player");
            Destroy(currentPlayer);
        }
        
        Vector3 spawnPosition = GetPlayerSpawnPosition();
        
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, playersContainer);
        currentPlayer.name = "Player";
        
        Debug.Log($"EntityManager: Player spawned at {spawnPosition}");
        
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
        
        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log($"EntityManager: Player respawned at {spawnPosition}");
        }
    }
    
    // ------------------------- //
    // ENHANCED ENEMY SPAWNING
    // ------------------------- //

    [ContextMenu("Debug NavMesh State")]
    public void DebugNavMeshState()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Debug.Log($"=== NavMesh Debug ===");
        Debug.Log($"Vertices: {triangulation.vertices.Length}");
        Debug.Log($"Indices: {triangulation.indices.Length}");
        Debug.Log($"Areas: {triangulation.areas.Length}");
        
        if (triangulation.vertices.Length > 0)
        {
            Debug.Log($"Sample position: {triangulation.vertices[0]}");
        }
        else
        {
            Debug.LogError("NavMesh has NO geometry!");
        }
    }
    
    public void SpawnAllEntitiesInDungeon()
    {
        BiomeManager biomeManager = GameDirector.Instance?.layoutManager?.GetComponent<BiomeManager>();
        if (biomeManager != null)
        {
            currentBiome = biomeManager.CurrentBiome;
        }
        
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager == null || layoutManager.CurrentLayout == null)
        {
            Debug.LogWarning("EntityManager: Cannot spawn entities - no layout available");
            return;
        }
        
        int totalEnemiesSpawned = 0;
        
        foreach (RoomModel room in layoutManager.CurrentLayout.Rooms)
        {
            int enemiesSpawned = SpawnEntitiesInRoom(room);
            totalEnemiesSpawned += enemiesSpawned;
        }
        
        Debug.Log($"EntityManager: Spawned {totalEnemiesSpawned} total enemies in biome '{currentBiome}'");
    }
    
    private int SpawnEntitiesInRoom(RoomModel room)
    {
        switch (room.Type)
        {
            case RoomType.Combat:
                return SpawnCombatRoomEnemies(room);
                
            case RoomType.Boss:
                return SpawnBossRoomEntities(room);
                
            case RoomType.Shop:
                return SpawnShopKeeper(room);
                
            case RoomType.Treasure:
                return SpawnTreasureChest(room);
                
            default:
                return 0;
        }
    }
    
    private int SpawnCombatRoomEnemies(RoomModel room)
    {
        int enemyCount = spawnRandom.Next(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        
        List<GameObject> enemyPrefabs = GetBiomeEnemyPrefabs(currentBiome);
        
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"No enemy prefabs available for biome '{currentBiome}'");
            return 0;
        }
        
        int successfulSpawns = 0;
        int maxAttempts = enemyCount * 5; // More attempts
        int attempts = 0;
        
        while (successfulSpawns < enemyCount && attempts < maxAttempts)
        {
            attempts++;
            
            GameObject enemyPrefab = enemyPrefabs[spawnRandom.Next(0, enemyPrefabs.Count)];
            
            Vector2Int spawnTile = room.GetRandomSpawnPosition();
            Vector3 spawnPosition = new(spawnTile.x + 0.5f, spawnHeight, spawnTile.y + 0.5f);
            
            // CRITICAL: Validate position is on NavMesh
            if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"No NavMesh near {spawnTile}, trying another position...");
                continue; // Skip this position, try another
            }
            
            // Use the confirmed NavMesh position
            spawnPosition = hit.position;
            
            GameObject enemy = SpawnEnemy(enemyPrefab, spawnPosition);
            if (enemy != null)
            {
                successfulSpawns++;
            }
        }
        
        if (successfulSpawns < enemyCount)
        {
            Debug.LogWarning($"Room {room.ID}: Only spawned {successfulSpawns}/{enemyCount} enemies (NavMesh constraints)");
        }
        
        return successfulSpawns;
    }
    
    private int SpawnBossRoomEntities(RoomModel room)
    {
        // Spawn boss spawner landmark
        Vector2Int centerTile = room.Center;
        Vector3 spawnerPosition = new(centerTile.x + 0.5f, 0f, centerTile.y + 0.5f);
        
        GameObject bossSpawner = new("BossSpawner");
        bossSpawner.transform.position = spawnerPosition;
        
        BossSpawner spawnerComponent = bossSpawner.AddComponent<BossSpawner>();
        
        // Set boss power reward
        PowerType[] bossRewards = new PowerType[]
        {
            PowerType.MaxHealth,
            PowerType.Damage,
            PowerType.AttackSpeed,
            PowerType.SpeedBoost,
            PowerType.Vampire
        };
        spawnerComponent.guaranteedPower = bossRewards[spawnRandom.Next(0, bossRewards.Length)];
        
        Debug.Log($"Boss room spawner created with reward: {spawnerComponent.guaranteedPower}");
        
        return 1;
    }
    
    private int SpawnShopKeeper(RoomModel room)
    {
        Vector2Int centerTile = room.Center;
        Vector3 shopPosition = new(centerTile.x + 0.5f, spawnHeight, centerTile.y + 0.5f);
        
        // Create shop object
        GameObject shop = new("Shop");
        shop.transform.position = shopPosition;
        
        // Visual (placeholder)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(shop.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(2f, 1f, 2f);
        
        Renderer renderer = visual.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = Color.blue
        };
        renderer.material = mat;
        
        // Add trigger collider
        SphereCollider trigger = shop.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3f;
        
        // Add shop controller
        shop.AddComponent<ShopController>();
        
        Debug.Log($"Shop spawned in room {room.ID}");
        
        return 0; // Not an enemy
    }
    
    private int SpawnTreasureChest(RoomModel room)
    {
        Vector2Int centerTile = room.Center;
        Vector3 chestPosition = new(centerTile.x + 0.5f, spawnHeight, centerTile.y + 0.5f);
        
        // Create chest object
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chest.transform.position = chestPosition;
        chest.transform.localScale = new Vector3(1f, 0.8f, 0.7f);
        chest.name = "TreasureChest";
        
        Renderer renderer = chest.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = new Color(0.8f, 0.6f, 0.2f) // Gold color
        };
        renderer.material = mat;
        
        // Make trigger
        Collider collider = chest.GetComponent<Collider>();
        collider.isTrigger = true;
        
        // Add treasure controller
        TreasureChestController treasureController = chest.AddComponent<TreasureChestController>();
        treasureController.treasureType = TreasureChestController.TreasureType.Random;
        
        Debug.Log($"Treasure chest spawned in room {room.ID}");
        
        return 0; // Not an enemy
    }
    
    private List<GameObject> GetBiomeEnemyPrefabs(string biome)
    {
        List<GameObject> prefabs = new();
        
        // Try to load 3 enemy types per biome
        // GameObject basic = ResourceService.LoadBasicEnemyPrefab(biome);
        // GameObject elite = ResourceService.LoadEliteEnemyPrefab(biome);
        GameObject boss = ResourceService.LoadBossEnemyPrefab(biome);
        
        GameObject melee = ResourceService.LoadMeleeEnemyPrefab(biome);
        GameObject ranged = ResourceService.LoadRangedEnemyPrefab(biome);
        GameObject tank = ResourceService.LoadTankEnemyPrefab(biome);
        
        // if (basic != null) prefabs.Add(basic);
        // if (elite != null) prefabs.Add(elite);

        if (melee != null) prefabs.Add(melee);
        if (ranged != null) prefabs.Add(ranged);
        if (tank != null) prefabs.Add(tank);
        
        // Don't add boss to regular combat rooms
        
        // If no enemies found, create placeholder
        if (prefabs.Count == 0)
        {
            Debug.LogWarning($"No enemy prefabs found for biome '{biome}', using placeholder");
            prefabs.Add(CreatePlaceholderEnemy());
        }
        
        return prefabs;
    }
    
    private GameObject CreatePlaceholderEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "PlaceholderEnemy";
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Default");
        
        // Add enemy controller
        EnemyController controller = enemy.AddComponent<EnemyController>();
        controller.maxHealth = 30;
        controller.damage = 10;
        controller.moveSpeed = 2f;
        
        // Add NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 2f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        
        // Visual
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = Color.red
        };
        renderer.material = mat;
        
        return enemy;
    }
    
    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EntityManager: Cannot spawn enemy - prefab is null!");
            return null;
        }
        
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, enemiesContainer);
        allEnemies.Add(enemy);
        
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
            Vector3 spawnPosition = new(spawnTile.x + 0.5f, spawnHeight, spawnTile.y + 0.5f);
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
        allEnemies.RemoveAll(e => e == null);
        return allEnemies.Count;
    }
    
    public List<GameObject> GetEnemiesInRadius(Vector3 center, float radius)
    {
        List<GameObject> nearbyEnemies = new();
        
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
        
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
        
        ClearAllEnemies();
        ClearAllNPCs();
        
        Debug.Log("EntityManager: All entities cleared");
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        LayoutManager.OnNavMeshReady -= OnNavMeshReady;
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
        Debug.Log($"Current Biome: {currentBiome}");
        Debug.Log("===========================");
    }
}