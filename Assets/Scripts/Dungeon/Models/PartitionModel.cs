using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Models 
{
    public class PartitionModel
    {
        public RectInt Bounds;
        public PartitionModel LeftHalf;
        public PartitionModel RightHalf;
        public Room Room;
        public List<PartitionModel> Neighbors;

        public int X => Bounds.x;
        public int Y => Bounds.y;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        public PartitionModel(RectInt bounds)
        {
            Bounds = bounds;
            Neighbors = new List<PartitionModel>();
        }
    }
}