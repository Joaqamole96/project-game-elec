// -------------------------------------------------- //
// Scripts/Managers/LayoutManager.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;
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
    
    // Private Fields - Generators
    private PartitionGenerator _partitionGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomGenerator _roomGenerator;
    private LayoutGenerator _layoutGenerator;
    private NavMeshGenerator _navMeshGenerator;
    
    // Private Fields - Services
    private ConfigService _configService;
    private MaterialService _materialService;
    
    // Private Fields - Managers (MonoBehaviours)
    private BiomeManager _biomeManager;
    
    // Private Fields - Renderers
    private PrefabFloorRenderer _floorRenderer;
    private PrefabWallRenderer _wallRenderer;
    private PrefabDoorRenderer _doorRenderer;
    private LandmarkRenderer _specialRenderer;
    private OptimizedPrefabRenderer _optimizedRenderer;
    
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
        
        var defaultBiome = _biomeManager.GetBiomeForFloor(RuntimeLevelConfig.FloorLevel);
        
        try
        {
            _floorRenderer = new PrefabFloorRenderer(_biomeManager.GetFloorPrefab(defaultBiome), _biomeManager);
            _wallRenderer = new PrefabWallRenderer(_biomeManager.GetWallPrefab(defaultBiome), _biomeManager);
            _doorRenderer = new PrefabDoorRenderer(_biomeManager.GetDoorPrefab(defaultBiome), _biomeManager);
            
            // FIXED: Use GetLandmarkPrefab instead of deprecated method
            _specialRenderer = new LandmarkRenderer(
                _biomeManager.GetLandmarkPrefab(RoomType.Entrance), 
                _biomeManager.GetLandmarkPrefab(RoomType.Exit), 
                _biomeManager
            );
            
            _optimizedRenderer = new OptimizedPrefabRenderer(_biomeManager);
            
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
        layout.Corridors = MinimumSpanningTree.Apply(allCorridors, layout.Rooms);
        
        return layout;
    }

    private void RenderDungeon()
    {
        ClearRendering();
        CreateParentContainers();

        RenderRealMode(_layout, RuntimeLevelConfig.FloorLevel);
        RenderLandmarks(_rooms);
        LogRenderingResults();
    }

    private void RenderRealMode(LevelModel layout, int floorLevel)
    {
        _optimizedRenderer.SetBiomeForFloor(floorLevel);
        
        _optimizedRenderer.RenderFloorsByRoom(layout, _rooms, FloorsParent);
        _optimizedRenderer.RenderWallsOptimized(layout, WallsParent);
        _optimizedRenderer.RenderDoorsOptimized(layout, DoorsParent);
        
        _optimizedRenderer.FinalizeRendering(FloorsParent, WallsParent);
        
        RenderEnvironment(layout);
    }

    private void RenderEnvironment(LevelModel layout)
    {
        if (EnableCeiling) _optimizedRenderer.RenderCeilingOptimized(layout, EnvironmentParent);
        if (EnableVoid) _optimizedRenderer.RenderVoidPlane(layout, EnvironmentParent);
    }

    private void RenderLandmarks(List<RoomModel> rooms)
    {
        _specialRenderer.RenderLandmarks(rooms, LandmarksParent);
    }

    private void BakeNavMeshForDungeon()
    {
        if (_navMeshGenerator != null && _layout != null)
        {
            Debug.Log("Baking NavMesh for dungeon...");
            StartCoroutine(BakeNavMeshDelayed());
        }
        else
        {
            Debug.LogWarning("Cannot bake NavMesh: generator or layout is null");
        }
    }

    private System.Collections.IEnumerator BakeNavMeshDelayed()
    {
        yield return new WaitForFixedUpdate();
        _navMeshGenerator.BakeNavMesh(_layout, FloorsParent, WallsParent);
    }

    private void NotifyDungeonReady()
    {
        // Systems notified here
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
        CleanupMaterials();
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

    private void CleanupMaterials()
    {
        _materialService?.CleanupMaterialCache();
    }

    private void LogRenderingResults()
    {
        Debug.Log($"Rendering complete - Floors: {FloorsParent?.childCount ?? 0}, Walls: {WallsParent?.childCount ?? 0}, Doors: {DoorsParent?.childCount ?? 0}");
    }
}