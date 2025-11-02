// RoomGraphBuilder.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FGA.Generation
{
    public class RoomConnection
    {
        public int A, B;
        public Vector2Int doorPos; // shared wall position (we'll duplicate for convenience)
        public RoomConnection(int a, int b, Vector2Int door) { A = a; B = b; doorPos = door; }
    }

    public class RoomGraphBuilder
    {
        System.Random rng;
        double loopChance;

        public RoomGraphBuilder(int seed, double extraLoopChance = 0.2)
        {
            rng = new System.Random(seed);
            loopChance = extraLoopChance;
        }

        // Determine adjacency between partitions by touching edges
        public List<RoomConnection> BuildGraph(List<RectInt> partitions)
        {
            var edges = new List<RoomConnection>();
            int n = partitions.Count;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (TryAdjacent(partitions[i], partitions[j], out Vector2Int door))
                        edges.Add(new RoomConnection(i, j, door));
                }
            }

            // Compute random MST (Kruskal-like, via shuffled edges + union-find)
            var rndEdges = edges.OrderBy(e => rng.Next()).ToList();
            var uf = new UnionFind(n);
            var mst = new List<RoomConnection>();
            foreach (var e in rndEdges)
            {
                if (uf.Union(e.A, e.B)) mst.Add(e);
            }

            // Add a few extra edges for loops
            foreach (var e in edges)
            {
                if (!mst.Contains(e) && rng.NextDouble() < loopChance) mst.Add(e);
            }

            return mst;
        }

        bool TryAdjacent(RectInt a, RectInt b, out Vector2Int door)
        {
            door = default;
            // vertical adjacency
            if (a.xMax == b.xMin || b.xMax == a.xMin)
            {
                int overlapMin = Mathf.Max(a.yMin, b.yMin);
                int overlapMax = Mathf.Min(a.yMax - 1, b.yMax - 1);
                if (overlapMax >= overlapMin)
                {
                    int y = rng.Next(overlapMin, overlapMax + 1);
                    int x = (a.xMax == b.xMin) ? a.xMax - 1 : b.xMax - 1;
                    door = new Vector2Int(x, y);
                    return true;
                }
            }
            // horizontal adjacency
            if (a.yMax == b.yMin || b.yMax == a.yMin)
            {
                int overlapMin = Mathf.Max(a.xMin, b.xMin);
                int overlapMax = Mathf.Min(a.xMax - 1, b.xMax - 1);
                if (overlapMax >= overlapMin)
                {
                    int x = rng.Next(overlapMin, overlapMax + 1);
                    int y = (a.yMax == b.yMin) ? a.yMax - 1 : b.yMax - 1;
                    door = new Vector2Int(x, y);
                    return true;
                }
            }
            return false;
        }

        // Simple union-find
        class UnionFind
        {
            int[] p;
            public UnionFind(int n) { p = new int[n]; for (int i = 0; i < n; i++) p[i] = i; }
            int Find(int x) { if (p[x] == x) return x; p[x] = Find(p[x]); return p[x]; }
            public bool Union(int a, int b) { int pa = Find(a), pb = Find(b); if (pa == pb) return false; p[pb] = pa; return true; }
        }
    }
}