using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private LevelConfig levelConfig;
    [SerializeField] private PartitionConfig partitionConfig;
    [SerializeField] private RoomConfig roomConfig;
    
    [Header("References")]
    public DungeonRenderer Renderer;
    
    // Algorithm components
    private BSPGenerator _bspGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomAssigner _roomAssigner;
    private GeometryBuilder _geometryBuilder;
    
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    
    // NEW: Runtime config copies
    private RuntimeConfigs _runtimeConfigs;
    
    // NEW: Helper properties that use runtime configs
    private GameConfig RuntimeGameConfig => _runtimeConfigs?.GameConfig ?? gameConfig;
    private LevelConfig RuntimeLevelConfig => _runtimeConfigs?.LevelConfig ?? levelConfig;
    private PartitionConfig RuntimePartitionConfig => _runtimeConfigs?.PartitionConfig ?? partitionConfig;
    private RoomConfig RuntimeRoomConfig => _runtimeConfigs?.RoomConfig ?? roomConfig;
    
    public List<RoomModel> CurrentRooms => _rooms;
    public LevelModel CurrentLayout => _layout;
    
    void Awake()
    {
        InitializeComponents();
        InitializeRuntimeConfigs(); // NEW: Initialize runtime configs
        InitializeRandom();
    }

    void Start() => GenerateDungeon();

    // NEW: Initialize runtime config copies
    private void InitializeRuntimeConfigs()
    {
        _runtimeConfigs = new RuntimeConfigs(gameConfig, levelConfig, partitionConfig, roomConfig);
        Debug.Log($"Runtime configs initialized - Starting floor: {RuntimeLevelConfig.FloorLevel}");
    }

    private void InitializeComponents()
    {
        _bspGenerator = new BSPGenerator();
        _corridorGenerator = new CorridorGenerator();
        _roomAssigner = new RoomAssigner();
        _geometryBuilder = new GeometryBuilder();
    }

    private void EnsureComponentsInitialized()
    {
        if (_bspGenerator == null || _corridorGenerator == null || _roomAssigner == null || _geometryBuilder == null)
        {
            InitializeComponents();
        }
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        EnsureComponentsInitialized();
        InitializeRandom();
        var stopwatch = Stopwatch.StartNew();

        ClearPreviousGeneration();
        ValidateConfigs();
        
        // NEW: Log runtime config state
        Debug.Log($"Generating floor {RuntimeLevelConfig.FloorLevel} with size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");
        
        // Phase 1: Generate layout
        Debug.Log("Phase 1: Generating dungeon layout...");
        _layout = GenerateDungeonLayout();
        if (_layout == null) return;

        // Phase 2: Assign room types
        Debug.Log("Phase 2: Assigning room types...");
        _rooms = _roomAssigner.AssignRooms(_layout, RuntimeLevelConfig.FloorLevel, _random); // CHANGED: Use runtime config
        if (_rooms == null || _rooms.Count == 0) return;

        // Phase 3: Build geometry
        Debug.Log("Phase 3: Building final geometry...");
        _geometryBuilder.BuildFinalGeometry(_layout);
        _layout.InitializeSpatialData();
        
        // Phase 4: Render
        Debug.Log("Phase 4: Rendering dungeon...");
        if (Renderer != null)
        {
            Renderer.RenderDungeon(_layout, _rooms);
        }
        else
        {
            Debug.LogWarning("No renderer assigned!");
        }

        // Notify PlayerSpawner that dungeon is ready
        var playerSpawner = GetComponent<PlayerSpawner>();
        if (playerSpawner != null)
        {
            playerSpawner.OnDungeonGenerated();
        }
        
        stopwatch.Stop();
        Debug.Log($"Generated Floor {RuntimeLevelConfig.FloorLevel}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms"); // CHANGED: Use runtime config
    }

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        // NEW: Use runtime config for floor progression
        RuntimeLevelConfig.FloorLevel++;
        _random = new System.Random(RuntimeLevelConfig.Seed + RuntimeLevelConfig.FloorLevel);

        if (RuntimeLevelConfig.FloorLevel > 1)
        {
            bool growWidth = RandomBool();
            if (growWidth)
            {
                int newWidth = Mathf.Min(RuntimeLevelConfig.Width + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
                Debug.Log($"Growing width from {RuntimeLevelConfig.Width} to {newWidth}");
                RuntimeLevelConfig.Width = newWidth;
            }
            else
            {
                int newHeight = Mathf.Min(RuntimeLevelConfig.Height + RuntimeLevelConfig.FloorGrowth, RuntimeLevelConfig.MaxFloorSize);
                Debug.Log($"Growing height from {RuntimeLevelConfig.Height} to {newHeight}");
                RuntimeLevelConfig.Height = newHeight;
            }
        }

        Debug.Log($"Moving to floor {RuntimeLevelConfig.FloorLevel} - New size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");

        GenerateDungeon();
    }

    private LevelModel GenerateDungeonLayout()
    {
        var layout = new LevelModel();
        
        // CHANGED: Use runtime configs
        var root = _bspGenerator.GeneratePartitionTree(RuntimeLevelConfig, RuntimePartitionConfig, _random);
        var leaves = _bspGenerator.CollectLeafPartitions(root);
        layout.Rooms = _bspGenerator.CreateRoomsFromPartitions(leaves, RuntimeRoomConfig, _random);
        _bspGenerator.FindAndAssignNeighbors(leaves);
        
        var allCorridors = _corridorGenerator.GenerateAllPossibleCorridors(leaves, _random);
        layout.Corridors = MinimumSpanningTree.Apply(allCorridors, layout.Rooms);
        
        return layout;
    }

    #region Helper Methods
    private void InitializeRandom()
        => _random ??= new System.Random(RuntimeLevelConfig.Seed); // CHANGED: Use runtime config

    private bool RandomBool() 
        => _random.NextDouble() > 0.5;

    private void ValidateConfigs()
    {
        // CHANGED: Use runtime configs for validation
        RuntimeLevelConfig.Width = Mathf.Clamp(RuntimeLevelConfig.Width, 10, 1000); // Increased max for growth
        RuntimeLevelConfig.Height = Mathf.Clamp(RuntimeLevelConfig.Height, 10, 1000);
        RuntimePartitionConfig.MinPartitionSize = Mathf.Max(3, RuntimePartitionConfig.MinPartitionSize);
        RuntimeRoomConfig.MinRoomSize = Mathf.Max(3, RuntimeRoomConfig.MinRoomSize);
        RuntimeRoomConfig.MaxRooms = Mathf.Max(1, RuntimeRoomConfig.MaxRooms);
    }

    private void ClearPreviousGeneration()
    {
        _layout = null;
        _rooms = null;
        Renderer?.ClearRendering();
    }

    private string GetRoomTypeBreakdown()
    {
        if (_rooms == null) return "No rooms";
        return string.Join(", ", _rooms.GroupBy(r => r.Type).Select(g => $"{g.Key}: {g.Count()}"));
    }

    public Vector3 GetEntranceRoomPosition()
    {
        if (_rooms == null) 
        {
            Debug.LogWarning("GetEntranceRoomPosition: _rooms is null");
            return Vector3.zero;
        }
        
        var entrance = _rooms.FirstOrDefault(room => room.Type == RoomType.Entrance);
        if (entrance == null)
        {
            Debug.LogWarning("GetEntranceRoomPosition: No entrance room found");
            Debug.LogWarning($"Available rooms: {_rooms.Count}, Types: {string.Join(", ", _rooms.Select(r => r.Type))}");
            return Vector3.zero;
        }

        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new Vector3(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }
    #endregion
}