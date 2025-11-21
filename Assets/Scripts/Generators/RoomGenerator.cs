using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomGenerator
{
    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, System.Random random)
    {
        List<RoomModel> rooms = new();
        int roomIdCounter = 0;
        
        foreach (var leaf in leaves)
        {
            if (leaf == null) continue;
            
            var room = CreateRoomInPartition(leaf, random, roomIdCounter);
            if (room != null)
            {
                rooms.Add(room);
                leaf.Room = room;
                roomIdCounter++;
            }
        }
        
        Debug.Log("RoomGenerator.CreateRoomsFromPartitions(): Created all rooms successfully.");
        return rooms;
    }

    public List<RoomModel> AssignRoomTypes(List<RoomModel> rooms, int floorLevel, System.Random random, Dictionary<RoomModel, List<RoomModel>> roomGraph = null)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No rooms to assign!");
            return CreateFallbackAssignments(rooms);
        }

        // Clear previous assignments
        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.Type = RoomType.Combat;
                room.State = RoomAccess.Closed;
                room.IsRevealed = false;
            }
        }

        // Use provided graph or create simple fallback
        if (roomGraph == null || roomGraph.Count == 0)
        {
            Debug.LogWarning("No room connections found, using fallback assignment!");
            return CreateFallbackAssignments(rooms);
        }

        var entranceRoom = FindOptimalEntranceRoom(rooms, roomGraph);
        var distances = CalculateDistancesFromRoom(roomGraph, entranceRoom);
        
        AssignDistanceValues(rooms, distances);
        AssignCriticalRooms(rooms, distances, floorLevel, random);
        AssignSpecialRooms(rooms, distances, floorLevel, random);
        AssignEmptyRooms(rooms, random);
        GenerateSpawnPositions(rooms);
        
        Debug.Log($"Room assignment complete: {GetRoomTypeSummary(rooms)}");
        return rooms;
    }

    private RoomModel FindOptimalEntranceRoom(List<RoomModel> rooms, Dictionary<RoomModel, List<RoomModel>> graph)
    {
        return rooms.OrderBy(room => 
        {
            int connectionCount = graph.ContainsKey(room) ? graph[room].Count : int.MaxValue;
            int edgeDistance = CalculateEdgeDistance(room, rooms);
            return connectionCount * 1000 + edgeDistance;
        }).FirstOrDefault() ?? rooms[0];
    }

    private int CalculateEdgeDistance(RoomModel room, List<RoomModel> allRooms)
    {
        if (room?.Bounds == null || allRooms == null) 
            return int.MaxValue;

        // Calculate overall bounds from all rooms
        var allBounds = allRooms.Where(r => r?.Bounds != null).Select(r => r.Bounds);
        if (!allBounds.Any()) return int.MaxValue;

        int minX = allBounds.Min(b => b.xMin);
        int maxX = allBounds.Max(b => b.xMax);
        int minY = allBounds.Min(b => b.yMin);
        int maxY = allBounds.Max(b => b.yMax);

        return Mathf.Min(
            room.Bounds.xMin - minX, 
            maxX - room.Bounds.xMax,
            room.Bounds.yMin - minY, 
            maxY - room.Bounds.yMax
        );
    }

    private void AssignDistanceValues(List<RoomModel> rooms, Dictionary<RoomModel, int> distances)
    {
        if (rooms == null || distances == null) return;
        
        foreach (var room in rooms)
            if (room != null && distances.TryGetValue(room, out int distance))
                room.DistanceFromEntrance = distance;
    }

    private void AssignCriticalRooms(List<RoomModel> rooms, Dictionary<RoomModel, int> distances, int floorLevel, System.Random random)
    {
        if (rooms == null || rooms.Count < 2) 
        {
            Debug.LogWarning("Not enough rooms to assign critical rooms!");
            return;
        }

        var entranceRoom = rooms.OrderBy(r => distances.GetValueOrDefault(r, int.MaxValue)).First();
        var exitRoom = rooms.OrderByDescending(r => distances.GetValueOrDefault(r, int.MinValue)).First();
        
        // Ensure entrance and exit are different rooms
        if (entranceRoom == exitRoom && rooms.Count > 1)
        {
            exitRoom = rooms.Where(r => r != entranceRoom)
                           .OrderByDescending(r => distances.GetValueOrDefault(r, int.MinValue))
                           .First();
        }

        // Assign entrance
        entranceRoom.Type = RoomType.Entrance;
        entranceRoom.State = RoomAccess.Open;
        entranceRoom.IsRevealed = true;

        // Assign exit
        if (exitRoom != entranceRoom)
        {
            exitRoom.Type = RoomType.Exit;
            exitRoom.State = RoomAccess.Open;
            exitRoom.IsRevealed = true;
        }

        // Assign boss room on boss floors (every 5 floors)
        if (floorLevel % 5 == 0)
        {
            AssignBossRoom(rooms, exitRoom, distances);
        }

        Debug.Log($"Room assignment - Entrance: {entranceRoom.ID}, Exit: {exitRoom?.ID}, Total rooms: {rooms.Count}");
    }

    private void AssignBossRoom(List<RoomModel> rooms, RoomModel exitRoom, Dictionary<RoomModel, int> distances)
    {
        if (exitRoom == null) return;
        
        // Find a room connected to the exit room, far from entrance
        var bossCandidate = rooms
            .Where(r => r != null && r.Type == RoomType.Combat && r.ConnectedRooms.Contains(exitRoom))
            .OrderByDescending(r => distances.GetValueOrDefault(r, 0))
            .FirstOrDefault();
            
        // Fallback: any combat room far from entrance
        bossCandidate ??= rooms
            .Where(r => r != null && r.Type == RoomType.Combat)
            .OrderByDescending(r => distances.GetValueOrDefault(r, 0))
            .FirstOrDefault();

        if (bossCandidate != null)
        {
            bossCandidate.Type = RoomType.Boss;
            bossCandidate.State = RoomAccess.Closed;
        }
    }

    private void AssignSpecialRooms(List<RoomModel> rooms, Dictionary<RoomModel, int> distances, int floorLevel, System.Random random)
    {
        var combatRooms = rooms.Where(r => r != null && r.Type == RoomType.Combat).ToList();
        if (combatRooms.Count < 2) return;
        
        int targetShopRooms = 1;
        int targetTreasureRooms = 1;
        
        // Find mid-progress rooms (not too close, not too far from entrance)
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

    private void AssignEmptyRooms(List<RoomModel> rooms, System.Random random)
    {
        var combatRooms = rooms.Where(r => r != null && r.Type == RoomType.Combat).ToList();
        if (combatRooms.Count == 0) return;
        
        // Convert ~25% of combat rooms to empty rooms
        int emptyRoomCount = Mathf.Max(1, combatRooms.Count / 4);
        var roomsToMakeEmpty = combatRooms
            .OrderBy(r => random.NextDouble())
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
        foreach (var room in rooms)
        {
            if (room != null && (room.Type == RoomType.Combat || room.Type == RoomType.Boss))
            {
                GenerateRoomSpawnPositions(room, 3);
            }
        }
    }

    private void GenerateRoomSpawnPositions(RoomModel room, int count)
    {
        var innerTiles = GetRoomInnerTiles(room, RoomModel.SPAWN_PADDING).ToList();
        if (innerTiles.Count == 0) return;
        
        room.SpawnPositions.Clear();
        var usedPositions = new HashSet<Vector2Int>();
        
        for (int i = 0; i < Mathf.Min(count, innerTiles.Count); i++)
        {
            Vector2Int spawnPos = FindValidSpawnPosition(innerTiles, usedPositions);
            if (spawnPos != Vector2Int.zero)
            {
                room.SpawnPositions.Add(spawnPos);
                usedPositions.Add(spawnPos);
            }
        }
    }

    private IEnumerable<Vector2Int> GetRoomInnerTiles(RoomModel room, int padding)
    {
        if (room?.Bounds == null) yield break;

        for (int x = room.Bounds.xMin + padding; x < room.Bounds.xMax - padding; x++)
        {
            for (int y = room.Bounds.yMin + padding; y < room.Bounds.yMax - padding; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    private Vector2Int FindValidSpawnPosition(List<Vector2Int> availableTiles, HashSet<Vector2Int> usedPositions)
    {
        int attempts = 0;
        Vector2Int spawnPos;
        
        do {
            spawnPos = availableTiles[Random.Range(0, availableTiles.Count)];
            attempts++;
        } while (usedPositions.Contains(spawnPos) && attempts < 10);
        
        return attempts < 10 ? spawnPos : Vector2Int.zero;
    }

    private Dictionary<RoomModel, int> CalculateDistancesFromRoom(Dictionary<RoomModel, List<RoomModel>> graph, RoomModel startRoom)
    {
        var distances = new Dictionary<RoomModel, int>();
        if (graph == null || !graph.ContainsKey(startRoom)) return distances;
        
        var visited = new HashSet<RoomModel>();
        var queue = new Queue<(RoomModel, int)>();
        queue.Enqueue((startRoom, 0));
        visited.Add(startRoom);
        
        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            distances[current] = distance;
            
            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
                }
            }
        }
        return distances;
    }

    private List<RoomModel> CreateFallbackAssignments(List<RoomModel> rooms)
    {
        if (rooms == null || rooms.Count == 0)
            return new List<RoomModel>();
            
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i] != null)
            {
                if (i == 0)
                    rooms[i].Type = RoomType.Entrance;
                else if (i == rooms.Count - 1)
                    rooms[i].Type = RoomType.Exit;
                else
                    rooms[i].Type = RoomType.Combat;
                
                rooms[i].State = RoomAccess.Open;
                rooms[i].IsRevealed = (i == 0); // Only entrance revealed initially
            }
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

    // Existing methods from original RoomGenerator
    public void FindAndAssignNeighbors(List<PartitionModel> partitions)
    {
        if (partitions == null) return;
        
        foreach (var partition in partitions)
            partition.Neighbors.Clear();

        var rightEdgeMap = new Dictionary<int, List<PartitionModel>>();
        var bottomEdgeMap = new Dictionary<int, List<PartitionModel>>();

        foreach (var partition in partitions)
        {
            if (partition == null) continue;
            
            if (!rightEdgeMap.ContainsKey(partition.Bounds.xMax))
                rightEdgeMap[partition.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[partition.Bounds.xMax].Add(partition);

            if (!bottomEdgeMap.ContainsKey(partition.Bounds.yMax))
                bottomEdgeMap[partition.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[partition.Bounds.yMax].Add(partition);
        }

        foreach (var partition in partitions)
        {
            if (partition == null) continue;
            
            if (rightEdgeMap.TryGetValue(partition.Bounds.xMin, out var horizontalCandidates))
                FindValidNeighbors(partition, horizontalCandidates);
                
            if (bottomEdgeMap.TryGetValue(partition.Bounds.yMin, out var verticalCandidates))
                FindValidNeighbors(partition, verticalCandidates);
        }
    }

    private RoomModel CreateRoomInPartition(PartitionModel leaf, System.Random random, int roomId)
    {
        int maxHorizontalInset = (leaf.Bounds.width - RoomModel.MIN_SIZE) / 2;
        int maxVerticalInset = (leaf.Bounds.height - RoomModel.MIN_SIZE) / 2;
        
        int leftInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxHorizontalInset);
        int rightInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxHorizontalInset);
        int bottomInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxVerticalInset);
        int topInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxVerticalInset);

        int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
        int roomHeight = leaf.Bounds.height - (bottomInset + topInset);

        // Ensure room meets minimum size requirements
        if (roomWidth < RoomModel.MIN_SIZE || roomHeight < RoomModel.MIN_SIZE)
        {
            int neededWidth = RoomModel.MIN_SIZE - roomWidth;
            int neededHeight = RoomModel.MIN_SIZE - roomHeight;
            
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

        if (roomBounds.width >= RoomModel.MIN_SIZE && roomBounds.height >= RoomModel.MIN_SIZE)
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

    private void FindValidNeighbors(PartitionModel partition, List<PartitionModel> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate == partition) continue;
            
            if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
            {
                if (!partition.Neighbors.Contains(candidate))
                    partition.Neighbors.Add(candidate);
                if (!candidate.Neighbors.Contains(partition))
                    candidate.Neighbors.Add(partition);
            }
        }
    }

    private bool ArePartitionsNeighbors(RectInt boundsA, RectInt boundsB)
    {
        bool touchHorz = (boundsA.xMax == boundsB.xMin) || (boundsB.xMax == boundsA.xMin);
        bool touchVert = (boundsA.yMax == boundsB.yMin) || (boundsB.yMax == boundsA.yMin);
        bool overlapX = (boundsA.xMin < boundsB.xMax) && (boundsB.xMin < boundsA.xMax);
        bool overlapY = (boundsA.yMin < boundsB.yMax) && (boundsB.yMin < boundsA.yMax);
        
        return (touchHorz && overlapY) || (touchVert && overlapX);
    }
}