using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(DungeonRenderer))]
public class DungeonManager : MonoBehaviour
{
    [Header("Configuration Files")]
    public GameConfig GameConfig;
    public LevelConfig LevelConfig;
    public PartitionConfig PartitionConfig;
    
    [Header("Dependencies")]
    public DungeonRenderer DungeonRenderer;
    
    // Generator components
    private PartitionGenerator _partitionGenerator;
    private RoomGenerator _roomGenerator;
    private CorridorGenerator _corridorGenerator;
    private LayoutGenerator _layoutGenerator;
    private BiomeManager _biomeManager;
    private RoomTypeService _roomTypeService;
    
    // Runtime state
    private LevelModel _layout;
    private List<RoomModel> _rooms;
    private System.Random _random;
    private ConfigRegistry _configRegistry;
    
    // Runtime config accessors
    private GameConfig RuntimeGameConfig => _configRegistry?.GameConfig ?? GameConfig;
    private LevelConfig RuntimeLevelConfig => _configRegistry?.LevelConfig ?? LevelConfig;
    private PartitionConfig RuntimePartitionConfig => _configRegistry?.PartitionConfig ?? PartitionConfig;
    
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
        _configRegistry = new ConfigRegistry(GameConfig, LevelConfig, PartitionConfig);
        Debug.Log($"DungeonManager.InitializeConfigRegistry(): Runtime configs initialized - Starting floor: {RuntimeLevelConfig.LevelNumber}");
    }

    private void InitializeComponents()
    {
        _partitionGenerator = new PartitionGenerator();
        _roomGenerator = new RoomGenerator();
        _corridorGenerator = new CorridorGenerator();
        _layoutGenerator = new LayoutGenerator();
        _roomTypeService = new RoomTypeService();
        _biomeManager = new BiomeManager(LevelConfig);
        
        DungeonRenderer = GetComponent<DungeonRenderer>();
        DungeonRenderer.Initialize(_biomeManager);
        
        Debug.Log("DungeonManager: All components initialized");
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
        
        Debug.Log($"Generating floor {RuntimeLevelConfig.LevelNumber}");
        
        ExecuteGenerationPipeline();
        
        stopwatch.Stop();
        Debug.Log($"Generated Floor {RuntimeLevelConfig.LevelNumber}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms");
    }

    private void ExecuteGenerationPipeline()
    {
        // Get biome for this floor
        var biome = _biomeManager.GetBiomeForFloor(RuntimeLevelConfig.LevelNumber);
        
        // Phase 1: Generate layout
        _layout = GenerateDungeonLayout();
        if (_layout == null)
        {
            Debug.LogError("Dungeon generation failed: Layout is null");
            return;
        }

        // Phase 2: Render dungeon
        RenderDungeon(biome);
        
        // Phase 3: Notify systems
        NotifyDungeonReady();
    }

    private void RenderDungeon(BiomeModel biome)
    {
        if (DungeonRenderer != null)
        {
            DungeonRenderer.RenderDungeon(_layout, _layout.Rooms, biome);
        }
        else
        {
            Debug.LogError("DungeonRenderer is not assigned!");
        }
    }

    private void NotifyDungeonReady()
    {
        GetComponent<PlayerSpawner>()?.OnDungeonGenerated();
        
        // Notify RoomManager
        var roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.SetCurrentLevel(_layout);
        }
    }

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        RuntimeLevelConfig.LevelNumber++;
        _random = new System.Random(RuntimeLevelConfig.Seed + RuntimeLevelConfig.LevelNumber);

        Debug.Log($"Moving to floor {RuntimeLevelConfig.LevelNumber}");
        GenerateDungeon();
    }

    private LevelModel GenerateDungeonLayout()
    {
        // CORRECTED: Calculate dimensions from LevelConfig methods (respects your architecture)
        int width = RuntimeLevelConfig.GetFloorWidth();
        int height = RuntimeLevelConfig.GetFloorHeight();
        
        var layout = new LevelModel(width, height);
        
        Debug.Log($"Floor {RuntimeLevelConfig.LevelNumber} dimensions: {width}×{height}");
        
        // Generate partitions
        var root = _partitionGenerator.GeneratePartitionTree(layout, RuntimePartitionConfig, _random);
        var leaves = _partitionGenerator.CollectLeaves(root);
        
        // Create rooms from partitions
        var rooms = _roomGenerator.CreateRoomsFromPartitions(leaves, _random);
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("Failed to create rooms from partitions");
            return null;
        }
        
        // Find neighbors for corridor generation
        _roomGenerator.FindAndAssignNeighbors(leaves);
        
        // Generate corridors
        var allCorridors = _corridorGenerator.GenerateTotalCorridors(leaves, _random);
        layout.Corridors = MinimumSpanningTree.Apply(allCorridors, rooms);
        
        // Build final geometry
        _layoutGenerator.BuildFinalGeometry(layout);
        
        // Assign room types
        layout.Rooms = rooms;
        _roomTypeService.AssignRooms(layout, RuntimeLevelConfig.LevelNumber, _random);
        
        Debug.Log($"Dungeon layout generated: {rooms.Count} rooms, {layout.Corridors.Count} corridors");
        return layout;
    }

    #region Helper Methods
    private void InitializeRandom() => _random ??= new System.Random(RuntimeLevelConfig.Seed);

    private void ValidateConfigs()
    {
        RuntimeLevelConfig.Validate();
        RuntimePartitionConfig.Validate();
        RuntimeGameConfig.Validate();
    }

    private void ClearPreviousGeneration()
    {
        _layout = null;
        _rooms = null;
        DungeonRenderer?.ClearRendering();
    }

    private string GetRoomTypeBreakdown()
    {
        if (_layout?.Rooms == null) return "No rooms";
        return string.Join(", ", _layout.Rooms.GroupBy(r => r.Type).Select(g => $"{g.Key}: {g.Count()}"));
    }

    /// <summary>
    /// Gets the world position of the entrance room for player spawning.
    /// </summary>
    public Vector3 GetEntranceRoomPosition()
    {
        if (_layout?.Rooms == null) 
        {
            Debug.LogWarning("GetEntranceRoomPosition: Rooms is null");
            return Vector3.zero;
        }
        
        var entrance = _layout.Rooms.FirstOrDefault(room => room.Type == RoomType.Entrance);
        if (entrance == null)
        {
            Debug.LogWarning("GetEntranceRoomPosition: No entrance room found");
            LogAvailableRooms();
            return Vector3.zero;
        }

        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new Vector3(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }

    private void LogAvailableRooms()
    {
        if (_layout?.Rooms == null) 
        {
            Debug.LogWarning("No rooms available");
            return;
        }
        Debug.LogWarning($"Available rooms: {_layout.Rooms.Count}, Types: {string.Join(", ", _layout.Rooms.Select(r => r.Type))}");
    }
    #endregion
}