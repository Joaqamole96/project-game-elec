using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomAssigner
{
    public List<RoomModel> AssignRooms(LevelModel layout, int floorLevel, System.Random random)
    {
        if (layout?.Rooms == null || layout.Rooms.Count == 0)
        {
            Debug.LogWarning("No rooms to assign!");
            return new List<RoomModel>();
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
        
        foreach (var room in layout.Rooms)
        {
            distances.TryGetValue(room, out room.DistanceFromEntrance);
        }

        AssignCriticalRooms(layout.Rooms, distances, floorLevel, random);
        AssignSpecialRooms(layout.Rooms, floorLevel, random);
        AssignEmptyRooms(layout.Rooms, random);
        
        GenerateSpawnPositions(layout.Rooms);
        
        Debug.Log($"Room assignment complete: {layout.Rooms.Count} rooms");
        return layout.Rooms;
    }

    private RoomModel FindOptimalEntranceRoom(List<RoomModel> rooms, Dictionary<RoomModel, List<RoomModel>> graph, LevelModel layout)
    {
        return rooms.OrderBy(room => 
        {
            int connectionCount = graph.ContainsKey(room) ? graph[room].Count : int.MaxValue;
            int edgeDistance = Mathf.Min(
                room.Bounds.xMin, 
                layout.OverallBounds.size.x - room.Bounds.xMax,
                room.Bounds.yMin, 
                layout.OverallBounds.size.y - room.Bounds.yMax
            );
            return connectionCount * 1000 + edgeDistance;
        }).FirstOrDefault() ?? rooms[0];
    }

    private void AssignCriticalRooms(List<RoomModel> rooms, Dictionary<RoomModel, int> distances, int floorLevel, System.Random random)
    {
        if (rooms.Count < 2) return;

        var entranceRoom = rooms.OrderBy(r => r.DistanceFromEntrance).First();
        var exitRoom = rooms.OrderByDescending(r => r.DistanceFromEntrance).First();

        if (entranceRoom == exitRoom && rooms.Count > 1)
        {
            exitRoom = rooms.OrderByDescending(r => r.DistanceFromEntrance)
                           .First(r => r != entranceRoom);
        }

        entranceRoom.Type = RoomType.Entrance;
        entranceRoom.State = RoomAccess.Open;
        entranceRoom.IsRevealed = true;

        exitRoom.Type = RoomType.Exit;
        exitRoom.State = RoomAccess.Open;
        exitRoom.IsRevealed = true;

        if (floorLevel % 5 == 0)
        {
            AssignBossRoom(rooms, exitRoom);
        }
    }

    private void AssignBossRoom(List<RoomModel> rooms, RoomModel exitRoom)
    {
        var bossCandidate = rooms
            .Where(r => r.Type == RoomType.Combat && r.ConnectedRooms.Contains(exitRoom))
            .OrderByDescending(r => r.DistanceFromEntrance)
            .FirstOrDefault();

        bossCandidate ??= rooms
            .Where(r => r.Type == RoomType.Combat)
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
        var combatRooms = rooms.Where(r => r.Type == RoomType.Combat).ToList();
        if (combatRooms.Count < 2) return;

        // These would come from GameConfig in the real implementation
        int targetShopRooms = 1;
        int targetTreasureRooms = 1;

        var midProgressRooms = combatRooms
            .OrderBy(r => Mathf.Abs((float)(r.DistanceFromEntrance - combatRooms.Average(room => room.DistanceFromEntrance))))
            .ToList();

        for (int i = 0; i < targetShopRooms && i < midProgressRooms.Count; i++)
        {
            midProgressRooms[i].Type = RoomType.Shop;
            midProgressRooms[i].State = RoomAccess.Open;
        }

        int treasureStart = targetShopRooms;
        for (int i = 0; i < targetTreasureRooms && treasureStart + i < midProgressRooms.Count; i++)
        {
            midProgressRooms[treasureStart + i].Type = RoomType.Treasure;
            midProgressRooms[treasureStart + i].State = RoomAccess.Open;
        }
    }

    private void AssignEmptyRooms(List<RoomModel> rooms, System.Random random)
    {
        var combatRooms = rooms.Where(r => r.Type == RoomType.Combat).ToList();
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
            if (room.Type == RoomType.Combat || room.Type == RoomType.Boss)
            {
                room.GenerateSpawnPositions(enemiesPerRoom);
            }
        }
    }

    private List<RoomModel> CreateFallbackAssignments(List<RoomModel> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].Type = i == 0 ? RoomType.Entrance : 
                          i == rooms.Count - 1 ? RoomType.Exit : RoomType.Combat;
        }
        return rooms;
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
}