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
    /// <summary>
    /// Applies Minimum Spanning Tree to select the most efficient corridor connections
    /// Uses Kruskal's algorithm to find minimal set of corridors connecting all rooms
    /// </summary>
    /// <param name="corridors">All possible corridors between rooms</param>
    /// <param name="rooms">All rooms in the level</param>
    /// <returns>Minimal set of corridors that connect all rooms efficiently</returns>
    public static List<CorridorModel> Apply(List<CorridorModel> corridors, List<RoomModel> rooms)
    {
        try
        {
            // Validate input parameters
            if (rooms == null || rooms.Count == 0)
            {
                Debug.LogWarning("MSTService: No rooms provided - returning empty corridor list");
                return new List<CorridorModel>();
            }
            if (corridors == null)
            {
                Debug.LogWarning("MSTService: Null corridors list provided - returning empty list");
                return new List<CorridorModel>();
            }
            Debug.Log($"MSTService: Starting MST algorithm with {rooms.Count} rooms and {corridors.Count} possible corridors");
            var parentIds = InitializeUnionFind(rooms.Count);
            var spanningTreeCorridors = new List<CorridorModel>();
            // Sort corridors by distance (shorter corridors first for optimal MST)
            corridors.Sort((a, b) =>
            {
                if (a == null || b == null) 
                {
                    Debug.LogWarning("MSTService: Found null corridor during sorting");
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
                    Debug.LogError($"MSTService: Error calculating corridor distance: {ex.Message}");
                    return 0;
                }
            });
            Debug.Log("MSTService: Corridors sorted by distance, building spanning tree...");
            int selectedCorridors = 0;
            int skippedCorridors = 0;
            foreach (var corridor in corridors)
            {
                if (corridor?.StartRoom == null || corridor.EndRoom == null)
                {
                    Debug.LogWarning("MSTService: Skipping corridor with null start or end room");
                    skippedCorridors++;
                    continue;
                }
                int roomAIndex = rooms.IndexOf(corridor.StartRoom);
                int roomBIndex = rooms.IndexOf(corridor.EndRoom);
                if (roomAIndex < 0 || roomBIndex < 0)
                {
                    Debug.LogWarning("MSTService: Skipping corridor with rooms not in room list");
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
                    if (selectedCorridors >= rooms.Count - 1)
                    {
                        Debug.Log("MSTService: Early exit - all rooms connected");
                        break;
                    }
                }
                else skippedCorridors++;
            }
            Debug.Log($"MSTService: MST completed - {selectedCorridors} corridors selected, {skippedCorridors} skipped");
            Debug.Log($"MSTService: Final result: {spanningTreeCorridors.Count} corridors from {corridors.Count} possible");
            return spanningTreeCorridors;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MSTService: Error applying MST algorithm: {ex.Message}");
            return new List<CorridorModel>(); // Return empty list on error
        }
    }

    private static int[] InitializeUnionFind(int roomCount)
    {
        try
        {
            var parentIds = new int[roomCount];
            for (int i = 0; i < roomCount; i++) parentIds[i] = i;
            Debug.Log($"MSTService: Initialized union-find for {roomCount} rooms");
            return parentIds;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MSTService: Error initializing union-find: {ex.Message}");
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
            Debug.LogError($"MSTService: Error finding root for element {elementId}: {ex.Message}");
            return elementId; // Return self as fallback
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
            Debug.LogError($"MSTService: Error unioning sets {a} and {b}: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if all rooms are connected by the given corridors
    /// Uses union-find to verify connectivity of all rooms
    /// </summary>
    /// <param name="rooms">List of rooms to check connectivity for</param>
    /// <param name="corridors">List of corridors connecting rooms</param>
    /// <returns>True if all rooms are connected, false otherwise</returns>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    /*
    public static bool AreAllRoomsConnected(List<RoomModel> rooms, List<CorridorModel> corridors)
    {
        try
        {
            // Handle edge cases
            if (rooms == null || rooms.Count == 0)
            {
                Debug.LogWarning("MSTService: No rooms provided for connectivity check");
                return true; // Empty or null set is trivially connected
            }
            
            if (corridors == null)
            {
                Debug.LogWarning("MSTService: Null corridors provided for connectivity check");
                return false;
            }

            Debug.Log($"MSTService: Checking connectivity for {rooms.Count} rooms with {corridors.Count} corridors");

            var parentIds = InitializeUnionFind(rooms.Count);
            
            // Process all corridors to build connectivity
            foreach (var corridor in corridors)
            {
                if (corridor?.StartRoom == null || corridor.EndRoom == null)
                {
                    Debug.LogWarning("MSTService: Skipping null corridor in connectivity check");
                    continue;
                }

                int roomAIndex = rooms.IndexOf(corridor.StartRoom);
                int roomBIndex = rooms.IndexOf(corridor.EndRoom);
                
                if (roomAIndex >= 0 && roomBIndex >= 0)
                {
                    UnionSets(roomAIndex, roomBIndex, parentIds);
                }
                else
                {
                    Debug.LogWarning("MSTService: Corridor references rooms not in room list");
                }
            }

            // Check if all rooms have the same root (are connected)
            int firstRoot = FindRoot(0, parentIds);
            for (int i = 1; i < rooms.Count; i++)
            {
                if (FindRoot(i, parentIds) != firstRoot)
                {
                    Debug.Log($"MSTService: Rooms are not connected - room {i} has different root");
                    return false;
                }
            }

            Debug.Log("MSTService: All rooms are connected");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MSTService: Error checking room connectivity: {ex.Message}");
            return false; // Assume not connected on error
        }
    }
    */

    /// <summary>
    /// Calculates the total length of all corridors in the list
    /// Useful for analyzing MST efficiency
    /// </summary>
    /// <param name="corridors">List of corridors to measure</param>
    /// <returns>Total length of all corridors</returns>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    /*
    public static float CalculateTotalCorridorLength(List<CorridorModel> corridors)
    {
        try
        {
            if (corridors == null || corridors.Count == 0)
            {
                return 0f;
            }

            float totalLength = 0f;
            foreach (var corridor in corridors)
            {
                if (corridor?.StartRoom != null && corridor.EndRoom != null)
                {
                    totalLength += Vector2.Distance(
                        corridor.StartRoom.Bounds.center, 
                        corridor.EndRoom.Bounds.center
                    );
                }
            }

            Debug.Log($"MSTService: Total corridor length: {totalLength:F2}");
            return totalLength;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MSTService: Error calculating corridor length: {ex.Message}");
            return 0f;
        }
    }
    */
}