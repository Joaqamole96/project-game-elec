// -------------------------------------------------- //
// Scripts/Managers/LayoutManager.cs
// -------------------------------------------------- //

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(BiomeManager))]
public class LayoutManager : MonoBehaviour
{
    // Configuration
    public GameConfig GameConfig;
    public LevelConfig LevelConfig;
    public PartitionConfig PartitionConfig;
    public RoomConfig RoomConfig;
    
    // Environment Settings
    public bool EnableCeiling = true;
    public bool EnableVoid = true;
    
    // Parent Transforms
    public Transform FloorsParent;
    public Transform WallsParent;
    public Transform DoorsParent;
    public Transform LandmarksParent;
    public Transform EnvironmentParent;
    
    // Mobile Optimization
    public bool CombineMeshes = false;
    public bool EnableFloorCollision = true;
    public bool EnableWallCollision = true;
    public bool EnableDoorCollision = false;
    
    // Public Accessors
    public LevelModel CurrentLayout => _layout;

    public static System.Action OnNavMeshReady;
    
    // Private Fields - Generators
    private PartitionGenerator _partitionGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomGenerator _roomGenerator;
    private LayoutGenerator _layoutGenerator;
    private NavMeshGenerator _navMeshGenerator;
    
    // Private Fields - Services
    private ConfigService _configService;
    
    // Private Fields - Managers (MonoBehaviours)
    private BiomeManager _biomeManager;
    
    // Private Fields - Renderers
    private LayoutRenderer _layoutRenderer;
    private LandmarkRenderer _specialRenderer;
    private CorridorRenderer _corridorRenderer;
    
    // Private Fields - Data
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    
    // Combined mesh containers
    private readonly List<GameObject> _spawnedContainers = new();

    // Runtime config accessors
    private LevelConfig RuntimeLevelConfig => _configService?.LevelConfig ?? LevelConfig;
    private PartitionConfig RuntimePartitionConfig => _configService?.PartitionConfig ?? PartitionConfig;
    private RoomConfig RuntimeRoomConfig => _configService?.RoomConfig ?? RoomConfig;

    // ------------------------- //

    void Start() 
    {
        GenerateDungeon();
    }

    // ------------------------- //
    // COMPONENT INITIALIZATION
    // ------------------------- //

    private void InitializeAllComponents()
    {
        Debug.Log("Initializing components for generation...");
        
        InitializeManagers();
        InitializeServices();
        InitializeGenerators();
        InitializeRenderers();
        
        Debug.Log("All components initialized successfully");
    }

    private void InitializeManagers()
    {
        if (_biomeManager == null)
        {
            _biomeManager = GetOrAddComponent<BiomeManager>();
        }

        if (_navMeshGenerator == null)
        {
            _navMeshGenerator = GetOrAddComponent<NavMeshGenerator>();
        }
        
        Debug.Log("Managers initialized");
    }

    private void InitializeServices()
    {
        int seed = LevelConfig?.Seed ?? 0;
        
        _configService = new ConfigService(GameConfig, LevelConfig, PartitionConfig, RoomConfig);
        _random = new System.Random(seed);
        
        if (_biomeManager != null)
        {
            _biomeManager.InitializeRandom(seed);
        }
        
        Debug.Log($"Services initialized with seed: {seed}");
    }

    private void InitializeGenerators()
    {
        int seed = RuntimeLevelConfig.Seed;
        
        _partitionGenerator = new PartitionGenerator(seed);
        _corridorGenerator = new CorridorGenerator(seed);
        _roomGenerator = new RoomGenerator(seed);
        _layoutGenerator = new LayoutGenerator();
        
        Debug.Log("Generators initialized");
    }

    private void InitializeRenderers()
    {
        if (_biomeManager == null)
        {
            Debug.LogError("Cannot initialize renderers: BiomeManager is null");
            return;
        }

        FloorsParent = CreateParentIfNull(FloorsParent, "Floors");
        WallsParent = CreateParentIfNull(WallsParent, "Walls");
        DoorsParent = CreateParentIfNull(DoorsParent, "Doors");
        LandmarksParent = CreateParentIfNull(LandmarksParent, "Landmarks");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
        
        try
        {
            // FIXED: Use GetLandmarkPrefab instead of deprecated method
            _specialRenderer = new LandmarkRenderer(
                ResourceService.LoadEntrancePrefab(), 
                ResourceService.LoadExitPrefab(), 
                _biomeManager
            );
            
            _layoutRenderer = new LayoutRenderer();
            _corridorRenderer = new CorridorRenderer();
            
            Debug.Log("Renderers initialized");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize renderers: {ex.Message}");
        }
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        if (!TryGetComponent<T>(out var component))
        {
            component = gameObject.AddComponent<T>();
            Debug.Log($"Added missing component: {typeof(T).Name}");
        }
        return component;
    }

