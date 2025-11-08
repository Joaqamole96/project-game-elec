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
    
    public List<RoomModel> CurrentRooms => _rooms;
    public LevelModel CurrentLayout => _layout;
    
    void Awake()
    {
        InitializeComponents();
        InitializeRandom();
    }

    void Start() => GenerateDungeon();

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
        
        // Phase 1: Generate layout
        Debug.Log("Phase 1: Generating dungeon layout...");
        _layout = GenerateDungeonLayout();
        if (_layout == null) return;

        // Phase 2: Assign room types
        Debug.Log("Phase 2: Assigning room types...");
        _rooms = _roomAssigner.AssignRooms(_layout, levelConfig.FloorLevel, _random);
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
        Debug.Log($"Generated Floor {levelConfig.FloorLevel}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms");
    }

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        levelConfig.FloorLevel++;
        _random = new System.Random(levelConfig.Seed + levelConfig.FloorLevel);

        if (levelConfig.FloorLevel > 1)
        {
            bool growWidth = RandomBool();
            if (growWidth)
                levelConfig.Width = Mathf.Min(levelConfig.Width + levelConfig.FloorGrowth, levelConfig.MaxFloorSize);
            else
                levelConfig.Height = Mathf.Min(levelConfig.Height + levelConfig.FloorGrowth, levelConfig.MaxFloorSize);
        }

        GenerateDungeon();
    }

    private LevelModel GenerateDungeonLayout()
    {
        var layout = new LevelModel();
        
        var root = _bspGenerator.GeneratePartitionTree(levelConfig, partitionConfig, _random);
        var leaves = _bspGenerator.CollectLeafPartitions(root);
        layout.Rooms = _bspGenerator.CreateRoomsFromPartitions(leaves, roomConfig, _random);
        _bspGenerator.FindAndAssignNeighbors(leaves);
        
        var allCorridors = _corridorGenerator.GenerateAllPossibleCorridors(leaves, _random);
        layout.Corridors = MinimumSpanningTree.Apply(allCorridors, layout.Rooms);
        
        return layout;
    }

    #region Helper Methods
    private void InitializeRandom()
        => _random ??= new System.Random(levelConfig.Seed);

    private bool RandomBool() 
        => _random.NextDouble() > 0.5;

    private void ValidateConfigs()
    {
        levelConfig.Width = Mathf.Clamp(levelConfig.Width, 10, 100);
        levelConfig.Height = Mathf.Clamp(levelConfig.Height, 10, 100);
        partitionConfig.MinPartitionSize = Mathf.Max(3, partitionConfig.MinPartitionSize);
        roomConfig.MinRoomSize = Mathf.Max(3, roomConfig.MinRoomSize);
        roomConfig.MaxRooms = Mathf.Max(1, roomConfig.MaxRooms);
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

        // Use the room's center, but ensure we're spawning on a walkable floor tile
        Vector2Int spawnTile = entrance.Center;
        Vector3 spawnPosition = new Vector3(spawnTile.x + 0.5f, 1f, spawnTile.y + 0.5f);
        
        Debug.Log($"Spawning at entrance room center: {spawnTile} -> {spawnPosition}");
        return spawnPosition;
    }
    #endregion
}