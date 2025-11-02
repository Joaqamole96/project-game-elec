// FloorGenerator_Orchestrator.cs
using System.Collections.Generic;
using UnityEngine;
using FGA.Models;
using FGA.Generation;
using FGA.Layout;
using FGA.Presentation;

namespace FGA.Core
{
    [RequireComponent(typeof(FloorController))]
    public class FloorGenerator_Orchestrator : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 80;
        public int height = 60;
        [Tooltip("0 => random seed")]
        public int seed = 0;

        [Header("Partition")]
        public int minPartitionSize = 6;
        public int maxDepth = 5;

        [Header("Graph")]
        [Range(0f, 0.5f)]
        public float extraLoopChance = 0.2f;

        [Header("Baker")]
        public bool fillOuterWalls = true;

        [HideInInspector]
        public FloorModel model;

        public void Generate()
        {
            int usedSeed = seed == 0 ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
            model = new FloorModel(width, height, usedSeed);
            model.ClearContent();

            // 1) partition
            var partGen = new PartitionGenerator(usedSeed, minPartitionSize, maxDepth);
            var parts = partGen.Generate(width, height);
            model.partitions.AddRange(parts);

            // 2) place rooms inside partitions (inset by 1..3)
            var rng = new System.Random(usedSeed);
            model.rooms.Clear();
            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                int inset = Mathf.Clamp(1 + (int)(rng.NextDouble() * 2), 1, 3);
                int rx = p.xMin + inset;
                int ry = p.yMin + inset;
                int rw = Mathf.Max(2, p.width - inset * 2);
                int rh = Mathf.Max(2, p.height - inset * 2);
                RectInt rr = new RectInt(rx, ry, rw, rh);
                var rm = new RoomModel(rr);
                model.rooms.Add(rm);
                p = rr; // keep original partition list separate; rooms can be smaller
            }

            // 3) build room graph
            var graphBuilder = new RoomGraphBuilder(usedSeed, extraLoopChance);
            var connections = graphBuilder.BuildGraph(parts);

            // 4) carve corridors (CorridorSystem expects partitions list to clamp doors)
            CorridorSystem.BuildCorridors(model, connections, parts, true);

            // 5) assign room graph metadata (BFS main path, class types, spawn keys)
            // quick BFS + mainpath: build adjacency map
            var roomAdj = BuildAdjacency(model.rooms.Count, model.corridors);
            AssignMainPathAndTypes(model, roomAdj, usedSeed);

            // 6) lock some special rooms & place keys (simple: lock treasure/shop rooms)
            AssignLocksAndKeys(model, usedSeed, 1); // lockCount = 1 by default

            // 7) bake tilemap
            TileMapBaker.Bake(model, fillOuterWalls);