    // ------------------------- //
    // DUNGEON GENERATION
    // ------------------------- //

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        InitializeAllComponents();

        var stopwatch = Stopwatch.StartNew();

        ClearPreviousGeneration();
        ValidateConfigs();
        
        Debug.Log($"Generating floor {RuntimeLevelConfig.FloorLevel} with size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");
        
        ExecuteGenerationPipeline();
        
        stopwatch.Stop();

        Debug.Log($"Generated Floor {RuntimeLevelConfig.FloorLevel}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms");
    }

    private void ExecuteGenerationPipeline()
    {
        _layout = GenerateDungeonLayout();
        if (_layout == null)
        {
            Debug.LogError("Layout generation failed!");
            return;
        }

        _rooms = _roomGenerator.AssignRooms(_layout, RuntimeLevelConfig.FloorLevel);
        if (_rooms == null || _rooms.Count == 0)
        {
            Debug.LogError("Room assignment failed!");
            return;
        }

        _layoutGenerator.GenerateLayout(_layout);
        _layout.InitializeSpatialData();
        
        RenderDungeon();
        BakeNavMeshForDungeon();
        NotifyDungeonReady();
    }

    private LevelModel GenerateDungeonLayout()
    {
        var layout = new LevelModel();
        
        var root = _partitionGenerator.GeneratePartitionTree(RuntimeLevelConfig, RuntimePartitionConfig);
        var leaves = _partitionGenerator.CollectLeaves(root);
        layout.Rooms = _roomGenerator.CreateRoomsFromPartitions(leaves, RuntimeRoomConfig);
        _roomGenerator.FindAndAssignNeighbors(leaves);
        
        var allCorridors = _corridorGenerator.GenerateAllPossibleCorridors(leaves);
        layout.Corridors = MSTService.Apply(allCorridors, layout.Rooms);
        
        return layout;
    }

    private void RenderDungeon()
    {
        ClearRendering();
        CreateParentContainers();

        // Use ProBuilder renderer instead of tile-by-tile
        RenderProBuilderMode(_layout, _rooms, RuntimeLevelConfig.FloorLevel);
        RenderLandmarks(_rooms);
        
        LogRenderingResults();
    }

    private void RenderProBuilderMode(LevelModel layout, List<RoomModel> rooms, int floorLevel)
    {
        if (_layoutRenderer == null || _corridorRenderer == null)
        {
            Debug.LogError("ProBuilderRenderer or CorridorRenderer not initialized!");
            return;
        }
        
        string currentBiome = _biomeManager.GetBiomeForFloor(floorLevel);
        Debug.Log($"Rendering with ProBuilder for biome: {currentBiome}");
        
        // Render all rooms optimally (1 floor, 4 walls, 4 corners, N doorways per room)
        _layoutRenderer.RenderAllRooms(layout, rooms, FloorsParent, currentBiome);
        
        // Render corridors as stretched segments (NEW - replaces tile-by-tile)
        _corridorRenderer.RenderCorridors(layout, rooms, FloorsParent, currentBiome);
        
        // Render environment
        RenderEnvironment(layout);
    }

    private void RenderCorridors(LevelModel layout, string biome)
    {
        if (layout.Corridors == null || layout.Corridors.Count == 0) return;
        
        GameObject floorPrefab = Resources.Load<GameObject>("Layout/pf_Floor");
        GameObject wallPrefab = Resources.Load<GameObject>("Layout/pf_Wall");
        Material floorMat = Resources.Load<Material>($"Layout/{biome}/FloorMaterial");
        Material wallMat = Resources.Load<Material>($"Layout/{biome}/WallMaterial");
        
        if (floorPrefab == null || wallPrefab == null) return;
        
        // Create corridor container
        GameObject corridorContainer = new("Corridors");
        corridorContainer.transform.SetParent(FloorsParent);
        
        // Get all corridor floor tiles (excluding room tiles)
        HashSet<Vector2Int> corridorFloors = new();
        foreach (var corridor in layout.Corridors)
        {
            if (corridor?.Tiles == null) continue;
            foreach (var tile in corridor.Tiles)
            {
                // Check if tile is not in any room
                bool inRoom = false;
                foreach (var room in _rooms)
                {
                    if (room.ContainsPosition(tile))
                    {
                        inRoom = true;
                        break;
                    }
                }
                
                if (!inRoom)
                {
                    corridorFloors.Add(tile);
                }
            }
        }
        
        // Render corridor floors
        foreach (var tilePos in corridorFloors)
        {
            Vector3 worldPos = new(tilePos.x + 0.5f, 0.5f, tilePos.y + 0.5f);
            GameObject floor = Object.Instantiate(floorPrefab, worldPos, Quaternion.identity, corridorContainer.transform);
            floor.name = $"CorridorFloor_{tilePos.x}_{tilePos.y}";
            
            if (floorMat != null)
            {
                Renderer renderer = floor.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = floorMat;
            }
        }
        
        // Render corridor walls
        GameObject wallContainer = new("CorridorWalls");
        wallContainer.transform.SetParent(WallsParent);
        
        foreach (var tilePos in corridorFloors)
        {
            // Check each cardinal direction for walls
            Vector2Int[] directions = new Vector2Int[]
            {
                new(0, 1),  // North
                new(0, -1), // South
                new(1, 0),  // East
                new(-1, 0)  // West
            };
            
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int dir = directions[i];
                Vector2Int checkPos = tilePos + dir;
                
                // If neighbor is not a floor tile, place wall
                if (!layout.AllFloorTiles.Contains(checkPos))
                {
                    Vector3 worldPos = new(checkPos.x + 0.5f, 5.5f, checkPos.y + 0.5f);
                    Quaternion rotation = GetWallRotationFromDirection(dir);
                    
                    GameObject wall = Object.Instantiate(wallPrefab, worldPos, rotation, wallContainer.transform);
                    wall.name = $"CorridorWall_{checkPos.x}_{checkPos.y}";
                    
                    if (wallMat != null)
                    {
                        Renderer renderer = wall.GetComponent<Renderer>();
                        if (renderer != null) renderer.sharedMaterial = wallMat;
                    }
                }
            }
        }
        
        Debug.Log($"Rendered {corridorFloors.Count} corridor tiles");
    }

    private Quaternion GetWallRotationFromDirection(Vector2Int direction)
    {
        // Corridor walls: Same logic as room walls
        // Z=0.75 is thickness, Y-rot=0 means facing Z axis
        
        // If checking North/South neighbor (direction.y != 0)
        // Wall should extend East/West (along X axis), so needs Y-rot = 90
        if (direction.y != 0)
        {
            return Quaternion.Euler(0, 90, 0);
        }
        // If checking East/West neighbor (direction.x != 0)
        // Wall should extend North/South (along Z axis), so needs Y-rot = 0
        else
        {
            return Quaternion.Euler(0, 0, 0);
        }
    }

    private void RenderEnvironment(LevelModel layout)
    {
        if (EnableCeiling) RenderCeiling(layout);
        if (EnableVoid) RenderVoidPlane(layout);
    }

    private void RenderCeiling(LevelModel layout)
    {
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(EnvironmentParent);
        
        BoundsInt bounds = layout.OverallBounds;
        Vector3 center = new(bounds.center.x, 11f, bounds.center.y);
        ceiling.transform.position = center;
        
        float scaleX = bounds.size.x * 0.1f;
        float scaleZ = bounds.size.y * 0.1f;
        ceiling.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        Renderer renderer = ceiling.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = new Color(0.3f, 0.3f, 0.3f)
        };
        renderer.sharedMaterial = mat;
        
        Object.DestroyImmediate(ceiling.GetComponent<Collider>());
    }

    private void RenderVoidPlane(LevelModel layout)
    {
        GameObject voidPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        voidPlane.name = "VoidPlane";
        voidPlane.transform.SetParent(EnvironmentParent);
        
        BoundsInt bounds = layout.OverallBounds;
        Vector3 center = new(bounds.center.x, -5f, bounds.center.y);
        voidPlane.transform.position = center;
        
        float scaleX = bounds.size.x * 0.2f;
        float scaleZ = bounds.size.y * 0.2f;
        voidPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        Renderer renderer = voidPlane.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = Color.black
        };
        renderer.sharedMaterial = mat;
    }

    private void RenderLandmarks(List<RoomModel> rooms)
    {
        _specialRenderer.RenderLandmarks(rooms, LandmarksParent);
    }

    private void BakeNavMeshForDungeon()
    {
        if (_navMeshGenerator != null && _layout != null)
        {
            Debug.Log("LayoutManager: Baking NavMesh for dungeon (SYNCHRONOUS)...");
            
            // Bake immediately (not via coroutine)
            _navMeshGenerator.BakeNavMesh(_layout, FloorsParent, WallsParent);
            
            // Verify it worked
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            if (triangulation.vertices.Length > 0)
            {
                Debug.Log($"LayoutManager: NavMesh baked successfully - {triangulation.vertices.Length} vertices");
            }
            else
            {
                Debug.LogError("LayoutManager: NavMesh bake FAILED - no geometry generated!");
            }
        }
        else
        {
            Debug.LogWarning("LayoutManager: Cannot bake NavMesh - generator or layout is null");
        }
    }

    private void NotifyDungeonReady()
    {
        // Notify that dungeon AND NavMesh are ready
        OnNavMeshReady?.Invoke();
        Debug.Log("LayoutManager: Dungeon generation complete, NavMesh ready");
    }

    // ------------------------- //
    // FLOOR PROGRESSION
    // ------------------------- //

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        RuntimeLevelConfig.FloorLevel++;

        if (RuntimeLevelConfig.FloorLevel > 1)
        {
            GrowFloorSize();
        }

        Debug.Log($"Moving to floor {RuntimeLevelConfig.FloorLevel} - New size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");
        GenerateDungeon();
    }

    private void GrowFloorSize()
    {
        bool growWidth = _random.NextDouble() > 0.5;
        if (growWidth) 
            RuntimeLevelConfig.Width = Mathf.Min(RuntimeLevelConfig.Width + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
        else 
            RuntimeLevelConfig.Height = Mathf.Min(RuntimeLevelConfig.Height + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
    }

    // ------------------------- //
    // UTILITY METHODS
    // ------------------------- //

    private void ValidateConfigs()
    {
        RuntimeLevelConfig?.Validate();
        RuntimePartitionConfig?.Validate();
        RuntimeRoomConfig?.Validate();
    }

    private void ClearPreviousGeneration()
    {
        _layout = null;
        _rooms = null;
    }

    private string GetRoomTypeBreakdown()
    {
        if (_rooms == null) return "No rooms";
        return string.Join(", ", _rooms
            .GroupBy(r => r.Type)
            .Select(g => $"{g.Key}: {g.Count()}")
        );
    }

    public Vector3 GetEntranceRoomPosition()
    {
        if (_rooms == null) throw new System.Exception("GetEntranceRoomPosition: _rooms is null");
        
        var entrance = _rooms.FirstOrDefault(room => room.Type == RoomType.Entrance) 
            ?? throw new System.Exception("GetEntranceRoomPosition: No entrance room found");
        
        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }

    private void CreateParentContainers()
    {
        FloorsParent = CreateParentIfNull(FloorsParent, "Floors");
        WallsParent = CreateParentIfNull(WallsParent, "Walls");
        DoorsParent = CreateParentIfNull(DoorsParent, "Doors");
        LandmarksParent = CreateParentIfNull(LandmarksParent, "Landmarks");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
    }

    private Transform CreateParentIfNull(Transform parent, string name)
    {
        return parent != null ? parent : CreateParent(name);
    }

    private Transform CreateParent(string name)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform);
        return go.transform;
    }

    // ------------------------- //
    // CLEANUP
    // ------------------------- //

    public void ClearRendering()
    {
        if (_navMeshGenerator != null)
        {
            _navMeshGenerator.ClearNavMesh();
        }
        
        ClearSpawnedContainers();
        ClearAllChildObjects();
    }

    private void ClearSpawnedContainers()
    {
        foreach (var container in _spawnedContainers)
            if (container != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(container);
                #else
                Destroy(container);
                #endif
            }
        _spawnedContainers.Clear();
    }

    private void ClearAllChildObjects()
    {
        ClearChildObjects(FloorsParent);
        ClearChildObjects(WallsParent);
        ClearChildObjects(DoorsParent);
        ClearChildObjects(LandmarksParent);
        ClearChildObjects(EnvironmentParent);
    }

    private void ClearChildObjects(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
    }

    private void LogRenderingResults()
    {
        Debug.Log($"Rendering complete - Floors: {FloorsParent?.childCount ?? 0}, Walls: {WallsParent?.childCount ?? 0}, Doors: {DoorsParent?.childCount ?? 0}");
    }
}