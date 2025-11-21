// MinimumSpanningTree.cs
using System.Collections.Generic;
using UnityEngine;

public static class MinimumSpanningTree
{
    public static List<CorridorModel> Apply(List<CorridorModel> corridors, List<RoomModel> rooms)
    {
        if (rooms == null || rooms.Count == 0 || corridors == null)
            return corridors ?? new List<CorridorModel>();
        var parentIds = InitializeUnionFind(rooms.Count);
        var spanningTreeCorridors = new List<CorridorModel>();
        corridors.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            float distA = Vector2.Distance(a.StartRoom.Bounds.center, a.EndRoom.Bounds.center);
            float distB = Vector2.Distance(b.StartRoom.Bounds.center, b.EndRoom.Bounds.center);
            return distA.CompareTo(distB);
        });
        foreach (var corridor in corridors)
        {
            if (corridor?.StartRoom == null || corridor.EndRoom == null) 
                continue;
            int roomAIndex = rooms.IndexOf(corridor.StartRoom);
            int roomBIndex = rooms.IndexOf(corridor.EndRoom);
            if (roomAIndex < 0 || roomBIndex < 0) 
                continue;
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
}