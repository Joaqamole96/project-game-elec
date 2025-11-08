using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MinimumSpanningTree
{
    public static List<CorridorModel> Apply(List<CorridorModel> corridors, List<RoomModel> rooms)
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