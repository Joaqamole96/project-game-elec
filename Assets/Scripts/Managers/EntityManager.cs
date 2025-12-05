// ================================================== //
// Scripts/Managers/EntityManager.cs
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
    
    [Header("Spawn Settings")]
    public float spawnHeight = 1f;
    public int minEnemiesPerRoom = 4;
    public int maxEnemiesPerRoom = 8;
    
    private string currentBiome;
    private System.Random spawnRandom;
    private bool hasSpawnedEnemies = false;
    
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
    }

    private void OnNavMeshReady()
    {
        // CRITICAL FIX: Only spawn once
        if (hasSpawnedEnemies)
        {
            Debug.LogWarning("EntityManager: Enemies already spawned, ignoring duplicate call");
            return;
        }
        
        Debug.Log("EntityManager: NavMesh ready, spawning entities");
        StartCoroutine(SpawnEntitiesAfterDelay());
    }
    
    private IEnumerator SpawnEntitiesAfterDelay()
    {
        // Wait 1 frame to ensure NavMesh is fully ready
        yield return null;
        
        if (!IsNavMeshReady())
        {
            Debug.LogError("EntityManager: NavMesh not ready after event!");
            yield break;
        }
        
        hasSpawnedEnemies = true; // CRITICAL: Mark as spawned
        SpawnAllEntitiesInDungeon();
    }

    private bool IsNavMeshReady()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        return triangulation.vertices.Length > 0;
    }
    
    private void InitializeContainers()
    {
        if (entitiesContainer == null)
        {
            GameObject container = new("Entities");
            entitiesContainer = container.transform;
        }
        
        playersContainer = CreateOrGetChildContainer("Player");
        enemiesContainer = CreateOrGetChildContainer("Enemies");
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
    
    // ==========================================
    // PLAYER MANAGEMENT
    // ==========================================
    
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
        LayoutManager layoutManager = GameDirector.Instance != null ? GameDirector.Instance.layoutManager : null;
        
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
        if (currentPlayer == null) return;
        
        if (currentCamera == null) currentCamera = Camera.main?.GetComponent<CameraController>();
        
        if (currentCamera != null)
        {
            currentCamera.SetTarget(currentPlayer.transform);
            Debug.Log("EntityManager: Camera target set to player");
        }
    }
    
    public void RespawnPlayerAtEntrance()
    {
        if (currentPlayer == null) return;
        
        Vector3 spawnPosition = GetPlayerSpawnPosition();
        currentPlayer.transform.position = spawnPosition;
        
        Debug.Log($"EntityManager: Player respawned at {spawnPosition}");
    }
    
    // ==========================================
    // ENEMY SPAWNING (SIMPLIFIED)
    // ==========================================
    
    public void SpawnAllEntitiesInDungeon()
    {
        BiomeManager biomeManager = GameDirector.Instance?.layoutManager?.GetComponent<BiomeManager>();
        if (biomeManager != null)
        {
            currentBiome = biomeManager.CurrentBiome;
        }
        
        LayoutManager layoutManager = GameDirector.Instance != null ? GameDirector.Instance.layoutManager : null;
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
        return room.Type switch
        {
            RoomType.Combat => SpawnCombatRoomEnemies(room),
            RoomType.Boss => SpawnBossRoomEntities(room),
            RoomType.Shop => SpawnShopKeeper(room),
            RoomType.Treasure => SpawnTreasureChest(room),
            _ => 0,
        };
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
        int maxAttempts = enemyCount * 10;
        int attempts = 0;
        
        while (successfulSpawns < enemyCount && attempts < maxAttempts)
        {
            attempts++;
            
            GameObject enemyPrefab = enemyPrefabs[spawnRandom.Next(0, enemyPrefabs.Count)];
            Vector2Int spawnTile = room.GetRandomSpawnPosition();
            Vector3 spawnPosition = new(spawnTile.x + 0.5f, spawnHeight, spawnTile.y + 0.5f);
            
            // CRITICAL: Validate NavMesh position
            if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas)) continue;
            
            spawnPosition = hit.position;
            
            GameObject enemy = SpawnEnemy(enemyPrefab, spawnPosition);
            if (enemy != null) successfulSpawns++;
        }
        
        return successfulSpawns;
    }
    
    private int SpawnBossRoomEntities(RoomModel room)
    {
        // CRITICAL FIX: Only spawn ONE boss spawner
        Vector2Int centerTile = room.Center;
        Vector3 spawnerPosition = new(centerTile.x + 0.5f, 0f, centerTile.y + 0.5f);
        
        GameObject bossSpawner = new("BossSpawner");
        bossSpawner.transform.position = spawnerPosition;
        
        BossSpawner spawnerComponent = bossSpawner.AddComponent<BossSpawner>();
        
        PowerType[] bossRewards = new PowerType[]
        {
            PowerType.MaxHealth,
            PowerType.Damage,
            PowerType.AttackSpeed
        };
        spawnerComponent.guaranteedPower = bossRewards[spawnRandom.Next(0, bossRewards.Length)];
        
        Debug.Log($"Boss room spawner created with reward: {spawnerComponent.guaranteedPower}");
        
        return 1; // One spawner (will create one boss)
    }
    
    private int SpawnShopKeeper(RoomModel room)
    {
        Vector2Int centerTile = room.Center;
        Vector3 shopPosition = new(centerTile.x + 0.5f, spawnHeight, centerTile.y + 0.5f);
        
        GameObject shop = new("Shop");
        shop.transform.position = shopPosition;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(shop.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(2f, 1f, 2f);
        
        Renderer renderer = visual.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard")) { color = Color.blue };
        renderer.material = mat;
        
        SphereCollider trigger = shop.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3f;
        
        shop.AddComponent<ShopController>();
        
        Debug.Log($"Shop spawned in room {room.ID}");
        return 0;
    }
    
    private int SpawnTreasureChest(RoomModel room)
    {
        Vector2Int centerTile = room.Center;
        Vector3 chestPosition = new(centerTile.x + 0.5f, spawnHeight, centerTile.y + 0.5f);
        
        // GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // chest.transform.position = chestPosition;
        // chest.transform.localScale = new Vector3(1f, 0.8f, 0.7f);
        
        // Renderer renderer = chest.GetComponent<Renderer>();
        // Material mat = new(Shader.Find("Standard")) { color = new Color(0.8f, 0.6f, 0.2f) };
        // renderer.material = mat;
        
        GameObject chest = ResourceService.LoadTreasurePrefab();
        chest.name = "TreasureChest";
        
        Collider collider = chest.GetComponent<Collider>();
        collider.isTrigger = true;
        
        TreasureController treasureController = chest.AddComponent<TreasureController>();
        treasureController.treasureType = TreasureController.TreasureType.Random;
        
        Debug.Log($"Treasure chest spawned in room {room.ID}");
        return 0;
    }
    
    private List<GameObject> GetBiomeEnemyPrefabs(string biome)
    {
        List<GameObject> prefabs = new();
        
        GameObject melee = ResourceService.LoadMeleeEnemyPrefab(biome);
        GameObject ranged = ResourceService.LoadRangedEnemyPrefab(biome);
        GameObject tank = ResourceService.LoadTankEnemyPrefab(biome);
        
        if (melee != null) prefabs.Add(melee);
        if (ranged != null) prefabs.Add(ranged);
        if (tank != null) prefabs.Add(tank);
        
        if (prefabs.Count == 0) Debug.LogError($"No enemy prefabs found for biome '{biome}'");
        
        return prefabs;
    }
    
    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        if (enemyPrefab == null) return null;
        
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, enemiesContainer);
        allEnemies.Add(enemy);
        
        return enemy;
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        
        allEnemies.Clear();
        hasSpawnedEnemies = false; // CRITICAL: Reset flag
        Debug.Log("EntityManager: All enemies cleared");
    }
    
    public void ClearAllEntities()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
        
        ClearAllEnemies();
        hasSpawnedEnemies = false; // CRITICAL: Reset flag
        
        Debug.Log("EntityManager: All entities cleared");
    }

    void OnDestroy()
    {
        LayoutManager.OnNavMeshReady -= OnNavMeshReady;
    }
}

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public bool hasSpawned = false;
    public bool bossDefeated = false;
    
    [Header("Rewards")]
    public PowerType guaranteedPower;
    public int goldReward = 500;
    
    private GameObject currentBoss;
    
    void Start()
    {
        // Auto-spawn boss after short delay
        if (!hasSpawned)
        {
            StartCoroutine(SpawnBossDelayed());
        }
    }
    
    private IEnumerator SpawnBossDelayed()
    {
        yield return new WaitForSeconds(2f);
        SpawnBoss();
    }
    
    private void SpawnBoss()
    {
        if (hasSpawned) return;
        
        hasSpawned = true;
        
        // Load boss prefab if not assigned
        if (bossPrefab == null)
        {
            LayoutManager layoutManager = FindObjectOfType<LayoutManager>();
            if (layoutManager != null)
            {
                BiomeManager biomeManager = layoutManager.GetComponent<BiomeManager>();
                if (biomeManager != null)
                {
                    bossPrefab = ResourceService.LoadBossEnemyPrefab(biomeManager.CurrentBiome);
                }
            }
        }
        
        if (bossPrefab == null)
        {
            Debug.LogError("BossSpawner: No boss prefab available!");
            return;
        }
        
        // Spawn boss at this position
        Vector3 spawnPosition = transform.position + Vector3.up;
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        currentBoss.name = "Boss";
        
        // Make boss stronger
        if (currentBoss.TryGetComponent<EnemyController>(out var bossController))
        {
            bossController.maxHealth = 200;
            bossController.damage = 25;
            bossController.moveSpeed = 3f;
            
            // Subscribe to boss death
            StartCoroutine(WaitForBossDeath(bossController));
        }
        
        Debug.Log("Boss spawned!");
    }
    
    private IEnumerator WaitForBossDeath(EnemyController boss)
    {
        // Wait until boss is destroyed
        while (currentBoss != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        OnBossDefeated();
    }
    
    private void OnBossDefeated()
    {
        if (bossDefeated) return;
        
        bossDefeated = true;
        Debug.Log("BOSS DEFEATED!");
        
        // Spawn rewards
        SpawnBossRewards();
        
        // Unlock exit if this is the boss room
        UnlockExit();
    }
    
    private void SpawnBossRewards()
    {
        // Spawn power pickup
        Vector3 rewardPosition = transform.position + Vector3.up;
        
        // Create power pickup object
        GameObject powerPickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerPickup.transform.position = rewardPosition;
        powerPickup.transform.localScale = Vector3.one * 0.5f;
        powerPickup.name = "BossPowerReward";
        
        // Make it glow
        Renderer renderer = powerPickup.GetComponent<Renderer>();
        Material glowMat = new(Shader.Find("Standard"))
        {
            color = Color.yellow
        };
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", Color.yellow * 2f);
        renderer.material = glowMat;
        
        // Add pickup script
        PowerPickup pickup = powerPickup.AddComponent<PowerPickup>();
        pickup.powerType = guaranteedPower;
        
        Debug.Log($"Boss dropped power: {guaranteedPower}");
    }
    
    private void UnlockExit()
    {
        // Find and unlock the exit portal
        ProgressionManager progressionManager = FindObjectOfType<ProgressionManager>();
        if (progressionManager != null)
        {
            // Enable exit portal
            Debug.Log("Exit portal unlocked!");
        }
    }
}