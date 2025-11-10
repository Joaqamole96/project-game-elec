using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EARLY room assignment - assigns room types to partitions BEFORE room creation.
/// Enables predetermined room sizes based on room type.
/// </summary>
public class RoomAssigner
{
    /// <summary>
    /// Pre-assigns room types to partitions for early size determination.
    /// </summary>
    public void PreAssignRoomTypes(List<PartitionModel> partitions, int floorLevel, System.Random random)
    {
        if (partitions == null || partitions.Count == 0)
        {
            Debug.LogWarning("No partitions to assign room types!");
            return;
        }

        // Reset all partitions to combat type first
        foreach (var partition in partitions)
        {
            partition.PreAssignedRoomType = RoomType.Combat;
        }

        AssignCriticalRooms(partitions, floorLevel, random);
        AssignSpecialRooms(partitions, floorLevel, random);
        AssignEmptyRooms(partitions, random);

        Debug.Log($"Pre-assigned room types: {GetRoomTypeSummary(partitions)}");
    }

    private void AssignCriticalRooms(List<PartitionModel> partitions, int floorLevel, System.Random random)
    {
        // Find optimal entrance (edge, well-connected)
        var entrancePartition = FindOptimalEntrancePartition(partitions);
        if (entrancePartition != null)
        {
            entrancePartition.PreAssignedRoomType = RoomType.Entrance;
            Debug.Log($"Assigned Entrance: Partition at {entrancePartition.Bounds.position}");
        }

        // Find furthest partition for exit
        var exitPartition = FindFurthestPartition(partitions, entrancePartition);
        if (exitPartition != null && exitPartition != entrancePartition)
        {
            exitPartition.PreAssignedRoomType = RoomType.Exit;
            Debug.Log($"Assigned Exit: Partition at {exitPartition.Bounds.position}");
        }

        // Boss room on boss floors
        if (floorLevel % 5 == 0)
        {
            var bossPartition = FindBossPartition(partitions, exitPartition);
            if (bossPartition != null)
            {
                bossPartition.PreAssignedRoomType = RoomType.Boss;
                Debug.Log($"Assigned Boss: Partition at {bossPartition.Bounds.position}");
            }
        }
    }

    private PartitionModel FindOptimalEntrancePartition(List<PartitionModel> partitions)
    {
        return partitions.OrderBy(p => 
        {
            int edgeDistance = CalculateEdgeDistance(p);
            int neighborCount = p.Neighbors.Count;
            return edgeDistance * 10 + neighborCount; // Prefer edge partitions with some connections
        }).FirstOrDefault();
    }

    private int CalculateEdgeDistance(PartitionModel partition)
    {
        // Simple edge distance calculation
        return Mathf.Min(
            partition.Bounds.x,
            partition.Bounds.y
        );
    }

    private PartitionModel FindFurthestPartition(List<PartitionModel> partitions, PartitionModel fromPartition)
    {
        if (fromPartition == null) return partitions.FirstOrDefault();
        
        return partitions.OrderByDescending(p => 
            Vector2Int.Distance(p.Center, fromPartition.Center)
        ).FirstOrDefault();
    }

    private PartitionModel FindBossPartition(List<PartitionModel> partitions, PartitionModel exitPartition)
    {
        if (exitPartition == null) return partitions.FirstOrDefault();
        
        // Find a partition adjacent to the exit room
        return partitions.FirstOrDefault(p => 
            p.Neighbors.Contains(exitPartition) && 
            p.PreAssignedRoomType == RoomType.Combat
        ) ?? partitions.FirstOrDefault(p => p.PreAssignedRoomType == RoomType.Combat);
    }

    private void AssignSpecialRooms(List<PartitionModel> partitions, int floorLevel, System.Random random)
    {
        var combatPartitions = partitions.Where(p => p.PreAssignedRoomType == RoomType.Combat).ToList();
        if (combatPartitions.Count < 2) return;

        // Assign shop rooms (1 per floor)
        if (combatPartitions.Count > 0)
        {
            var shopPartition = combatPartitions[random.Next(combatPartitions.Count)];
            shopPartition.PreAssignedRoomType = RoomType.Shop;
            combatPartitions.Remove(shopPartition);
        }

        // Assign treasure rooms (1 per floor)  
        if (combatPartitions.Count > 0)
        {
            var treasurePartition = combatPartitions[random.Next(combatPartitions.Count)];
            treasurePartition.PreAssignedRoomType = RoomType.Treasure;
        }
    }

    private void AssignEmptyRooms(List<PartitionModel> partitions, System.Random random)
    {
        var combatPartitions = partitions.Where(p => p.PreAssignedRoomType == RoomType.Combat).ToList();
        int emptyRoomCount = Mathf.Max(1, combatPartitions.Count / 4);

        for (int i = 0; i < emptyRoomCount && combatPartitions.Count > 0; i++)
        {
            var emptyPartition = combatPartitions[random.Next(combatPartitions.Count)];
            emptyPartition.PreAssignedRoomType = RoomType.Empty;
            combatPartitions.Remove(emptyPartition);
        }
    }

    private string GetRoomTypeSummary(List<PartitionModel> partitions)
    {
        return string.Join(", ", partitions
            .GroupBy(p => p.PreAssignedRoomType)
            .Select(g => $"{g.Key}: {g.Count()}"));
    }

    // KEEP the old AssignRooms method but mark it obsolete or remove if unused
    // We'll update the pipeline to use pre-assignment instead
}