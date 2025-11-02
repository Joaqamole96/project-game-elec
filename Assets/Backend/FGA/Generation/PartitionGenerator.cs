// PartitionGenerator.cs
using System.Collections.Generic;
using UnityEngine;

namespace FGA.Generation
{
    // Simple partition node used internally
    public class PartitionGenerator
    {
        System.Random rng;
        int minSize;
        int maxDepth;

        public PartitionGenerator(int seed, int minSize = 6, int maxDepth = 5)
        {
            rng = new System.Random(seed);
            this.minSize = minSize;
            this.maxDepth = maxDepth;
        }

        public List<RectInt> Generate(int width, int height)
        {
            var root = new Node(0, 0, width, height, 0);
            var leaves = new List<RectInt>();
            SplitNode(root);
            CollectLeaves(root, leaves);
            return leaves;
        }

        void SplitNode(Node n)
        {
            if (n.depth >= maxDepth) return;
            bool splitVert = rng.NextDouble() > 0.5;
            if (n.width < minSize * 2) splitVert = false;
            if (n.height < minSize * 2) splitVert = true;

            if (splitVert)
            {
                int split = rng.Next(minSize, n.width - minSize + 1);
                n.left = new Node(n.x, n.y, split, n.height, n.depth + 1);
                n.right = new Node(n.x + split, n.y, n.width - split, n.height, n.depth + 1);
            }
            else
            {
                int split = rng.Next(minSize, n.height - minSize + 1);
                n.left = new Node(n.x, n.y, n.width, split, n.depth + 1);
                n.right = new Node(n.x, n.y + split, n.width, n.height - split, n.depth + 1);
            }

            SplitNode(n.left); SplitNode(n.right);
        }

        void CollectLeaves(Node n, List<RectInt> outList)
        {
            if (n.left == null && n.right == null) { outList.Add(new RectInt(n.x, n.y, n.width, n.height)); return; }
            if (n.left != null) CollectLeaves(n.left, outList);
            if (n.right != null) CollectLeaves(n.right, outList);
        }

        class Node { public int x, y, width, height, depth; public Node left, right; public Node(int X,int Y,int W,int H,int D){x=X;y=Y;width=W;height=H;depth=D;} }
    }
}