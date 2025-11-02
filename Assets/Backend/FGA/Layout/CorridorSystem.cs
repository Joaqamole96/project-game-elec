// CorridorSystem.cs
using System.Collections.Generic;
using UnityEngine;
using FGA.Models;

namespace FGA.Layout
{
    public static class CorridorSystem
    {
        // connections: use RoomConnection list (A,B,doorPos from RoomGraphBuilder)
        // rooms: list of RoomModel; we produce CorridorModel entries and populate model.doorTiles
        public static void BuildCorridors(FloorModel model, List<FGA.Generation.RoomConnection> connections, List<RectInt> partitions, bool horizFirstRandom = true)
        {
            if (model == null || connections == null) return;
            model.corridors.Clear();
            model.doorTiles.Clear();

            var rng = new System.Random(model.seed);
            int w = model.width, h = model.height;

            foreach (var conn in connections)
            {
                // Find door tile on each room: clamp door coords to each room boundary
                var partA = partitions[conn.A]; var partB = partitions[conn.B];
                Vector2Int doorA = ClampToPartitionWall(partA, partB);
                Vector2Int doorB = ClampToPartitionWall(partB, partA);

                bool horizFirst = horizFirstRandom ? (rng.NextDouble() > 0.5) : true;
                var tiles = new HashSet<Vector2Int>();

                if (horizFirst)
                {
                    CarveHorizontalSegment(tiles, doorA.x, doorB.x, doorA.y, w, h);
                    CarveVerticalSegment(tiles, doorA.y, doorB.y, doorB.x, w, h);
                }
                else
                {
                    CarveVerticalSegment(tiles, doorA.y, doorB.y, doorA.x, w, h);
                    CarveHorizontalSegment(tiles, doorA.x, doorB.x, doorB.y, w, h);
                }

                // register doors (closest tile within each room)
                if (InBounds(doorA, w, h)) { tiles.Add(doorA); model.doorTiles.Add(doorA); }
                if (InBounds(doorB, w, h)) { tiles.Add(doorB); model.doorTiles.Add(doorB); }

                var path = new List<Vector2Int>(tiles);
                var corridor = new CorridorModel(path, conn.A, conn.B);
                model.corridors.Add(corridor);
            }
        }

        static void CarveHorizontalSegment(HashSet<Vector2Int> outTiles, int x0, int x1, int y, int w, int h)
        {
            int sx = Mathf.Min(x0, x1), ex = Mathf.Max(x0, x1);
            for (int x = sx; x <= ex; x++)
            {
                Add(outTiles, new Vector2Int(x, y), w, h);
                Add(outTiles, new Vector2Int(x, y + 1), w, h); // two-tile width vertical expansion
            }
        }

        static void CarveVerticalSegment(HashSet<Vector2Int> outTiles, int y0, int y1, int x, int w, int h)
        {
            int sy = Mathf.Min(y0, y1), ey = Mathf.Max(y0, y1);
            for (int y = sy; y <= ey; y++)
            {
                Add(outTiles, new Vector2Int(x, y), w, h);
                Add(outTiles, new Vector2Int(x + 1, y), w, h); // two-tile width horizontal expansion
            }
        }

        static void Add(HashSet<Vector2Int> s, Vector2Int p, int w, int h) { if (InBounds(p, w, h)) s.Add(p); }
        static bool InBounds(Vector2Int p, int w, int h) => p.x >= 0 && p.y >= 0 && p.x < w && p.y < h;

        static Vector2Int ClampToPartitionWall(RectInt p, RectInt other)
        {
            // prefer shared boundary; fallback to center
            if (p.xMax == other.xMin)
            {
                int yMin = Mathf.Max(p.yMin, other.yMin);
                int yMax = Mathf.Min(p.yMax - 1, other.yMax - 1);
                int yy = (yMin <= yMax) ? (yMin + yMax) / 2 : p.yMin + p.height / 2;
                return new Vector2Int(p.xMax - 1, yy);
            }
            if (other.xMax == p.xMin)
            {
                int yMin = Mathf.Max(p.yMin, other.yMin);
                int yMax = Mathf.Min(p.yMax - 1, other.yMax - 1);
                int yy = (yMin <= yMax) ? (yMin + yMax) / 2 : p.yMin + p.height / 2;
                return new Vector2Int(p.xMin, yy);
            }
            if (p.yMax == other.yMin)
            {
                int xMin = Mathf.Max(p.xMin, other.xMin);
                int xMax = Mathf.Min(p.xMax - 1, other.xMax - 1);
                int xx = (xMin <= xMax) ? (xMin + xMax) / 2 : p.xMin + p.width / 2;
                return new Vector2Int(xx, p.yMax - 1);
            }
            if (other.yMax == p.yMin)
            {
                int xMin = Mathf.Max(p.xMin, other.xMin);
                int xMax = Mathf.Min(p.xMax - 1, other.xMax - 1);
                int xx = (xMin <= xMax) ? (xMin + xMax) / 2 : p.xMin + p.width / 2;
                return new Vector2Int(xx, p.yMin);
            }
            return new Vector2Int(p.x + p.width / 2, p.y + p.height / 2);
        }
    }
}