// DungeonGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

/// <summary>
/// Main dungeon generator that orchestrates the entire generation pipeline.
/// Handles layout generation, room assignment, and rendering coordination.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private LevelConfig _levelConfig;
    [SerializeField] private PartitionConfig _partitionConfig;
    [SerializeField] private RoomConfig _roomConfig;
    
    [Header("References")]
    public DungeonRenderer Renderer;
    
    // Algorithm components
    private BSPGenerator _bspGenerator;
    private CorridorGenerator _corridorGenerator;
    private RoomAssigner _roomAssigner;
    private GeometryGenerator _geometryGenerator;
    
    // Runtime state
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    private ConfigRegistry _configRegistry;
    
    // Runtime config accessors
    private GameConfig RuntimeGameConfig => _configRegistry?.GameConfig ?? _gameConfig;
    private LevelConfig RuntimeLevelConfig => _configRegistry?.LevelConfig ?? _levelConfig;
    private PartitionConfig RuntimePartitionConfig => _configRegistry?.PartitionConfig ?? _partitionConfig;
    private RoomConfig RuntimeRoomConfig => _configRegistry?.RoomConfig ?? _roomConfig;
    
    public List<RoomModel> CurrentRooms => _rooms;
    public LevelModel CurrentLayout => _layout;
    
    void Awake()
    {
        InitializeComponents();
        InitializeConfigRegistry();
        InitializeRandom();
    }

    void Start() => GenerateDungeon();

    private void InitializeConfigRegistry()
    {
        _configRegistry = new ConfigRegistry(_gameConfig, _levelConfig, _partitionConfig, _roomConfig);
        Debug.Log($"Runtime configs initialized - Starting floor: {RuntimeLevelConfig.LevelNumber}");
    }

    private void InitializeComponents()
    {
        _bspGenerator = new BSPGenerator();
        _corridorGenerator = new CorridorGenerator();
        _roomAssigner = new RoomAssigner();
        _geometryGenerator = new GeometryGenerator();
    }

    private void EnsureComponentsInitialized()
    {
        if (_bspGenerator == null)
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
        
        Debug.Log($"Generating floor {RuntimeLevelConfig.LevelNumber} with size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");
        
        ExecuteGenerationPipeline();
        
        stopwatch.Stop();
        Debug.Log($"Generated Floor {RuntimeLevelConfig.LevelNumber}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms");
    }

    private void ExecuteGenerationPipeline()
    {
        // Phase 1: Generate layout
        _layout = GenerateDungeonLayout();
        if (_layout == null) return;

        // Phase 2: Assign room types
        _rooms = _roomAssigner.AssignRooms(_layout, RuntimeLevelConfig.LevelNumber, _random);
        if (_rooms == null || _rooms.Count == 0) return;

        // Phase 3: Build geometry
        _geometryGenerator.BuildFinalGeometry(_layout);
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
            Renderer.RenderDungeon(_layout, _rooms, _levelConfig.LevelNumber, _levelConfig.Seed);
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
        RuntimeLevelConfig.LevelNumber++;
        _random = new System.Random(RuntimeLevelConfig.Seed + RuntimeLevelConfig.LevelNumber);

        if (RuntimeLevelConfig.LevelNumber > 1)
        {
            GrowFloorSize();
        }

        Debug.Log($"Moving to floor {RuntimeLevelConfig.LevelNumber} - New size: {RuntimeLevelConfig.Width}x{RuntimeLevelConfig.Height}");
        GenerateDungeon();
    }

    private void GrowFloorSize()
    {
        bool growWidth = _random.NextDouble() > 0.5;
        if (growWidth)
        {
            RuntimeLevelConfig.Width = Mathf.Min(RuntimeLevelConfig.Width + RuntimeLevelConfig.Growth, RuntimeLevelConfig.MaxSize);
        }
        else
        {
            RuntimeLevelConfig.Height = Mathf.Min(RuntimeLevelConfig.Height + RuntimeLevelConfig.Growth, RuntimeLevelConfig.MaxSize);
        }
    }

    private LevelModel GenerateDungeonLayout()
    {
        var layout = new LevelModel();
        
        var root = _bspGenerator.GeneratePartitionTree(RuntimeLevelConfig, RuntimePartitionConfig, _random);
        var leaves = _bspGenerator.CollectLeafPartitions(root);
        layout.Rooms = _bspGenerator.CreateRoomsFromPartitions(leaves, RuntimeRoomConfig, _random);
        _bspGenerator.FindAndAssignNeighbors(leaves);
        
        var allCorridors = _corridorGenerator.GenerateAllPossibleCorridors(leaves, _random);
        layout.Corridors = MinimumSpanningTree.Apply(allCorridors, layout.Rooms);
        
        return layout;
    }

    #region Helper Methods
    private void InitializeRandom() => _random ??= new System.Random(RuntimeLevelConfig.Seed);

    private void ValidateConfigs()
    {
        RuntimeLevelConfig.Width = Mathf.Clamp(RuntimeLevelConfig.Width, 10, 1000);
        RuntimeLevelConfig.Height = Mathf.Clamp(RuntimeLevelConfig.Height, 10, 1000);
        RuntimePartitionConfig.MinSize = Mathf.Max(3, RuntimePartitionConfig.MinSize);
        RuntimeRoomConfig.MinRoomSize = Mathf.Max(3, RuntimeRoomConfig.MinRoomSize);
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