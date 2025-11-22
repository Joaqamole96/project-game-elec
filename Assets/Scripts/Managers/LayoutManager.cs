// -------------------------------------------------- //
// Scripts/Managers/LayoutManager.cs
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
    
    // Rendering Mode
    public RenderMode Mode = RenderMode.Real;
    
    // Environment Settings
    public bool EnableCeiling = true;
    public bool EnableVoid = true;
    
    // Parent Transforms
    public Transform FloorsParent;
    public Transform WallsParent;
    public Transform DoorsParent;
    public Transform SpecialObjectsParent;
    public Transform EnvironmentParent;
    
    // Mobile Optimization
    public bool CombineMeshes = false;
    public bool EnableFloorCollision = true;
    public bool EnableWallCollision = true;
    public bool EnableDoorCollision = false;
    
    // Material Settings - Gizmo Mode
    public Material DefaultFloorMaterial;
    public Material DefaultWallMaterial;
    public Material DefaultDoorMaterial;
    
    // Public Accessors
    public LevelModel CurrentLayout => _layout;
    
    // Private Fields
    private PartitionGenerator _partitionGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomGenerator _roomGenerator;
    private LayoutGenerator _layoutGenerator;
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    private ConfigService _configService;
    
    // Rendering components
    private IFloorRenderer _floorRenderer;
    private IWallRenderer _wallRenderer;
    private IDoorRenderer _doorRenderer;
    private SpecialRoomRenderer _specialRenderer;
    private MaterialManager _materialManager;
    private OptimizedPrefabRenderer _optimizedRenderer;
    private BiomeManager _biomeManager;
    
    // Combined mesh containers
    private readonly List<GameObject> _spawnedContainers = new();

    // Runtime config accessors
    private LevelConfig RuntimeLevelConfig => _configService?.LevelConfig ?? LevelConfig;
    private PartitionConfig RuntimePartitionConfig => _configService?.PartitionConfig ?? PartitionConfig;
    private RoomConfig RuntimeRoomConfig => _configService?.RoomConfig ?? RoomConfig;

    public enum RenderMode { Gizmo, Real }

    // ------------------------- //
    
    void Awake()
    {
        InitializeComponents();
    }

    void Start() => GenerateDungeon();

    // ------------------------- //

    private void InitializeComponents()
    {
        int seed = RuntimeLevelConfig.Seed;

        // Initialize core components
        _partitionGenerator ??= new(seed);
        _corridorGenerator ??= new(seed);
        _roomGenerator ??= new(seed);
        _layoutGenerator ??= new();
        _configService ??= new(GameConfig, LevelConfig, PartitionConfig, RoomConfig);
        _random ??= new(RuntimeLevelConfig.Seed);

        // Initialize rendering components
        InitializeRenderingComponents();
    }

    private void InitializeRenderingComponents()
    {
        _materialManager = new MaterialManager(DefaultFloorMaterial, DefaultWallMaterial, DefaultDoorMaterial);
        _biomeManager = new BiomeManager(RuntimeLevelConfig.Seed);
        _optimizedRenderer = new OptimizedPrefabRenderer(_biomeManager);

        // Find or create parent transforms
        FloorsParent = CreateParentIfNull(FloorsParent, "Floors");
        WallsParent = CreateParentIfNull(WallsParent, "Walls");
        DoorsParent = CreateParentIfNull(DoorsParent, "Doors");
        SpecialObjectsParent = CreateParentIfNull(SpecialObjectsParent, "SpecialObjects");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
        
        InitializeRenderers();
        
        _specialRenderer = new SpecialRoomRenderer(
            _biomeManager.GetSpecialRoomPrefab(RoomType.Entrance), 
            _biomeManager.GetSpecialRoomPrefab(RoomType.Exit), 
            _biomeManager
        );
    }

    private void InitializeRenderers()
    {
        if (Mode == RenderMode.Gizmo)
        {
            _floorRenderer = new GizmoFloorRenderer(_materialManager);
            _wallRenderer = new GizmoWallRenderer(_materialManager);
            _doorRenderer = new GizmoDoorRenderer(_materialManager);
        }
        else
        {
            _floorRenderer = new PrefabFloorRenderer(_biomeManager.GetPrefab("Biomes/Default/FloorPrefab"), _materialManager, _biomeManager);
            _wallRenderer = new PrefabWallRenderer(_biomeManager.GetPrefab("Biomes/Default/WallPrefab"), _materialManager, _biomeManager);
            _doorRenderer = new PrefabDoorRenderer(_biomeManager.GetPrefab("Biomes/Default/DoorPrefab"), _materialManager, _biomeManager);
        }
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        InitializeComponents();

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
        // Phase 1: Generate layout
        _layout = GenerateDungeonLayout();
        if (_layout == null) return;

        // Phase 2: Assign room types
        _rooms = _roomGenerator.AssignRooms(_layout, RuntimeLevelConfig.FloorLevel);
        if (_rooms == null || _rooms.Count == 0) return;

        // Phase 3: Build geometry
        _layoutGenerator.GenerateLayout(_layout);
        _layout.InitializeSpatialData();
        
        // Phase 4: Render
        RenderDungeon();
        
        // Phase 5: Notify systems
        NotifyDungeonReady();
    }

    private void RenderDungeon()
    {
        EnsureRenderingComponentsInitialized();
        ClearRendering();
        CreateParentContainers();

        if (Mode == RenderMode.Gizmo)
        {
            RenderGizmoMode(_layout, _rooms);
        }
        else
        {
            RenderRealMode(_layout, RuntimeLevelConfig.FloorLevel);
        }
        
        RenderSpecialObjects(_layout, _rooms);
        LogRenderingResults();
    }

    private void RenderGizmoMode(LevelModel layout, List<RoomModel> rooms)
    {
        _materialManager.InitializeMaterialCache();
        RenderFloors(layout, rooms);
        RenderWalls(layout);
        RenderDoors(layout);
    }

    private void RenderRealMode(LevelModel layout, int floorLevel)
    {
        _optimizedRenderer.SetBiomeForFloor(floorLevel);
        
        // Queue geometry for combining
        _optimizedRenderer.RenderFloorsOptimized(layout, FloorsParent);
        _optimizedRenderer.RenderWallsOptimized(layout, WallsParent);
        _optimizedRenderer.RenderDoorsOptimized(layout, DoorsParent);
        
        // Build combined meshes with correct parents
        _optimizedRenderer.FinalizeRendering(FloorsParent, WallsParent);
        
        RenderEnvironment(layout);
    }

    private void RenderEnvironment(LevelModel layout)
    {
        if (EnableCeiling) _optimizedRenderer.RenderCeilingOptimized(layout, EnvironmentParent);
        if (EnableVoid) _optimizedRenderer.RenderVoidPlane(layout, EnvironmentParent);
    }

    private void NotifyDungeonReady()
    {
        GetComponent<PlayerSpawner>()?.OnDungeonGenerated();
    }

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
        {
            RuntimeLevelConfig.Width = Mathf.Min(RuntimeLevelConfig.Width + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
        }
        else
        {
            RuntimeLevelConfig.Height = Mathf.Min(RuntimeLevelConfig.Height + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
        }
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

    private void ValidateConfigs()
    {
        RuntimeLevelConfig.Validate();
        RuntimePartitionConfig.Validate();
        RuntimeRoomConfig.Validate();
    }

    private void ClearPreviousGeneration()
    {
        _layout = null;
        _rooms = null;
        ClearRendering();
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
        if (_rooms == null) throw new("GetEntranceRoomPosition: _rooms is null");
        
        var entrance = _rooms.FirstOrDefault(room => room.Type == RoomType.Entrance) ?? throw new("GetEntranceRoomPosition: No entrance room found");
        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }

    #region Rendering Methods (formerly in LayoutRenderer)
    private void EnsureRenderingComponentsInitialized()
    {
        if (_materialManager == null)
            InitializeRenderingComponents();
    }

    #region Rendering Orchestration
    private void RenderFloors(LevelModel layout, List<RoomModel> rooms)
    {
        if (layout?.AllFloorTiles == null || rooms == null) 
        {
            Debug.LogError("Cannot render floors: layout or rooms is null");
            return;
        }
        
        Debug.Log($"Starting floor rendering: {layout.AllFloorTiles.Count} floor tiles, {rooms.Count} rooms");
        
        if (CombineMeshes && Mode == RenderMode.Gizmo)
        {
            RenderCombinedFloors(layout, rooms);
        }
        else
        {
            RenderIndividualFloors(layout, rooms);
        }
        
        LogFloorRenderingResults();
    }

    private void RenderCombinedFloors(LevelModel layout, List<RoomModel> rooms)
    {
        Debug.Log("Using combined mesh rendering for floors");
        var floorMeshes = _floorRenderer.RenderCombinedFloorsByRoomType(layout, rooms, FloorsParent);
        _spawnedContainers.AddRange(floorMeshes);
        
        if (EnableFloorCollision)
        {
            foreach (var mesh in floorMeshes)
                AddCollisionToObject(mesh, "Floor");
        }
        
        Debug.Log($"Combined mesh rendering complete: {floorMeshes.Count} mesh objects created");
    }

    private void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms)
    {
        Debug.Log("Using individual floor rendering");
        _floorRenderer.RenderIndividualFloors(layout, rooms, FloorsParent, EnableFloorCollision);
    }

    private void RenderWalls(LevelModel layout)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null) return;
        
        if (CombineMeshes && Mode == RenderMode.Gizmo)
        {
            var wallMeshes = _wallRenderer.RenderCombinedWallsByType(layout, WallsParent);
            _spawnedContainers.AddRange(wallMeshes);
            
            if (EnableWallCollision)
            {
                foreach (var mesh in wallMeshes)
                    AddCollisionToObject(mesh, "Wall");
            }
        }
        else
        {
            _wallRenderer.RenderIndividualWalls(layout, WallsParent, EnableWallCollision);
        }
    }

    private void RenderDoors(LevelModel layout)
    {
        if (layout?.AllDoorTiles == null) return;
        _doorRenderer.RenderDoors(layout, DoorsParent, EnableDoorCollision);
    }

    private void RenderSpecialObjects(LevelModel layout, List<RoomModel> rooms)
    {
        if (Mode == RenderMode.Real)
        {
            _specialRenderer.RenderSpecialObjects(layout, rooms, SpecialObjectsParent);
        }
    }
    #endregion

    #region Utility Methods
    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;

        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
        }

        if (objectType == "Door")
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
    }

    private void CreateParentContainers()
    {
        FloorsParent = CreateParentIfNull(FloorsParent, "Floors");
        WallsParent = CreateParentIfNull(WallsParent, "Walls");
        DoorsParent = CreateParentIfNull(DoorsParent, "Doors");
        SpecialObjectsParent = CreateParentIfNull(SpecialObjectsParent, "SpecialObjects");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
    }

    private Transform CreateParentIfNull(Transform parent, string name)
    {
        return parent ?? CreateParent(name);
    }

    private Transform CreateParent(string name)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform);
        return go.transform;
    }

    private void ClearSpawnedContainers()
    {
        foreach (var container in _spawnedContainers)
        {
            if (container != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(container);
                #else
                Destroy(container);
                #endif
            }
        }
        _spawnedContainers.Clear();
    }

    private void ClearAllChildObjects()
    {
        ClearChildObjects(FloorsParent);
        ClearChildObjects(WallsParent);
        ClearChildObjects(DoorsParent);
        ClearChildObjects(SpecialObjectsParent);
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
        _materialManager?.CleanupMaterialCache();
    }

    public void ClearRendering()
    {
        EnsureRenderingComponentsInitialized();
        
        ClearSpawnedContainers();
        ClearAllChildObjects();
        CleanupMaterials();
    }

    private void LogFloorRenderingResults()
    {
        int renderedFloors = FloorsParent?.childCount ?? 0;
        Debug.Log($"Floor rendering complete: {renderedFloors} floor objects in scene");
    }

    private void LogRenderingResults()
    {
        Debug.Log($"Rendering complete - Floors: {FloorsParent.childCount}, Walls: {WallsParent.childCount}, Doors: {DoorsParent.childCount}");
    }
    #endregion
    #endregion
}