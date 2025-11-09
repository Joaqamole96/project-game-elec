// RoomAssigner.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Assigns room types and purposes based on dungeon progression and connectivity.
/// </summary>
public class RoomAssigner
{
    /// <summary>
    /// Assigns room types based on dungeon progression and connectivity.
    /// </summary>
    public List<RoomModel> AssignRooms(LevelModel layout, int floorLevel, System.Random random)
    {
        if (layout?.Rooms == null || layout.Rooms.Count == 0)
        {
            Debug.LogWarning("No rooms to assign!");
            return CreateFallbackAssignments(layout?.Rooms);
        }

        layout.InitializeSpatialData();
        var roomGraph = layout.RoomGraph;
        
        if (roomGraph.Count == 0)
        {
            Debug.LogWarning("No room connections found!");
            return CreateFallbackAssignments(layout.Rooms);
        }

        var entranceRoom = FindOptimalEntranceRoom(layout.Rooms, roomGraph, layout);
        var distances = CalculateDistancesFromRoom(roomGraph, entranceRoom);
        
        AssignDistanceValues(layout.Rooms, distances);
        AssignCriticalRooms(layout.Rooms, distances, floorLevel, random);
        AssignSpecialRooms(layout.Rooms, floorLevel, random);
        AssignEmptyRooms(layout.Rooms, random);
        
        GenerateSpawnPositions(layout.Rooms);
        
        Debug.Log($"Room assignment complete: {GetRoomTypeSummary(layout.Rooms)}");
        return layout.Rooms;
    }

    private RoomModel FindOptimalEntranceRoom(List<RoomModel> rooms, Dictionary<RoomModel, List<RoomModel>> graph, LevelModel layout)
    {
        return rooms.OrderBy(room => 
        {
            int connectionCount = graph.ContainsKey(room) ? graph[room].Count : int.MaxValue;
            int edgeDistance = CalculateEdgeDistance(room, layout);
            return connectionCount * 1000 + edgeDistance; // Prioritize well-connected edge rooms
        }).FirstOrDefault() ?? rooms[0];
    }

    private int CalculateEdgeDistance(RoomModel room, LevelModel layout)
    {
        if (room?.Bounds == null || layout?.OverallBounds == null) return int.MaxValue;
        
        return Mathf.Min(
            room.Bounds.xMin, 
            layout.OverallBounds.size.x - room.Bounds.xMax,
            room.Bounds.yMin, 
            layout.OverallBounds.size.y - room.Bounds.yMax
        );
    }

    private void AssignDistanceValues(List<RoomModel> rooms, Dictionary<RoomModel, int> distances)
    {
        if (rooms == null || distances == null) return;

        foreach (var room in rooms)
        {
            if (room != null && distances.TryGetValue(room, out int distance))
            {
                room.DistanceFromEntrance = distance;
            }
        }
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

        // Ensure we have distinct rooms for entrance and exit
        if (entranceRoom == exitRoom && rooms.Count > 1)
        {
            exitRoom = rooms.Where(r => r != entranceRoom)
                          .OrderByDescending(r => distances.GetValueOrDefault(r, int.MinValue))
                          .First();
        }

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

        if (floorLevel % 5 == 0)
        {
            AssignBossRoom(rooms, exitRoom);
        }
        
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

    private void AssignSpecialRooms(List<RoomModel> rooms, int floorLevel, System.Random random)
    {
        var combatRooms = rooms.Where(r => r != null && r.Type == RoomType.Combat).ToList();
        if (combatRooms.Count < 2) return;

        // These would come from GameConfig in the real implementation
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

    private void AssignEmptyRooms(List<RoomModel> rooms, System.Random random)
    {
        var combatRooms = rooms.Where(r => r != null && r.Type == RoomType.Combat).ToList();
        if (combatRooms.Count == 0) return;

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
        // This would use GameConfig.EnemiesPerCombatRoom in real implementation
        int enemiesPerRoom = 3;
        
        foreach (var room in rooms)
        {
            if (room != null && (room.Type == RoomType.Combat || room.Type == RoomType.Boss))
            {
                room.GenerateSpawnPositions(enemiesPerRoom);
            }
        }
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
                rooms[i].Type = i == 0 ? RoomType.Entrance : 
                              i == rooms.Count - 1 ? RoomType.Exit : RoomType.Combat;
            }
        }
        return rooms;
    }

    private string GetRoomTypeSummary(List<RoomModel> rooms)
    {
        if (rooms == null) return "No rooms";
        return string.Join(", ", rooms.Where(r => r != null)
                                    .GroupBy(r => r.Type)
                                    .Select(g => $"{g.Key}: {g.Count()}"));
    }
}