            Debug.Log($"[Orchestrator] Generated floor seed={usedSeed} rooms={model.rooms.Count} corridors={model.corridors.Count}");
        }

        // Builds adjacency list from corridor models (room indices)
        Dictionary<int, List<int>> BuildAdjacency(int roomCount, List<CorridorModel> corridors)
        {
            var dict = new Dictionary<int, List<int>>();
            for (int i = 0; i < roomCount; i++) dict[i] = new List<int>();
            foreach (var c in corridors)
            {
                if (c.roomA >= 0 && c.roomB >= 0 && c.roomA < roomCount && c.roomB < roomCount)
                {
                    dict[c.roomA].Add(c.roomB);
                    dict[c.roomB].Add(c.roomA);
                }
            }
            return dict;
        }

        void AssignMainPathAndTypes(FloorModel model, Dictionary<int, List<int>> adj, int usedSeed)
        {
            if (model.rooms.Count == 0) return;
            // pick start = room with smallest (x+y)
            int start = 0;
            float best = float.MaxValue;
            for (int i = 0; i < model.rooms.Count; i++)
            {
                var r = model.rooms[i].rect;
                float v = r.xMin + r.yMin;
                if (v < best) { best = v; start = i; }
            }

            // BFS to compute distances
            var q = new Queue<int>();
            var depth = new int[model.rooms.Count];
            for (int i = 0; i < depth.Length; i++) depth[i] = -1;
            depth[start] = 0; q.Enqueue(start);
            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                foreach (var n in adj[cur])
                {
                    if (depth[n] == -1) { depth[n] = depth[cur] + 1; q.Enqueue(n); }
                }
            }

            // farthest = boss
            int boss = 0; int maxd = -1;
            for (int i = 0; i < depth.Length; i++) if (depth[i] > maxd) { maxd = depth[i]; boss = i; }

            // mark main path by backtracking from boss to start using parent map
            var parent = BuildParentMap(start, adj);
            var curNode = boss;
            var onPath = new bool[model.rooms.Count];
            while (curNode != -1)
            {
                onPath[curNode] = true;
                if (curNode == start) break;
                parent.TryGetValue(curNode, out int p); curNode = p;
            }

            // apply types
            for (int i = 0; i < model.rooms.Count; i++)
            {
                var rm = model.rooms[i];
                if (i == start) rm.type = RoomType.Start;
                else if (i == boss) rm.type = RoomType.Boss;
                else if (onPath[i]) rm.type = RoomType.MainPath;
                else rm.type = RoomType.Side;
                rm.onMainPath = onPath[i];
                rm.depth = depth[i];
            }

            Debug.Log($"[Orchestrator] Start room {start} Boss room {boss}");
        }

        Dictionary<int, int> BuildParentMap(int root, Dictionary<int, List<int>> adj)
        {
            var parent = new Dictionary<int, int>();
            var q = new Queue<int>();
            parent[root] = -1; q.Enqueue(root);
            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                foreach (var n in adj[cur])
                    if (!parent.ContainsKey(n)) { parent[n] = cur; q.Enqueue(n); }
            }
            return parent;
        }

        void AssignLocksAndKeys(FloorModel model, int usedSeed, int lockCount)
        {
            model.lockedDoorTiles.Clear(); model.keyTiles.Clear();
            var specials = new List<int>();
            for (int i = 0; i < model.rooms.Count; i++)
                if (model.rooms[i].type == RoomType.Treasure || model.rooms[i].type == RoomType.Shop) specials.Add(i);

            // fallback: pick deepest side rooms
            if (specials.Count == 0)
            {
                var list = new List<int>();
                for (int i = 0; i < model.rooms.Count; i++) if (!model.rooms[i].onMainPath) list.Add(i);
                list.Sort((a, b) => model.rooms[b].depth.CompareTo(model.rooms[a].depth));
                for (int i = 0; i < Mathf.Min(lockCount, list.Count); i++) specials.Add(list[i]);
            }

            var rng = new System.Random(usedSeed);
            int locksToMake = Mathf.Min(lockCount, specials.Count);
            var chosen = new List<int>();
            for (int i = 0; i < locksToMake; i++) chosen.Add(specials[rng.Next(specials.Count)]);

            foreach (var target in chosen)
            {
                // find corridor connecting target to parent on path using corridor room links
                var parentRoomIndex = FindParentOnPath(target, model);
                if (parentRoomIndex == -1) continue;

                // find corridor linking them
                foreach (var c in model.corridors)
                {
                    if ((c.roomA == target && c.roomB == parentRoomIndex) || (c.roomB == target && c.roomA == parentRoomIndex))
                    {
                        // lock the door tile closest to target
                        Vector2Int lockPos = FindClosestTileToRoom(c.tiles, model.rooms[target].rect);
                        model.lockedDoorTiles.Add(lockPos);
                        // find key room: choose a room earlier on path (depth < target.depth)
                        int keyRoom = PickKeyRoom(model, target, rng);
                        if (keyRoom != -1)
                        {
                            Vector2Int kp = new Vector2Int((int)model.rooms[keyRoom].rect.center.x, (int)model.rooms[keyRoom].rect.center.y);
                            model.keyTiles.Add(kp);
                            model.rooms[keyRoom].hasKey = true;
                        }
                        break;
                    }
                }
            }
        }

        int FindParentOnPath(int node, FloorModel model)
        {
            // look for neighbouring room with depth < node.depth
            int best = -1; int bestd = int.MaxValue;
            for (int i = 0; i < model.rooms.Count; i++)
            {
                if (i == node) continue;
                if (model.rooms[i].depth < model.rooms[node].depth && model.rooms[i].onMainPath)
                {
                    if (model.rooms[node].depth - model.rooms[i].depth < bestd)
                    { bestd = model.rooms[node].depth - model.rooms[i].depth; best = i; }
                }
            }
            return best;
        }

        int PickKeyRoom(FloorModel model, int lockedRoom, System.Random rng)
        {
            // choose a room with smaller depth than lockedRoom and not the start
            var candidates = new List<int>();
            for (int i = 0; i < model.rooms.Count; i++)
                if (model.rooms[i].depth >= 0 && model.rooms[i].depth < model.rooms[lockedRoom].depth && i != 0)
                    candidates.Add(i);
            if (candidates.Count == 0) return -1;
            return candidates[rng.Next(candidates.Count)];
        }

        Vector2Int FindClosestTileToRoom(List<Vector2Int> tiles, RectInt rect)
        {
            Vector2Int best = tiles[0];
            float bestd = float.MaxValue;
            foreach (var t in tiles)
            {
                var center = new Vector2(rect.center.x, rect.center.y);
                float d = (new Vector2(t.x, t.y) - center).sqrMagnitude;
                if (d < bestd) { bestd = d; best = t; }
            }
            return best;
        }
    }
}