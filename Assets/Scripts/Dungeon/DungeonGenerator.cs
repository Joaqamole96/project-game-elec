using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public DungeonConfig Config;

    [Header("References")]
    public DungeonRenderer Renderer;

    // Current dungeon state
    private DungeonLayout _layout;
    private List<RoomAssignment> _roomAssignments;

    void Start() => GenerateDungeon();

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        var stopwatch = Stopwatch.StartNew();

        ClearPreviousGeneration();

        // Phase 1: Generate the conceptual layout
        _layout = GenerateDungeonLayout();

        // Phase 2: Assign room types and progression
        var roomAssigner = new RoomAssigner();
        _roomAssignments = roomAssigner.AssignRooms(_layout, Config.FloorLevel);

        // Phase 3: Build final geometry based on finalized layout
        BuildFinalGeometry();

        // Phase 4: Render everything
        if (Renderer != null)
        {
            Renderer.RenderDungeon(_layout, _roomAssignments);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Generated Floor {Config.FloorLevel}: {GetRoomTypeBreakdown()} in {stopwatch.ElapsedMilliseconds}ms");
    }

    [ContextMenu("Next Floor")]
    public void GenerateNextFloor()
    {
        Config.FloorLevel++;

        // Increase dungeon size with floor growth
        if (Config.FloorLevel > 1)
        {
            bool growWidth = Random.value > 0.5f;
            if (growWidth)
            {
                Config.Width = Mathf.Min(Config.Width + Config.FloorGrowth, Config.MaxFloorSize);
            }
            else
            {
                Config.Height = Mathf.Min(Config.Height + Config.FloorGrowth, Config.MaxFloorSize);
            }
        }

        GenerateDungeon();
    }

    private DungeonLayout GenerateDungeonLayout()
    {
        var layout = new DungeonLayout();

        // Step 1: Generate BSP partitions and rooms
        Random.InitState(Config.Seed);
        var rootPartition = GeneratePartitionTree();
        var leafPartitions = CollectLeafPartitions(rootPartition);
        layout.Rooms = CreateRoomsFromPartitions(leafPartitions);

        // Step 2: Build spatial relationships
        FindAndAssignNeighbors(leafPartitions);

        // Step 3: Generate all possible corridors
        var allCorridors = GenerateAllPossibleCorridors(leafPartitions);

        // Step 4: Apply MST to get minimal connected set
        layout.Corridors = ApplyMinimumSpanningTree(allCorridors, layout.Rooms);

        return layout;
    }

    private void BuildFinalGeometry()
    {
        _layout.AllFloorTiles.Clear();
        _layout.AllWallTiles.Clear();
        _layout.AllDoorTiles.Clear();
        _layout.WallTypes.Clear();

        // Add all room floors
        foreach (var room in _layout.Rooms)
        {
            foreach (var floorPos in room.GetFloorTiles())
            {
                _layout.AllFloorTiles.Add(floorPos);
            }
        }

        // Add all corridor floors
        foreach (var corridor in _layout.Corridors)
        {
            foreach (var tile in corridor.Tiles)
            {
                _layout.AllFloorTiles.Add(tile);
            }
        }

        // Build initial wall perimeter for all rooms
        var roomWallPerimeters = new Dictionary<RoomModel, HashSet<Vector2Int>>();
        foreach (var room in _layout.Rooms)
        {
            var wallPerimeter = new HashSet<Vector2Int>();
            foreach (var wallPos in room.GetWallPerimeter())
            {
                wallPerimeter.Add(wallPos);
            }
            roomWallPerimeters[room] = wallPerimeter;
        }

        // Remove walls where we have final doors (from surviving corridors)
        foreach (var corridor in _layout.Corridors)
        {
            if (roomWallPerimeters.ContainsKey(corridor.StartRoom))
                roomWallPerimeters[corridor.StartRoom].Remove(corridor.StartDoor);

            if (roomWallPerimeters.ContainsKey(corridor.EndRoom))
                roomWallPerimeters[corridor.EndRoom].Remove(corridor.EndDoor);

            _layout.AllDoorTiles.Add(corridor.StartDoor);
            _layout.AllDoorTiles.Add(corridor.EndDoor);
        }

        // Combine all walls into final wall set
        foreach (var roomWalls in roomWallPerimeters.Values)
        {
            foreach (var wallPos in roomWalls)
            {
                _layout.AllWallTiles.Add(wallPos);
            }
        }

        // Determine wall types
        foreach (var wallPos in _layout.AllWallTiles)
        {
            _layout.WallTypes[wallPos] = DetermineWallType(wallPos, _layout.Rooms);
        }
    }

    // [Include all the algorithm methods from the previous monofile here]
    // GeneratePartitionTree, SplitPartition, CollectLeafPartitions, CreateRoomsFromPartitions,
    // FindAndAssignNeighbors, ArePartitionsNeighbors, GenerateAllPossibleCorridors, 
    // CreateCorridorBetweenRooms, FindAlignedDoorPositions, FindAnyDoorPosition, 
    // CreateStraightCorridor, ApplyMinimumSpanningTree, FindRoot, UnionSets, DetermineWallType

    #region Dungeon Generation Algorithms
    private PartitionModel GeneratePartitionTree()
    {
        var root = new PartitionModel(new RectInt(0, 0, Config.Width, Config.Height));
        SplitPartition(root);
        return root;
    }

    private void SplitPartition(PartitionModel partition)
    {
        if (partition.Bounds.width <= Config.MinimumPartitionSize ||
            partition.Bounds.height <= Config.MinimumPartitionSize)
            return;

        bool splitVertically = partition.Bounds.width > partition.Bounds.height;
        float splitRatio = Random.Range(0.4f, 0.6f);

        if (splitVertically)
        {
            int splitPoint = Mathf.RoundToInt(partition.Bounds.width * splitRatio);
            partition.LeftChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y, splitPoint, partition.Bounds.height));
            partition.RightChild = new PartitionModel(new RectInt(
                partition.Bounds.x + splitPoint, partition.Bounds.y,
                partition.Bounds.width - splitPoint, partition.Bounds.height));
        }
        else
        {
            int splitPoint = Mathf.RoundToInt(partition.Bounds.height * splitRatio);
            partition.LeftChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y, partition.Bounds.width, splitPoint));
            partition.RightChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y + splitPoint,
                partition.Bounds.width, partition.Bounds.height - splitPoint));
        }

        SplitPartition(partition.LeftChild);
        SplitPartition(partition.RightChild);
    }

    private List<PartitionModel> CollectLeafPartitions(PartitionModel root)
    {
        var leaves = new List<PartitionModel>();
        CollectLeavesRecursive(root, leaves);
        return leaves;
    }

    private void CollectLeavesRecursive(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) return;

        if (partition.LeftChild == null && partition.RightChild == null)
            leaves.Add(partition);
        else
        {
            CollectLeavesRecursive(partition.LeftChild, leaves);
            CollectLeavesRecursive(partition.RightChild, leaves);
        }
    }

    private List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves)
    {
        var rooms = new List<RoomModel>();
        int roomIdCounter = 0;

        foreach (var leaf in leaves)
        {
            int leftInset = Random.Range(Config.MinimumInset, Config.MaximumInset + 1);
            int rightInset = Random.Range(Config.MinimumInset, Config.MaximumInset + 1);
            int bottomInset = Random.Range(Config.MinimumInset, Config.MaximumInset + 1);
            int topInset = Random.Range(Config.MinimumInset, Config.MaximumInset + 1);

            int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
            int roomHeight = leaf.Bounds.height - (bottomInset + topInset);

            if (roomWidth < 3 || roomHeight < 3)
            {
                leftInset = Mathf.Min(leftInset, (leaf.Bounds.width - 3) / 2);
                rightInset = Mathf.Min(rightInset, (leaf.Bounds.width - 3) / 2);
                bottomInset = Mathf.Min(bottomInset, (leaf.Bounds.height - 3) / 2);
                topInset = Mathf.Min(topInset, (leaf.Bounds.height - 3) / 2);

                roomWidth = Mathf.Max(3, leaf.Bounds.width - (leftInset + rightInset));
                roomHeight = Mathf.Max(3, leaf.Bounds.height - (bottomInset + topInset));
            }

            RectInt roomBounds = new RectInt(
                leaf.Bounds.x + leftInset,
                leaf.Bounds.y + bottomInset,
                roomWidth,
                roomHeight
            );

            var room = new RoomModel(roomBounds, roomIdCounter++);
            leaf.Room = room;
            rooms.Add(room);
        }

        return rooms;
    }

    private void FindAndAssignNeighbors(List<PartitionModel> partitions)
    {
        foreach (var partition in partitions)
            partition.Neighbors.Clear();

        var rightEdgeMap = new Dictionary<int, List<PartitionModel>>();
        var bottomEdgeMap = new Dictionary<int, List<PartitionModel>>();

        foreach (var partition in partitions)
        {
            if (!rightEdgeMap.ContainsKey(partition.Bounds.xMax))
                rightEdgeMap[partition.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[partition.Bounds.xMax].Add(partition);

            if (!bottomEdgeMap.ContainsKey(partition.Bounds.yMax))
                bottomEdgeMap[partition.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[partition.Bounds.yMax].Add(partition);
        }

        foreach (var partition in partitions)
        {
            if (rightEdgeMap.TryGetValue(partition.Bounds.xMin, out var horizontalCandidates))
            {
                foreach (var candidate in horizontalCandidates)
                {
                    if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
                    {
                        partition.Neighbors.Add(candidate);
                        candidate.Neighbors.Add(partition);
                    }
                }
            }

            if (bottomEdgeMap.TryGetValue(partition.Bounds.yMin, out var verticalCandidates))
            {
                foreach (var candidate in verticalCandidates)
                {
                    if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
                    {
                        partition.Neighbors.Add(candidate);
                        candidate.Neighbors.Add(partition);
                    }
                }
            }
        }
    }

    private bool ArePartitionsNeighbors(RectInt boundsA, RectInt boundsB)
    {
        bool touchHorizontally = boundsA.xMax == boundsB.xMin || boundsB.xMax == boundsA.xMin;
        bool touchVertically = boundsA.yMax == boundsB.yMin || boundsB.yMax == boundsA.yMin;

        bool overlapX = boundsA.xMin < boundsB.xMax && boundsB.xMin < boundsA.xMax;
        bool overlapY = boundsA.yMin < boundsB.yMax && boundsB.yMin < boundsA.yMax;

        return (touchHorizontally && overlapY) || (touchVertically && overlapX);
    }

    private List<CorridorModel> GenerateAllPossibleCorridors(List<PartitionModel> partitions)
    {
        var allCorridors = new List<CorridorModel>();
        var roomFloorTiles = new HashSet<Vector2Int>();

        foreach (var partition in partitions)
        {
            if (partition.Room != null)
            {
                foreach (var floorPos in partition.Room.GetFloorTiles())
                    roomFloorTiles.Add(floorPos);
            }
        }

        var connectedPairs = new HashSet<(int, int)>();

        foreach (var partition in partitions)
        {
            foreach (var neighbor in partition.Neighbors)
            {
                if (partition.Room == null || neighbor.Room == null) continue;

                var roomA = partition.Room;
                var roomB = neighbor.Room;

                var pairKey = (Mathf.Min(roomA.Id, roomB.Id), Mathf.Max(roomA.Id, roomB.Id));
                if (connectedPairs.Contains(pairKey)) continue;

                var corridor = CreateCorridorBetweenRooms(roomA, roomB, roomFloorTiles);
                if (corridor != null)
                {
                    allCorridors.Add(corridor);
                    connectedPairs.Add(pairKey);
                }
            }
        }

        return allCorridors;
    }

    private CorridorModel CreateCorridorBetweenRooms(RoomModel roomA, RoomModel roomB, HashSet<Vector2Int> roomFloorTiles)
    {
        var (doorA, doorB) = FindAlignedDoorPositions(roomA, roomB);

        if (doorA == null || doorB == null)
        {
            doorA = FindAnyDoorPosition(roomA, roomB);
            doorB = FindAnyDoorPosition(roomB, roomA);
        }

        if (doorA == null || doorB == null) return null;

        var corridorTiles = CreateStraightCorridor(doorA.Value, doorB.Value, roomFloorTiles);

        if (corridorTiles.Count > 0)
        {
            return new CorridorModel(corridorTiles, roomA, roomB, doorA.Value, doorB.Value);
        }

        return null;
    }

    private (Vector2Int?, Vector2Int?) FindAlignedDoorPositions(RoomModel roomA, RoomModel roomB)
    {
        var boundsA = roomA.Bounds;
        var boundsB = roomB.Bounds;

        if (boundsA.yMax <= boundsB.yMin || boundsB.yMax <= boundsA.yMin)
        {
            int overlapStart = Mathf.Max(boundsA.xMin, boundsB.xMin);
            int overlapEnd = Mathf.Min(boundsA.xMax, boundsB.xMax);

            if (overlapStart < overlapEnd - 2)
            {
                int doorX = Random.Range(overlapStart + 1, overlapEnd - 1);
                bool roomAIsAbove = boundsA.yMax <= boundsB.yMin;

                Vector2Int doorA = roomAIsAbove ?
                    new Vector2Int(doorX, boundsA.yMax - 1) :
                    new Vector2Int(doorX, boundsA.yMin);

                Vector2Int doorB = roomAIsAbove ?
                    new Vector2Int(doorX, boundsB.yMin) :
                    new Vector2Int(doorX, boundsB.yMax - 1);

                return (doorA, doorB);
            }
        }

        if (boundsA.xMax <= boundsB.xMin || boundsB.xMax <= boundsA.xMin)
        {
            int overlapStart = Mathf.Max(boundsA.yMin, boundsB.yMin);
            int overlapEnd = Mathf.Min(boundsA.yMax, boundsB.yMax);

            if (overlapStart < overlapEnd - 2)
            {
                int doorY = Random.Range(overlapStart + 1, overlapEnd - 1);
                bool roomAIsLeft = boundsA.xMax <= boundsB.xMin;

                Vector2Int doorA = roomAIsLeft ?
                    new Vector2Int(boundsA.xMax - 1, doorY) :
                    new Vector2Int(boundsA.xMin, doorY);

                Vector2Int doorB = roomAIsLeft ?
                    new Vector2Int(boundsB.xMin, doorY) :
                    new Vector2Int(boundsB.xMax - 1, doorY);

                return (doorA, doorB);
            }
        }

        return (null, null);
    }

    private Vector2Int? FindAnyDoorPosition(RoomModel sourceRoom, RoomModel targetRoom)
    {
        var sourceBounds = sourceRoom.Bounds;
        var targetCenter = targetRoom.Bounds.center;

        float distToNorth = Mathf.Abs((sourceBounds.yMax - 1) - targetCenter.y);
        float distToSouth = Mathf.Abs(sourceBounds.yMin - targetCenter.y);
        float distToEast = Mathf.Abs((sourceBounds.xMax - 1) - targetCenter.x);
        float distToWest = Mathf.Abs(sourceBounds.xMin - targetCenter.x);

        float minDist = Mathf.Min(distToNorth, distToSouth, distToEast, distToWest);

        List<Vector2Int> candidatePositions = new List<Vector2Int>();

        if (minDist == distToNorth)
        {
            for (int x = sourceBounds.xMin + 1; x < sourceBounds.xMax - 1; x++)
                candidatePositions.Add(new Vector2Int(x, sourceBounds.yMax - 1));
        }
        else if (minDist == distToSouth)
        {
            for (int x = sourceBounds.xMin + 1; x < sourceBounds.xMax - 1; x++)
                candidatePositions.Add(new Vector2Int(x, sourceBounds.yMin));
        }
        else if (minDist == distToEast)
        {
            for (int y = sourceBounds.yMin + 1; y < sourceBounds.yMax - 1; y++)
                candidatePositions.Add(new Vector2Int(sourceBounds.xMax - 1, y));
        }
        else
        {
            for (int y = sourceBounds.yMin + 1; y < sourceBounds.yMax - 1; y++)
                candidatePositions.Add(new Vector2Int(sourceBounds.xMin, y));
        }

        if (candidatePositions.Count > 0)
        {
            return candidatePositions[Random.Range(0, candidatePositions.Count)];
        }

        return null;
    }

    private List<Vector2Int> CreateStraightCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> roomFloorTiles)
    {
        var tiles = new List<Vector2Int>();

        if (start.x == end.x || start.y == end.y)
        {
            if (start.x == end.x)
            {
                int yStart = Mathf.Min(start.y, end.y);
                int yEnd = Mathf.Max(start.y, end.y);
                for (int y = yStart; y <= yEnd; y++)
                {
                    var pos = new Vector2Int(start.x, y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }
            }
            else
            {
                int xStart = Mathf.Min(start.x, end.x);
                int xEnd = Mathf.Max(start.x, end.x);
                for (int x = xStart; x <= xEnd; x++)
                {
                    var pos = new Vector2Int(x, start.y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }
            }
        }
        else
        {
            bool horizontalFirst = Random.value > 0.5f;

            if (horizontalFirst)
            {
                int dx = Mathf.Clamp(end.x - start.x, -1, 1);
                for (int x = start.x; x != end.x; x += dx)
                {
                    var pos = new Vector2Int(x, start.y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }

                int dy = Mathf.Clamp(end.y - start.y, -1, 1);
                for (int y = start.y; y != end.y; y += dy)
                {
                    var pos = new Vector2Int(end.x, y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }
            }
            else
            {
                int dy = Mathf.Clamp(end.y - start.y, -1, 1);
                for (int y = start.y; y != end.y; y += dy)
                {
                    var pos = new Vector2Int(start.x, y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }

                int dx = Mathf.Clamp(end.x - start.x, -1, 1);
                for (int x = start.x; x != end.x; x += dx)
                {
                    var pos = new Vector2Int(x, end.y);
                    if (!roomFloorTiles.Contains(pos))
                        tiles.Add(pos);
                }
            }
        }

        tiles.Add(end);
        return tiles;
    }

    private List<CorridorModel> ApplyMinimumSpanningTree(List<CorridorModel> corridors, List<RoomModel> rooms)
    {
        if (rooms == null || rooms.Count == 0 || corridors == null)
            return corridors ?? new List<CorridorModel>();

        var parentIds = new int[rooms.Count];
        for (int i = 0; i < rooms.Count; i++)
            parentIds[i] = i;

        var spanningTreeCorridors = new List<CorridorModel>();

        corridors.Sort((a, b) =>
        {
            float distA = Vector2.Distance(a.StartRoom.Bounds.center, a.EndRoom.Bounds.center);
            float distB = Vector2.Distance(b.StartRoom.Bounds.center, b.EndRoom.Bounds.center);
            return distA.CompareTo(distB);
        });

        foreach (var corridor in corridors)
        {
            if (corridor?.StartRoom == null || corridor.EndRoom == null) continue;

            int roomAIndex = rooms.IndexOf(corridor.StartRoom);
            int roomBIndex = rooms.IndexOf(corridor.EndRoom);

            if (roomAIndex < 0 || roomBIndex < 0) continue;

            if (FindRoot(roomAIndex, parentIds) != FindRoot(roomBIndex, parentIds))
            {
                spanningTreeCorridors.Add(corridor);
                UnionSets(roomAIndex, roomBIndex, parentIds);
            }
        }

        return spanningTreeCorridors;
    }

    private int FindRoot(int elementId, int[] parentIds)
    {
        if (parentIds[elementId] != elementId)
            parentIds[elementId] = FindRoot(parentIds[elementId], parentIds);
        return parentIds[elementId];
    }

    private void UnionSets(int a, int b, int[] parentIds)
    {
        int rootA = FindRoot(a, parentIds);
        int rootB = FindRoot(b, parentIds);
        if (rootA != rootB)
            parentIds[rootB] = rootA;
    }

    private WallType DetermineWallType(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms)
        {
            var bounds = room.Bounds;
            bool isNorth = pos.y == bounds.yMax - 1;
            bool isSouth = pos.y == bounds.yMin;
            bool isEast = pos.x == bounds.xMax - 1;
            bool isWest = pos.x == bounds.xMin;

            if (isNorth && isWest) return WallType.NorthWestCorner;
            if (isNorth && isEast) return WallType.NorthEastCorner;
            if (isSouth && isWest) return WallType.SouthWestCorner;
            if (isSouth && isEast) return WallType.SouthEastCorner;

            if (isNorth) return WallType.North;
            if (isSouth) return WallType.South;
            if (isEast) return WallType.East;
            if (isWest) return WallType.West;
        }

        return WallType.Interior;
    }
    #endregion

    private void ClearPreviousGeneration()
    {
        _layout = null;
        _roomAssignments = null;

        if (Renderer != null)
        {
            Renderer.ClearRendering();
        }
    }

    private string GetRoomTypeBreakdown()
    {
        if (_roomAssignments == null) return "No rooms assigned";

        var breakdown = _roomAssignments
            .GroupBy(a => a.Type)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToArray();
        return string.Join(", ", breakdown);
    }
}

public class RoomAssigner
{
    private DungeonLayout _layout;
    private int _floorLevel;
    private List<RoomAssignment> _assignments = new List<RoomAssignment>();
    
    public List<RoomAssignment> AssignRooms(DungeonLayout layout, int floorLevel)
    {
        _layout = layout;
        _floorLevel = floorLevel;
        _assignments.Clear();
        
        // Step 1: Create basic assignments for all rooms (default to combat)
        foreach (var room in layout.Rooms)
        {
            _assignments.Add(new RoomAssignment(room, RoomType.Combat));
        }
        
        // Step 2: Calculate distances for progression placement
        CalculateRoomDistances();
        
        // Step 3: Assign special rooms based on progression
        AssignEntranceAndExit();
        AssignBossRoom();
        AssignSpecialRooms();
        AssignEmptyRooms();
        
        return _assignments;
    }
    
    private void CalculateRoomDistances()
    {
        var roomGraph = BuildRoomGraph();
        
        // Use the room with most connections as center for distance calculation
        var centralRoom = roomGraph.OrderByDescending(kvp => kvp.Value.Count).First().Key;
        var distances = CalculateDistancesFromRoom(roomGraph, centralRoom);
        
        foreach (var assignment in _assignments)
        {
            assignment.DistanceFromEntrance = distances[assignment.Room];
        }
    }
    
    private void AssignEntranceAndExit()
    {
        // Find rooms with max distance for entrance and exit
        var sortedByDistance = _assignments.OrderByDescending(a => a.DistanceFromEntrance).ToList();
        
        // Entrance at one end
        var entrance = sortedByDistance[0];
        entrance.Type = RoomType.Entrance;
        entrance.State = RoomState.Open;
        
        // Exit at the other end
        var exitCandidates = sortedByDistance.Take(Mathf.Min(3, sortedByDistance.Count)).ToList();
        var exit = exitCandidates[Random.Range(0, exitCandidates.Count)];
        exit.Type = RoomType.Exit;
        exit.State = RoomState.Open;
    }
    
    private void AssignBossRoom()
    {
        if (_floorLevel % 5 == 0) // Boss every 5 floors
        {
            var exitAssignment = _assignments.First(a => a.Type == RoomType.Exit);
            var neighbors = GetNeighborRooms(exitAssignment.Room);
            
            // Find a combat room adjacent to exit for boss room
            var bossCandidate = neighbors
                .Select(neighbor => _assignments.First(a => a.Room == neighbor))
                .Where(a => a.Type == RoomType.Combat)
                .OrderByDescending(a => a.DistanceFromEntrance)
                .FirstOrDefault();
            
            if (bossCandidate != null)
            {
                bossCandidate.Type = RoomType.Boss;
                bossCandidate.State = RoomState.Closed;
            }
            else
            {
                // Fallback: use highest distance combat room
                var fallbackBoss = _assignments
                    .Where(a => a.Type == RoomType.Combat)
                    .OrderByDescending(a => a.DistanceFromEntrance)
                    .FirstOrDefault();
                if (fallbackBoss != null)
                {
                    fallbackBoss.Type = RoomType.Boss;
                    fallbackBoss.State = RoomState.Closed;
                }
            }
        }
    }
    
    private void AssignSpecialRooms()
    {
        // Assign shop or treasure to rooms in the middle of the progression
        var midProgressRooms = _assignments
            .Where(a => a.Type == RoomType.Combat)
            .OrderBy(a => Mathf.Abs((float)(a.DistanceFromEntrance - _assignments.Average(x => x.DistanceFromEntrance))))
            .Take(3)
            .ToList();
        
        if (midProgressRooms.Count > 0)
        {
            // Randomly choose between shop and treasure
            var specialRoom = midProgressRooms[Random.Range(0, midProgressRooms.Count)];
            specialRoom.Type = Random.value > 0.5f ? RoomType.Shop : RoomType.Treasure;
            specialRoom.State = RoomState.Open;
        }
    }
    
    private void AssignEmptyRooms()
    {
        // Convert some combat rooms to empty rooms (15-25% of combat rooms)
        var combatRooms = _assignments.Where(a => a.Type == RoomType.Combat).ToList();
        int emptyRoomCount = Mathf.Max(1, combatRooms.Count / 4);
        
        for (int i = 0; i < emptyRoomCount && i < combatRooms.Count; i++)
        {
            combatRooms[i].Type = RoomType.Empty;
            combatRooms[i].State = RoomState.Open;
        }
    }
    
    private Dictionary<RoomModel, List<RoomModel>> BuildRoomGraph()
    {
        var graph = new Dictionary<RoomModel, List<RoomModel>>();
        
        foreach (var room in _layout.Rooms)
        {
            graph[room] = new List<RoomModel>();
        }
        
        foreach (var corridor in _layout.Corridors)
        {
            graph[corridor.StartRoom].Add(corridor.EndRoom);
            graph[corridor.EndRoom].Add(corridor.StartRoom);
        }
        
        return graph;
    }
    
    private Dictionary<RoomModel, int> CalculateDistancesFromRoom(Dictionary<RoomModel, List<RoomModel>> graph, RoomModel startRoom)
    {
        var distances = new Dictionary<RoomModel, int>();
        var visited = new HashSet<RoomModel>();
        var queue = new Queue<(RoomModel, int)>();
        
        queue.Enqueue((startRoom, 0));
        visited.Add(startRoom);
        
        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            distances[current] = distance;
            
            foreach (var neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, distance + 1));
                }
            }
        }
        
        return distances;
    }
    
    private List<RoomModel> GetNeighborRooms(RoomModel room)
    {
        return _layout.Corridors
            .Where(c => c.StartRoom == room || c.EndRoom == room)
            .Select(c => c.StartRoom == room ? c.EndRoom : c.StartRoom)
            .ToList();
    }
}