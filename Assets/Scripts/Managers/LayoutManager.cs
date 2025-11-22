// LayoutManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

/// <summary>
/// Main dungeon generator that orchestrates the entire generation pipeline.
/// Handles layout generation, room assignment, and rendering coordination.
/// </summary>
public class LayoutManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private LevelConfig _levelConfig;
    [SerializeField] private PartitionConfig _partitionConfig;
    [SerializeField] private RoomConfig _roomConfig;
    
    [Header("References")]
    public DungeonRenderer Renderer;
    
    // Algorithm components
    private PartitionGenerator _partitionGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomGenerator _roomGenerator;
    private RoomAssigner _roomAssigner;
    private GeometryBuilder _geometryBuilder;
    
    // Runtime state
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    private ConfigService _configService;
    
    // Runtime config accessors
    private GameConfig RuntimeGameConfig => _configService?.GameConfig ?? _gameConfig;
    private LevelConfig RuntimeLevelConfig => _configService?.LevelConfig ?? _levelConfig;
    private PartitionConfig RuntimePartitionConfig => _configService?.PartitionConfig ?? _partitionConfig;
    private RoomConfig RuntimeRoomConfig => _configService?.RoomConfig ?? _roomConfig;
    
    public List<RoomModel> CurrentRooms => _rooms;
    public LevelModel CurrentLayout => _layout;
    
    void Awake()
    {
        InitializeComponents();
        InitializeConfigService();
        InitializeRandom();
    }

    void Start() => GenerateDungeon();

    private void InitializeConfigService()
    {
        _configService = new ConfigService(_gameConfig, _levelConfig, _partitionConfig, _roomConfig);
        Debug.Log($"Runtime configs initialized - Starting floor: {RuntimeLevelConfig.FloorLevel}");
    }

    private void InitializeComponents()
    {
        int seed = RuntimeLevelConfig.Seed;

        _partitionGenerator = new PartitionGenerator(seed);
        _corridorGenerator = new CorridorGenerator(seed);
        _roomGenerator = new(seed);
        _roomAssigner = new RoomAssigner();
        _geometryBuilder = new GeometryBuilder();
    }

    private void EnsureComponentsInitialized()
    {
        if (_partitionGenerator == null)
            InitializeComponents();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        EnsureComponentsInitialized();
        InitializeRandom();
        
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
        _rooms = _roomAssigner.AssignRooms(_layout, RuntimeLevelConfig.FloorLevel, _random);
        if (_rooms == null || _rooms.Count == 0) return;

        // Phase 3: Build geometry
        _geometryBuilder.BuildFinalGeometry(_layout);
        _layout.InitializeSpatialData();
        
        // Phase 4: Render
        RenderDungeon();
        
        // Phase 5: Notify systems
        NotifyDungeonReady();
    }

    private void RenderDungeon()
    {
        if (Renderer != null)
        {
            Renderer.RenderDungeon(_layout, _rooms, _levelConfig.FloorLevel, _levelConfig.Seed);
        }
        else
        {
            Debug.LogWarning("No renderer assigned!");
        }
    }

    private void NotifyDungeonReady()
    {
        GetComponent<PlayerSpawner>()?.OnDungeonGenerated();
    }

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        RuntimeLevelConfig.FloorLevel++;
        _random = new System.Random(RuntimeLevelConfig.Seed);

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

    #region Helper Methods
    private void InitializeRandom() => _random ??= new System.Random(RuntimeLevelConfig.Seed);

    private void ValidateConfigs()
    {
        RuntimeLevelConfig.Width = Mathf.Clamp(RuntimeLevelConfig.Width, 10, 1000);
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

    /// <summary>
    /// Gets the world position of the entrance room for player spawning.
    /// </summary>
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
            LogAvailableRooms();
            return Vector3.zero;
        }

        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }

    private void LogAvailableRooms()
    {
        Debug.LogWarning($"Available rooms: {_rooms.Count}, Types: {string.Join(", ", _rooms.Select(r => r.Type))}");
    }
    #endregion
}