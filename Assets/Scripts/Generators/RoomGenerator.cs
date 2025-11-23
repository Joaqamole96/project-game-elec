// -------------------------------------------------- //
// Scripts/Generators/PartitionGenerator.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomGenerator
{
    public readonly System.Random _random;
    
    public RoomGenerator(int seed) => _random = new(seed);

    // ------------------------- //

    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, RoomConfig roomConfig)
    {
        int roomIdCounter = 0;

        List<RoomModel> rooms = new();
        
        foreach (var leaf in leaves)
        {
            if (leaf == null) continue;

            var room = CreateRoomInPartition(leaf, roomConfig, roomIdCounter);
            if (room != null)
            {
                rooms.Add(room);
                leaf.Room = room;
                roomIdCounter++;
            }
        }
        
        Debug.Log($"Created {rooms.Count} rooms from {leaves.Count} parts");
        return rooms;
    }

    private RoomModel CreateRoomInPartition(PartitionModel leaf, RoomConfig roomConfig, int roomId)
    {
        // Calculate maximum possible insets
        int maxHorizontalInset = (leaf.Bounds.width - roomConfig.MinRoomSize) / 2;
        int maxVerticalInset = (leaf.Bounds.height - roomConfig.MinRoomSize) / 2;
        
        // Clamp insets to safe values
        int leftInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
        int rightInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
        int bottomInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);
        int topInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);

        // Ensure we have at least a minimum room size
        int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
        int roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        
        // Force minimum room size and ensure we have at least 5x5 for floors
        if (roomWidth < 5 || roomHeight < 5)
        {
            Debug.LogWarning($"Partition too small for room: {leaf.Bounds}. Adjusting insets...");
            
            // Use minimal insets to create smallest possible room
            leftInset = 1;
            rightInset = 1;
            bottomInset = 1;
            topInset = 1;
            
            roomWidth = Mathf.Max(5, leaf.Bounds.width - 2);
            roomHeight = Mathf.Max(5, leaf.Bounds.height - 2);
        }

        // Ensure room meets minimum size requirements
        if (roomWidth < roomConfig.MinRoomSize || roomHeight < roomConfig.MinRoomSize)
        {
            // Adjust insets to meet minimum size
            int neededWidth = roomConfig.MinRoomSize - roomWidth;
            int neededHeight = roomConfig.MinRoomSize - roomHeight;
            
            leftInset = Mathf.Max(1, leftInset - neededWidth / 2);
            rightInset = Mathf.Max(1, rightInset - neededWidth / 2);
            bottomInset = Mathf.Max(1, bottomInset - neededHeight / 2);
            topInset = Mathf.Max(1, topInset - neededHeight / 2);
            
            roomWidth = leaf.Bounds.width - (leftInset + rightInset);
            roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        }

        RectInt roomBounds = new(
            leaf.Bounds.x + leftInset,
            leaf.Bounds.y + bottomInset,
            roomWidth,
            roomHeight
        );

        if (roomBounds.width >= 5 && roomBounds.height >= 5)
        {
            var room = new RoomModel(roomBounds, roomId, RoomType.Combat);
            Debug.Log($"Created room {room.ID}: {roomBounds} (Size: {roomBounds.width}x{roomBounds.height})");
            return room;
        }
        else
        {
            Debug.LogWarning($"Skipped room creation - bounds too small: {roomBounds}");
            return null;
        }
    }

    public void FindAndAssignNeighbors(List<PartitionModel> parts)
    {
        if (parts == null) return;

        foreach (var part in parts)
            part.Neighbors.Clear();

        Dictionary<int, List<PartitionModel>> rightEdgeMap = new(), bottomEdgeMap = new();

        // Build edge maps for efficient neighbor finding
        foreach (var part in parts)
        {
            if (part == null) continue;

            // Right edge mapping
            if (!rightEdgeMap.ContainsKey(part.Bounds.xMax)) rightEdgeMap[part.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[part.Bounds.xMax].Add(part);
            
            // Bottom edge mapping
            if (!bottomEdgeMap.ContainsKey(part.Bounds.yMax)) bottomEdgeMap[part.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[part.Bounds.yMax].Add(part);
        }

        // Find horizontal neighbors
        foreach (var part in parts)
        {
            if (part == null) continue;

            if (rightEdgeMap.TryGetValue(part.Bounds.xMin, out var horizontalCandidates)) FindValidNeighbors(part, horizontalCandidates);

            if (bottomEdgeMap.TryGetValue(part.Bounds.yMin, out var verticalCandidates)) FindValidNeighbors(part, verticalCandidates);
        }
    }

    private void FindValidNeighbors(PartitionModel part, List<PartitionModel> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate == part) continue;

            if (ArePartitionsNeighbors(part.Bounds, candidate.Bounds))
            {
                if (!part.Neighbors.Contains(candidate))
                    part.Neighbors.Add(candidate);
                if (!candidate.Neighbors.Contains(part))
                    candidate.Neighbors.Add(part);
            }
        }
    }

    private bool ArePartitionsNeighbors(RectInt boundsA, RectInt boundsB)
    {
        bool touchHorz = boundsA.xMax == boundsB.xMin || boundsB.xMax == boundsA.xMin;
        bool touchVert = boundsA.yMax == boundsB.yMin || boundsB.yMax == boundsA.yMin;

        bool overlapX = boundsA.xMin < boundsB.xMax && boundsB.xMin < boundsA.xMax;
        bool overlapY = boundsA.yMin < boundsB.yMax && boundsB.yMin < boundsA.yMax;

        return (touchHorz && overlapY) || (touchVert && overlapX);
    }

    // ------------------------- //

    public List<RoomModel> AssignRooms(LevelModel level, int levelNumber)
    {
        if (level?.Rooms == null || level.Rooms.Count == 0) return CreateFallbackAssignments(level?.Rooms);

        level.InitializeSpatialData();
        var roomGraph = level.RoomGraph;
        
        if (roomGraph.Count == 0) return CreateFallbackAssignments(level.Rooms);

        var entranceRoom = FindOptimalEntranceRoom(level.Rooms, roomGraph, level);
        var distances = CalculateDistancesFromRoom(roomGraph, entranceRoom);
        
        AssignDistanceValues(level.Rooms, distances);
        AssignCriticalRooms(level.Rooms, distances, levelNumber);
        AssignSpecialRooms(level.Rooms);
        AssignEmptyRooms(level.Rooms);
        
        GenerateSpawnPositions(level.Rooms);
        
        Debug.Log($"Room assignment complete: {GetRoomTypeSummary(level.Rooms)}");
        return level.Rooms;
    }

    private RoomModel FindOptimalEntranceRoom(List<RoomModel> rooms, Dictionary<RoomModel, List<RoomModel>> graph, LevelModel level)
    {
        return rooms
            .OrderBy(room => 
            {
                int connectionCount = graph.ContainsKey(room) ? graph[room].Count : int.MaxValue;
                int edgeDistance = CalculateEdgeDistance(room, level);
                return connectionCount * 1000 + edgeDistance; // Prioritize well-connected edge rooms
            })
            .FirstOrDefault() 
            ?? rooms[0];
    }

    private int CalculateEdgeDistance(RoomModel room, LevelModel level)
    {
        if (room?.Bounds == null || level?.OverallBounds == null) return int.MaxValue;
        
        return Mathf.Min(
            room.Bounds.xMin, 
            level.OverallBounds.size.x - room.Bounds.xMax,
            room.Bounds.yMin, 
            level.OverallBounds.size.y - room.Bounds.yMax
        );
    }

    private void AssignDistanceValues(List<RoomModel> rooms, Dictionary<RoomModel, int> distances)
    {
        if (rooms == null || distances == null) return;

        foreach (var room in rooms)
            if (room != null && distances.TryGetValue(room, out int distance))
                room.DistanceFromEntrance = distance;
    }

    private void AssignCriticalRooms(List<RoomModel> rooms, Dictionary<RoomModel, int> distances, int levelNumber)
    {
        if (rooms == null || rooms.Count < 2) throw new("Not enough rooms to assign critical rooms!");

        var entranceRoom = rooms
            .OrderBy(r => distances.GetValueOrDefault(r, int.MaxValue))
            .First();
        var exitRoom = rooms
            .OrderByDescending(r => distances.GetValueOrDefault(r, int.MinValue))
            .First();

        // Ensure we have distinct rooms for entrance and exit
        if (entranceRoom == exitRoom && rooms.Count > 1)
            exitRoom = rooms
                .Where(r => r != entranceRoom)
                .OrderByDescending(r => distances.GetValueOrDefault(r, int.MinValue))
                .First();

        // Only assign if they're currently combat rooms (don't reassign already assigned rooms)
        if (entranceRoom.Type == RoomType.Combat)
        {
            entranceRoom.Type = RoomType.Entrance;
            entranceRoom.State = RoomAccess.Open;
            entranceRoom.IsRevealed = true;
            Debug.Log($"Assigned Entrance: Room {entranceRoom.ID}");
        }

        if (exitRoom.Type == RoomType.Combat && exitRoom != entranceRoom)
        {
            exitRoom.Type = RoomType.Exit;
            exitRoom.State = RoomAccess.Open;
            exitRoom.IsRevealed = true;
            Debug.Log($"Assigned Exit: Room {exitRoom.ID}");
        }

        if (levelNumber % 5 == 0) AssignBossRoom(rooms, exitRoom);
        
        // Log the final room distribution
        Debug.Log($"Room assignment - Entrance: {entranceRoom.ID}, Exit: {exitRoom?.ID}, Total rooms: {rooms.Count}");
    }

    private void AssignBossRoom(List<RoomModel> rooms, RoomModel exitRoom)
    {
        if (exitRoom == null) return;

        var bossCandidate = rooms
            .Where(r => r != null && r.Type == RoomType.Combat && r.ConnectedRooms.Contains(exitRoom))
            .OrderByDescending(r => r.DistanceFromEntrance)
            .FirstOrDefault();

        bossCandidate ??= rooms
            .Where(r => r != null && r.Type == RoomType.Combat)
            .OrderByDescending(r => r.DistanceFromEntrance)
            .FirstOrDefault();

        if (bossCandidate != null)
        {
            bossCandidate.Type = RoomType.Boss;
            bossCandidate.State = RoomAccess.Closed;
        }
    }

    private void AssignSpecialRooms(List<RoomModel> rooms)
    {
        var combatRooms = rooms.Where(r => r != null && r.Type == RoomType.Combat).ToList();

        if (combatRooms.Count < 2) return;

        int targetShopRooms = 1;
        int targetTreasureRooms = 1;

        var midProgressRooms = combatRooms
            .OrderBy(r => Mathf.Abs(r.DistanceFromEntrance - (float)combatRooms.Average(room => room.DistanceFromEntrance)))
            .ToList();

        // Assign shop rooms
        for (int i = 0; i < targetShopRooms && i < midProgressRooms.Count; i++)
        {
            midProgressRooms[i].Type = RoomType.Shop;
            midProgressRooms[i].State = RoomAccess.Open;
        }

        // Assign treasure rooms
        int treasureStart = targetShopRooms;
        for (int i = 0; i < targetTreasureRooms && treasureStart + i < midProgressRooms.Count; i++)
        {
            midProgressRooms[treasureStart + i].Type = RoomType.Treasure;
            midProgressRooms[treasureStart + i].State = RoomAccess.Open;
        }
    }

    private void AssignEmptyRooms(List<RoomModel> rooms)
    {
        var combatRooms = rooms
            .Where(r => r != null && r.Type == RoomType.Combat)
            .ToList();
        if (combatRooms.Count == 0) return;

        int emptyRoomCount = Mathf.Max(1, combatRooms.Count / 4);
        var roomsToMakeEmpty = combatRooms
            .OrderBy(r => _random.NextDouble())
            .Take(emptyRoomCount)
            .ToList();

        foreach (var room in roomsToMakeEmpty)
        {
            room.Type = RoomType.Empty;
            room.State = RoomAccess.Open;
        }
    }

    private void GenerateSpawnPositions(List<RoomModel> rooms)
    {
        int enemiesPerRoom = 3;
        
        foreach (var room in rooms)
            if (room != null && (room.Type == RoomType.Combat || room.Type == RoomType.Boss))
                room.GenerateSpawnPositions(enemiesPerRoom);
    }

    private Dictionary<RoomModel, int> CalculateDistancesFromRoom(Dictionary<RoomModel, List<RoomModel>> graph, RoomModel startRoom)
    {
        var distances = new Dictionary<RoomModel, int>();
        
        if (graph == null || !graph.ContainsKey(startRoom)) 
            return distances;

        var visited = new HashSet<RoomModel>();
        var queue = new Queue<(RoomModel, int)>();
        
        queue.Enqueue((startRoom, 0));
        visited.Add(startRoom);
        
        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            distances[current] = distance;
            
            if (graph.ContainsKey(current))
                foreach (var neighbor in graph[current])
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
        }
        
        return distances;
    }

    private List<RoomModel> CreateFallbackAssignments(List<RoomModel> rooms)
    {
        if (rooms == null || rooms.Count == 0) return new();

        for (int i = 0; i < rooms.Count; i++)
            if (rooms[i] != null)
            {
                rooms[i].Type = ((i == 0) ? (RoomType.Entrance) : ((i == rooms.Count - 1) ? (RoomType.Exit) : (RoomType.Combat)));
            }

        return rooms;
    }

    private string GetRoomTypeSummary(List<RoomModel> rooms)
    {
        if (rooms == null) return "No rooms";

        return string.Join(", ", rooms
            .Where(r => r != null)
            .GroupBy(r => r.Type)
            .Select(g => $"{g.Key}: {g.Count()}"));
    }
}