// -------------------- //
// Scripts/Generators/PartitionGenerator.cs
// -------------------- //

using System.Collections.Generic;
using UnityEngine;

public class PartitionGenerator
{
    public PartitionModel GeneratePartitionTree(LevelModel level, PartitionConfig partitionConfig, System.Random random)
    {
        PartitionModel rootPartition = new(new RectInt(0, 0, level.Width, level.Height));
        
        SplitRecursively(rootPartition, partitionConfig, random);
        
        Debug.Log("PartitionGenerator.GeneratePartitionTree(): Generated partition tree successfully.");
        return rootPartition;
    }
    
    private void SplitRecursively(PartitionModel partition, PartitionConfig config, System.Random random)
    {
        bool canSplitVert = (partition.Height >= (PartitionModel.MIN_SIZE * 2 + 1));
        bool canSplitHorz = (partition.Width >= (PartitionModel.MIN_SIZE * 2 + 1));

        if (!canSplitVert && !canSplitHorz) return;

        bool splitVert;

        if (canSplitVert && canSplitHorz) splitVert = (partition.Height > partition.Width);
        else if (canSplitVert) splitVert = true;
        else splitVert = false;

        float splitRatio = (float)(random.NextDouble() * (config.MaxSplitRatio - config.MinSplitRatio) + config.MinSplitRatio);
        
        if (splitVert) SplitVert(partition, splitRatio);
        else SplitHorz(partition, splitRatio);

        SplitRecursively(partition.LeftChild, config, random);
        SplitRecursively(partition.RightChild, config, random);
    }

    private void SplitVert(PartitionModel partition, float splitRatio)
    {
        int minSplit = PartitionModel.MIN_SIZE;
        int maxSplit = partition.Width - PartitionModel.MIN_SIZE;

        if (maxSplit <= minSplit) return;

        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(partition.Width * splitRatio),
            minSplit,
            maxSplit
        );

        partition.LeftChild = new(new RectInt(partition.X, partition.Y, splitPoint, partition.Height));
        partition.RightChild = new(new RectInt(partition.X + splitPoint, partition.Y, partition.Width - splitPoint, partition.Height));
    }

    private void SplitHorz(PartitionModel partition, float splitRatio)
    {
        int minSplit = PartitionModel.MIN_SIZE;
        int maxSplit = partition.Height - PartitionModel.MIN_SIZE;
        
        if (maxSplit <= minSplit) return;

        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(partition.Height * splitRatio),
            minSplit,
            maxSplit
        );

        partition.LeftChild = new(new RectInt(partition.X, partition.Y, partition.Width, splitPoint));
        partition.RightChild = new(new RectInt(partition.X, partition.Y + splitPoint, partition.Width, partition.Height - splitPoint));
    }

    public List<PartitionModel> CollectLeaves(PartitionModel rootPartition)
    {
        List<PartitionModel> leaves = new();

        GetLeavesRecursively(rootPartition, leaves);

        Debug.Log("PartitionGenerator.CollectLeaves(): Collected all leaf partitions successfully.");
        return leaves;
    }

    private void GetLeavesRecursively(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) return;
        
        if (partition.IsLeaf) leaves.Add(partition);
        else
        {
            GetLeavesRecursively(partition.LeftChild, leaves);
            GetLeavesRecursively(partition.RightChild, leaves);
        }
    }
}