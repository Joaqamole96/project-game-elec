// MinimumSpanningTree.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Applies Minimum Spanning Tree algorithm to create efficient room connectivity.
/// Uses Kruskal's algorithm with union-find data structure.
/// </summary>
public static class MinimumSpanningTree
{
    /// <summary>
    /// Applies Minimum Spanning Tree to select the most efficient corridor connections.
    /// </summary>
    /// <param name="corridors">All possible corridors between rooms.</param>
    /// <param name="rooms">All rooms in the level.</param>
    /// <returns>Minimal set of corridors that connect all rooms.</returns>
    public static List<CorridorModel> Apply(List<CorridorModel> corridors, List<RoomModel> rooms)
    {
        if (rooms == null || rooms.Count == 0 || corridors == null)
            return corridors ?? new List<CorridorModel>();
        
        var parentIds = InitializeUnionFind(rooms.Count);
        var spanningTreeCorridors = new List<CorridorModel>();

        // Sort corridors by distance (shorter corridors first)
        corridors.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
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

        Debug.Log($"MST applied: {spanningTreeCorridors.Count} corridors selected from {corridors.Count} possible");
        return spanningTreeCorridors;
    }

    private static int[] InitializeUnionFind(int roomCount)
    {
        var parentIds = new int[roomCount];
        for (int i = 0; i < roomCount; i++)
            parentIds[i] = i;
        return parentIds;
    }

    private static int FindRoot(int elementId, int[] parentIds)
    {
        if (parentIds[elementId] != elementId)
            parentIds[elementId] = FindRoot(parentIds[elementId], parentIds);
        return parentIds[elementId];
    }

    private static void UnionSets(int a, int b, int[] parentIds)
    {
        int rootA = FindRoot(a, parentIds);
        int rootB = FindRoot(b, parentIds);
        if (rootA != rootB)
            parentIds[rootB] = rootA;
    }

    /// <summary>
    /// Checks if all rooms are connected by the given corridors.
    /// </summary>
    public static bool AreAllRoomsConnected(List<RoomModel> rooms, List<CorridorModel> corridors)
    {
        if (rooms == null || rooms.Count == 0) return true;
        if (corridors == null) return false;
        
        var parentIds = InitializeUnionFind(rooms.Count);
        
        foreach (var corridor in corridors)
        {
            if (corridor?.StartRoom == null || corridor.EndRoom == null) continue;
                
            int roomAIndex = rooms.IndexOf(corridor.StartRoom);
            int roomBIndex = rooms.IndexOf(corridor.EndRoom);
            
            if (roomAIndex >= 0 && roomBIndex >= 0)
            {
                UnionSets(roomAIndex, roomBIndex, parentIds);
            }
        }

        // Check if all rooms have the same root
        int firstRoot = FindRoot(0, parentIds);
        for (int i = 1; i < rooms.Count; i++)
        {
            if (FindRoot(i, parentIds) != firstRoot)
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Validates if a corridor can be used in the MST algorithm.
    /// </summary>
    private static bool IsValidCorridor(CorridorModel corridor, List<RoomModel> rooms)
    {
        return corridor?.StartRoom != null && 
               corridor.EndRoom != null && 
               rooms.Contains(corridor.StartRoom) && 
               rooms.Contains(corridor.EndRoom);
    }
}