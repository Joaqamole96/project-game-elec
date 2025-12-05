// -------------------------------------------------- //
// Scripts/Services/MSTService.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service for applying Minimum Spanning Tree algorithm to create efficient room connectivity
/// Uses Kruskal's algorithm with union-find data structure for optimal path selection
/// Ensures all rooms are connected with minimal total corridor length
/// </summary>
public static class MSTService
{
    public static List<CorridorModel> Apply(List<CorridorModel> corridors, List<RoomModel> rooms)
    {
        try
        {
            // Validate input parameters
            if (rooms == null || rooms.Count == 0)
            {
                Debug.LogWarning("No rooms provided - returning empty corridor list");
                return new();
            }
            if (corridors == null)
            {
                Debug.LogWarning("Null corridors list provided - returning empty list");
                return new();
            }
            
            var parentIds = InitializeUnionFind(rooms.Count);
            List<CorridorModel> spanningTreeCorridors = new();
            
            // Sort corridors by distance (shorter corridors first for optimal MST)
            corridors.Sort((a, b) =>
            {
                if (a == null || b == null) 
                {
                    Debug.LogWarning("Found null corridor during sorting");
                    return 0;
                }

                try
                {
                    float distA = Vector2.Distance(a.StartRoom.Bounds.center, a.EndRoom.Bounds.center);
                    float distB = Vector2.Distance(b.StartRoom.Bounds.center, b.EndRoom.Bounds.center);
                    return distA.CompareTo(distB);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error calculating corridor distance: {ex.Message}");
                    return 0;
                }
            });
            
            int selectedCorridors = 0;
            int skippedCorridors = 0;

            foreach (var corridor in corridors)
            {
                if (corridor?.StartRoom == null || corridor.EndRoom == null)
                {
                    Debug.LogWarning("Skipping corridor with null start or end room");
                    skippedCorridors++;
                    continue;
                }

                int roomAIndex = rooms.IndexOf(corridor.StartRoom);
                int roomBIndex = rooms.IndexOf(corridor.EndRoom);

                if (roomAIndex < 0 || roomBIndex < 0)
                {
                    Debug.LogWarning("Skipping corridor with rooms not in room list");
                    skippedCorridors++;
                    continue;
                }

                // Check if adding this corridor would create a cycle
                if (FindRoot(roomAIndex, parentIds) != FindRoot(roomBIndex, parentIds))
                {
                    spanningTreeCorridors.Add(corridor);
                    UnionSets(roomAIndex, roomBIndex, parentIds);
                    selectedCorridors++;
                    // Early exit if we've connected all rooms (MST has n-1 edges for n nodes)
                    if (selectedCorridors >= rooms.Count - 1) break;
                }
                else skippedCorridors++;
            }
            
            return spanningTreeCorridors;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error applying MST algorithm: {ex.Message}");
            return new();
        }
    }

    private static int[] InitializeUnionFind(int roomCount)
    {
        try
        {
            var parentIds = new int[roomCount];
            for (int i = 0; i < roomCount; i++) parentIds[i] = i;
            
            return parentIds;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error initializing union-find: {ex.Message}");
            return new int[0];
        }
    }

    private static int FindRoot(int elementId, int[] parentIds)
    {
        try
        {
            // Path compression: make nodes point directly to root
            if (parentIds[elementId] != elementId) parentIds[elementId] = FindRoot(parentIds[elementId], parentIds);

            return parentIds[elementId];
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error finding root for element {elementId}: {ex.Message}");
            return elementId;
        }
    }

    private static void UnionSets(int a, int b, int[] parentIds)
    {
        try
        {
            int rootA = FindRoot(a, parentIds);
            int rootB = FindRoot(b, parentIds);
            if (rootA != rootB) parentIds[rootB] = rootA;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error unioning sets {a} and {b}: {ex.Message}");
        }
    }
}