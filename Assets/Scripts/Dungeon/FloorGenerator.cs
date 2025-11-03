using UnityEngine;
using System.Collections.Generic;

// FloorGenerator: adjacency-first corridors (based on your working "snaking" version)
public class FloorGenerator : MonoBehaviour
{
    [Header("Floor Settings")]
    public int Width = 100;
    public int Height = 100;
    public int Seed = 0;
    [Tooltip("Partitions smaller than this won't split.")] public int minPartitionSize = 20;

    [Header("Room Insets")]
    public int MinInset = 1;
    public int MaxInset = 3;

    [Header("MST / Graph")]
    [Tooltip("Extra random connections in addition to MST")]
    public int extraConnections = 3;

    // Core data
    private Partition root;
    private List<Partition> leaves = new();
    private List<Room> rooms = new();
    private List<Corridor> corridors = new();
    private List<Vector2Int> doors = new();

    // Occupancy grid (true = walkable)
    private bool[,] walkable;

    void Start() => Generate();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Random.InitState(Seed);

        // reset
        leaves.Clear(); rooms.Clear(); corridors.Clear(); doors.Clear();
        root = new Partition(new RectInt(0, 0, Width, Height));

        // BSP -> rooms (same as Failsafe)
        Split(root);
        CreateRooms();

        // Build occupancy grid (rooms are obstacles)
        BuildWalkableGrid();

        // Spatial neighbor graph among leaves
        BuildNeighbors();

        // ADJACENCY-FIRST: build adjacency MST and create straight-edge corridors along shared boundaries
        BuildAdjacencyCorridors();

        // If adjacency couldn't fully connect (rare), fall back to MST over neighbors using BFS connectors
        ConnectNeighborsMST_Fallback();

        Debug.Log($"Generate complete: leaves={leaves.Count} rooms={rooms.Count} corridors={corridors.Count}");
    }

    // ------------------- BSP (unchanged logic) -------------------
    private void Split(Partition p)
    {
        if (p.rect.width <= minPartitionSize || p.rect.height <= minPartitionSize)
        {
            leaves.Add(p);
            return;
        }

        bool vert = (p.rect.width > p.rect.height);
        float ratio = Random.Range(.45f, .55f);

        if (vert)
        {
            int split = Mathf.RoundToInt(p.rect.width * ratio);
            p.left = new(new RectInt(p.rect.x, p.rect.y, split, p.rect.height));
            p.right = new(new RectInt(p.rect.x + split, p.rect.y, p.rect.width - split, p.rect.height));
        }
        else
        {
            int split = Mathf.RoundToInt(p.rect.height * ratio);
            p.left = new(new RectInt(p.rect.x, p.rect.y, p.rect.width, split));
            p.right = new(new RectInt(p.rect.x, p.rect.y + split, p.rect.width, p.rect.height - split));
        }

        Split(p.left); Split(p.right);
    }

    private void CreateRooms()
    {
        rooms.Clear();
        foreach (var p in leaves)
        {
            int l = Random.Range(MinInset, MaxInset + 1);
            int r = Random.Range(MinInset, MaxInset + 1);
            int b = Random.Range(MinInset, MaxInset + 1);
            int t = Random.Range(MinInset, MaxInset + 1);

            RectInt rr = new RectInt(
                p.rect.x + l,
                p.rect.y + b,
                Mathf.Max(1, p.rect.width - (l + r)),
                Mathf.Max(1, p.rect.height - (b + t))
            );

            var room = new Room(rr);
            p.room = room;
            rooms.Add(room);
        }
    }

    // ------------------- Spatial neighbor graph -------------------
    private void BuildNeighbors()
    {
        if (leaves == null || leaves.Count == 0) return;

        // init/clear neighbor lists
        foreach (var p in leaves)
        {
            if (p.neighbors == null) p.neighbors = new List<Partition>();
            else p.neighbors.Clear();
        }

        for (int i = 0; i < leaves.Count; i++)
        {
            var a = leaves[i];
            if (a == null) continue;
            for (int j = i + 1; j < leaves.Count; j++)
            {
                var b = leaves[j];
                if (b == null) continue;

                if (AreNeighbors(a.rect, b.rect))
                {
                    a.neighbors.Add(b);
                    b.neighbors.Add(a);
                }
            }
        }
    }

    bool AreNeighbors(RectInt a, RectInt b)
    {
        bool xTouch = a.xMax == b.xMin || b.xMax == a.xMin;
        bool yTouch = a.yMax == b.yMin || b.yMax == a.yMin;

        bool overlapX = a.xMin < b.xMax && b.xMin < a.xMax;
        bool overlapY = a.yMin < b.yMax && b.yMin < a.yMax;

        return (xTouch && overlapY) || (yTouch && overlapX);
    }

    // ------------------- ADJACENCY-FIRST corridor generation -------------------
    private void BuildAdjacencyCorridors()
    {
        // collect unique neighbor pairs
        var pairs = new List<(Partition, Partition)>();
        var seen = new HashSet<(Partition, Partition)>();

        foreach (var a in leaves)
        {
            if (a == null || a.neighbors == null) continue;
            foreach (var b in a.neighbors)
            {
                if (b == null) continue;
                // canonical ordering to avoid duplicates
                var key = (a.GetHashCode() < b.GetHashCode()) ? (a, b) : (b, a);
                if (seen.Contains(key)) continue;
                seen.Add(key);
                pairs.Add(key);
            }
        }

        // Build a simple weighted graph (nodes = leaves). We'll run Prim's MST below using distances.
        // Create adjacency lookup for quick neighbor access
        var adjacency = new Dictionary<Partition, List<Partition>>();
        foreach (var l in leaves) adjacency[l] = new List<Partition>(l.neighbors ?? new List<Partition>());

        // Prim-like MST
        var connected = new HashSet<Partition>();
        List<(Partition A, Partition B)> mstEdges = new List<(Partition A, Partition B)>();
        if (leaves.Count > 0)
        {
            Partition start = leaves[0];
            connected.Add(start);

            while (connected.Count < leaves.Count)
            {
                float best = float.MaxValue;
                Partition bestA = null, bestB = null;

                foreach (var a in connected)
                {
                    foreach (var b in adjacency[a])
                    {
                        if (connected.Contains(b)) continue;
                        if (a.room == null || b.room == null) continue;
                        float d = Vector2.Distance(a.room.rect.center, b.room.rect.center);
                        if (d < best)
                        {
                            best = d;
                            bestA = a;
                            bestB = b;
                        }
                    }
                }

                if (bestA == null || bestB == null) break;
                connected.Add(bestB);
                mstEdges.Add((bestA, bestB));
            }
        }

        // Commit MST edges as direct boundary corridors
        var createdEdges = new HashSet<string>(); // simple string key to avoid dups
        foreach (var e in mstEdges)
        {
            CreateBoundaryCorridor(e.A, e.B, createdEdges);
        }

        // Add some extra adjacency edges (loops) for flavor
        int added = 0, tries = 0;
        while (added < extraConnections && tries < pairs.Count * 3)
        {
            tries++;
            var pick = pairs[Random.Range(0, pairs.Count)];
            // skip if already created
            string k = MakeEdgeKey(pick.Item1, pick.Item2);
            if (createdEdges.Contains(k)) continue;
            CreateBoundaryCorridor(pick.Item1, pick.Item2, createdEdges);
            added++;
        }
    }

    // build a canonical string key for a partition pair
    private string MakeEdgeKey(Partition a, Partition b)
    {
        // Use instance ids (hashcodes) sorted to canonicalize
        int h1 = a.GetHashCode(), h2 = b.GetHashCode();
        if (h1 < h2) return h1 + "_" + h2;
        return h2 + "_" + h1;
    }

    // Create a corridor along the exact shared boundary between two touching partitions.
    // If there's no clean shared boundary or door placement fails, returns false.
    private bool CreateBoundaryCorridor(Partition A, Partition B, HashSet<string> createdEdges)
    {
        if (A == null || B == null) return false;
        if (A.room == null || B.room == null) return false;

        // Ensure they're neighbors
        if (!AreNeighbors(A.rect, B.rect)) return false;

        // canonical key to avoid duplicates
        string key = MakeEdgeKey(A, B);
        if (createdEdges.Contains(key)) return false;

        // Determine if vertical or horizontal neighbor
        bool verticalBoundary = (A.rect.xMax == B.rect.xMin) || (B.rect.xMax == A.rect.xMin);
        bool horizontalBoundary = (A.rect.yMax == B.rect.yMin) || (B.rect.yMax == A.rect.yMin);

        // compute outside door positions (one tile outside each room's interior)
        Vector2Int doorA, doorB;

        if (verticalBoundary)
        {
            bool aIsLeft = A.rect.xMax == B.rect.xMin;
            int spanMinY = Mathf.Max(A.rect.yMin, B.rect.yMin);
            int spanMaxY = Mathf.Min(A.rect.yMax - 1, B.rect.yMax - 1);
            if (spanMinY > spanMaxY) return false;

            // Prefer Y overlap of the rooms; fallback to A.room center clamped
            int roomOverlapMinY = Mathf.Max(A.room.rect.yMin, B.room.rect.yMin);
            int roomOverlapMaxY = Mathf.Min(A.room.rect.yMax - 1, B.room.rect.yMax - 1);

            int pickY;
            if (roomOverlapMinY <= roomOverlapMaxY && roomOverlapMinY >= spanMinY && roomOverlapMaxY <= spanMaxY)
                pickY = Random.Range(roomOverlapMinY, roomOverlapMaxY + 1);
            else
            {
                int centerY = Mathf.RoundToInt(A.room.rect.center.y);
                pickY = Mathf.Clamp(centerY, spanMinY, spanMaxY);
            }

            // Outside door: one tile outside the room interior toward the neighbor
            doorA = aIsLeft ? new Vector2Int(A.room.rect.xMax, pickY) : new Vector2Int(A.room.rect.xMin - 1, pickY);
            doorB = aIsLeft ? new Vector2Int(B.room.rect.xMin - 1, pickY) : new Vector2Int(B.room.rect.xMax, pickY);
        }
        else if (horizontalBoundary)
        {
            bool aIsBelow = A.rect.yMax == B.rect.yMin;
            int spanMinX = Mathf.Max(A.rect.xMin, B.rect.xMin);
            int spanMaxX = Mathf.Min(A.rect.xMax - 1, B.rect.xMax - 1);
            if (spanMinX > spanMaxX) return false;

            int roomOverlapMinX = Mathf.Max(A.room.rect.xMin, B.room.rect.xMin);
            int roomOverlapMaxX = Mathf.Min(A.room.rect.xMax - 1, B.room.rect.xMax - 1);

            int pickX;
            if (roomOverlapMinX <= roomOverlapMaxX && roomOverlapMinX >= spanMinX && roomOverlapMaxX <= spanMaxX)
                pickX = Random.Range(roomOverlapMinX, roomOverlapMaxX + 1);
            else
            {
                int centerX = Mathf.RoundToInt(A.room.rect.center.x);
                pickX = Mathf.Clamp(centerX, spanMinX, spanMaxX);
            }

            // Outside door: one tile outside the room interior toward the neighbor
            doorA = aIsBelow ? new Vector2Int(pickX, A.room.rect.yMax) : new Vector2Int(pickX, A.room.rect.yMin - 1);
            doorB = aIsBelow ? new Vector2Int(pickX, B.room.rect.yMin - 1) : new Vector2Int(pickX, B.room.rect.yMax);
        }
        else
        {
            return false;
        }

        // Ensure doors are in bounds and not inside some other room; if they are, nudge them outward
        doorA = EnsureDoorIsOutsideAndInBounds(doorA, A.room);
        doorB = EnsureDoorIsOutsideAndInBounds(doorB, B.room);

        // Now build straight corridor tiles between doorA and doorB (axis-aligned)
        var tiles = new List<Vector2Int>();
        if (doorA.x == doorB.x) // vertical
        {
            int yStart = Mathf.Min(doorA.y, doorB.y);
            int yEnd = Mathf.Max(doorA.y, doorB.y);

            for (int y = yStart; y <= yEnd; y++)
            {
                var t = new Vector2Int(doorA.x, y);
                // avoid carving inside either room interior
                if (IsPointInsideAnyRoom(t, A.room) || IsPointInsideAnyRoom(t, B.room)) continue;
                if (CorridorTileExists(t)) continue;
                tiles.Add(t);
            }
        }
        else if (doorA.y == doorB.y) // horizontal
        {
            int xStart = Mathf.Min(doorA.x, doorB.x);
            int xEnd = Mathf.Max(doorA.x, doorB.x);

            for (int x = xStart; x <= xEnd; x++)
            {
                var t = new Vector2Int(x, doorA.y);
                if (IsPointInsideAnyRoom(t, A.room) || IsPointInsideAnyRoom(t, B.room)) continue;
                if (CorridorTileExists(t)) continue;
                tiles.Add(t);
            }
        }
        else
        {
            // Not axis-aligned — shouldn't happen for touching partitions, fallback
            return false;
        }

        // If we failed to generate any corridor tiles, try fallback
        if (tiles.Count == 0)
        {
            var fallback = GuardedLPath(doorA, doorB, A.room, B.room);
            if (fallback != null && fallback.Count > 0)
            {
                // Snap doors to nearest corridor tiles to guarantee doors are corridor tiles
                var snappedA = SnapDoorToNearestTile(doorA, fallback);
                var snappedB = SnapDoorToNearestTile(doorB, fallback);

                doors.Add(snappedA);
                doors.Add(snappedB);
                corridors.Add(new Corridor(fallback));
                createdEdges.Add(key);
                return true;
            }
            return false;
        }

        // Ensure doors land on corridor tiles; if not, snap them to the nearest corridor tile
        var finalDoorA = tiles.Contains(doorA) ? doorA : SnapDoorToNearestTile(doorA, tiles);
        var finalDoorB = tiles.Contains(doorB) ? doorB : SnapDoorToNearestTile(doorB, tiles);

        doors.Add(finalDoorA);
        doors.Add(finalDoorB);
        corridors.Add(new Corridor(tiles));
        createdEdges.Add(key);
        return true;
    }

    // returns true if any existing corridor already contains that tile
    private bool CorridorTileExists(Vector2Int t)
    {
        foreach (var c in corridors)
        {
            if (c?.tiles == null) continue;
            foreach (var q in c.tiles) if (q == t) return true;
        }
        return false;
    }

    // ------------------- Grid & BFS pathfinding (kept as fallback) -------------------
    private void BuildWalkableGrid()
    {
        walkable = new bool[Width, Height];

        // default all walkable
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                walkable[x, y] = true;

        // mark room interior as blocked (not walkable)
        foreach (var r in rooms)
        {
            for (int x = r.rect.xMin; x < r.rect.xMax; x++)
                for (int y = r.rect.yMin; y < r.rect.yMax; y++)
                {
                    if (InBounds(x, y))
                        walkable[x, y] = false;
                }
        }

        // allow corridors to be walkable if desired (they will be added later)
    }

    private bool InBounds(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;

    // BFS grid path (returns path including start and goal) — avoids blocked room tiles
    private List<Vector2Int> FindPathBFS(Vector2Int start, Vector2Int goal)
    {
        var path = new List<Vector2Int>();
        if (!InBounds(start.x, start.y) || !InBounds(goal.x, goal.y)) return path;

        // visited & parent
        bool[,] visited = new bool[Width, Height];
        Vector2Int[,] parent = new Vector2Int[Width, Height];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(start);
        visited[start.x, start.y] = true;
        parent[start.x, start.y] = new Vector2Int(-1, -1);

        int[] dx = new int[] { 1, -1, 0, 0 };
        int[] dy = new int[] { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();

            if (cur == goal)
            {
                // reconstruct
                Vector2Int p = cur;
                while (p.x != -1)
                {
                    path.Add(p);
                    p = parent[p.x, p.y];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = cur.x + dx[i];
                int ny = cur.y + dy[i];
                if (!InBounds(nx, ny)) continue;
                if (visited[nx, ny]) continue;

                // allow stepping into blocked only if it's the goal (so endpoints inside rooms ok)
                if (!walkable[nx, ny] && !(nx == goal.x && ny == goal.y)) continue;

                visited[nx, ny] = true;
                parent[nx, ny] = cur;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        // no path found
        return path;
    }

    // ------------------- Door selection & safe connection (fallback) -------------------
    // Connect two rooms by selecting perimeter-adjacent door positions and running BFS
    private void SafeConnectRooms(Room a, Room b)
    {
        // compute candidate outside-perimeter points for A and B
        Vector2Int doorA = ComputeExteriorDoorPoint(a.rect, b.rect);
        Vector2Int doorB = ComputeExteriorDoorPoint(b.rect, a.rect);

        // if chosen points end up inside other rooms or out of bounds, nudge along shared boundary or fallback later
        if (!InBounds(doorA.x, doorA.y) || !InBounds(doorB.x, doorB.y))
        {
            // fallback to center-based L-shape (will be guarded later)
            AddFallbackCorridor(a, b);
            return;
        }

        // attempt BFS avoiding rooms
        var path = FindPathBFS(doorA, doorB);

        if (path != null && path.Count > 0)
        {
            // commit doors & corridor
            doors.Add(doorA); doors.Add(doorB);
            corridors.Add(new Corridor(path));
        }
        else
        {
            // fallback: try a guarded L-path that refuses to place tiles inside rooms
            AddFallbackCorridor(a, b);
        }
    }

    // Pick a point on room perimeter facing the other rect, then step one tile outward toward the target rect.
    private Vector2Int ComputeExteriorDoorPoint(RectInt from, RectInt toward)
    {
        // find the nearest perimeter position on 'from' to the center of 'toward'
        Vector2 targetCenter = new Vector2(toward.x + toward.width / 2.0f, toward.y + toward.height / 2.0f);

        // clamp target center to the perimeter of 'from' to get a perimeter point
        float cx = Mathf.Clamp(targetCenter.x, from.xMin, from.xMax - 1);
        float cy = Mathf.Clamp(targetCenter.y, from.yMin, from.yMax - 1);

        // nearest integer perimeter candidate (clamped)
        int px = Mathf.RoundToInt(cx);
        int py = Mathf.RoundToInt(cy);

        // If that candidate is inside the room interior, push it to the nearest edge
        if (from.Contains(new Vector2Int(px, py)))
        {
            // push px/py to the edge: choose whichever axis is closer to outside
            int leftDist = Mathf.Abs(px - from.xMin);
            int rightDist = Mathf.Abs((from.xMax - 1) - px);
            int downDist = Mathf.Abs(py - from.yMin);
            int upDist = Mathf.Abs((from.yMax - 1) - py);

            int min = Mathf.Min(Mathf.Min(leftDist, rightDist), Mathf.Min(downDist, upDist));

            if (min == leftDist) px = from.xMin - 1;
            else if (min == rightDist) px = from.xMax; // one tile outside right edge
            else if (min == downDist) py = from.yMin - 1;
            else py = from.yMax; // outside top edge
        }

        // Now we have a candidate, but we need it to be outside the room interior.
        // If it's still inside a room (edge case due to rounding), step outward along vector from from.center to toward.center
        Vector2Int candidate = new Vector2Int(px, py);
        if (IsPointInsideAnyRoom(candidate, from)) // if it's inside 'from' (rare)
        {
            // direction away from room center toward target
            Vector2 dir = (targetCenter - new Vector2(from.center.x, from.center.y)).normalized;
            Vector2Int step = new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(Mathf.Sign(dir.x)), -1, 1),
                                             Mathf.Clamp(Mathf.RoundToInt(Mathf.Sign(dir.y)), -1, 1));
            candidate += step;

            // clamp to bounds
            candidate.x = Mathf.Clamp(candidate.x, 0, Width - 1);
            candidate.y = Mathf.Clamp(candidate.y, 0, Height - 1);
        }

        // If candidate lies inside any other room, try sliding along perimeter outward until free
        if (IsPointInsideAnyRoom(candidate, from) || !InBounds(candidate.x, candidate.y))
        {
            // search around perimeter ring up to radius 3 for available tile
            for (int r = 1; r <= 3; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                        var t = new Vector2Int(candidate.x + dx, candidate.y + dy);
                        if (!InBounds(t.x, t.y)) continue;
                        if (!IsPointInsideAnyRoom(t, (Room)null)) // explicit overload
                        {
                            return t;
                        }
                    }
            }
        }

        return candidate;
    }

    // Check whether pt is inside any room (optionally ignoring a room 'ignoreFrom' which is allowed)
    private bool IsPointInsideAnyRoom(Vector2Int pt, RectInt? allowInsideRoom)
    {
        foreach (var r in rooms)
        {
            if (allowInsideRoom.HasValue && r.rect.Equals(allowInsideRoom.Value)) continue;
            if (r.rect.Contains(pt)) return true;
        }
        return false;
    }

    // overload: ignore a specific RectInt room
    private bool IsPointInsideAnyRoom(Vector2Int pt, RectInt allowToIgnore)
    {
        foreach (var r in rooms)
        {
            if (r.rect.Equals(allowToIgnore)) continue;
            if (r.rect.Contains(pt)) return true;
        }
        return false;
    }

    private bool IsPointInsideAnyRoom(Vector2Int pt, Room ignoreRoom)
    {
        foreach (var r in rooms)
        {
            if (ignoreRoom != null && r == ignoreRoom) continue;
            if (r.rect.Contains(pt)) return true;
        }
        return false;
    }

    // If adjacency can't create tiles, do a guarded L-path: skip tiles that would lie inside rooms.
    private void AddFallbackCorridor(Room a, Room b)
    {
        var ar = a.rect;
        var br = b.rect;
        Vector2Int dA, dB;

        if (ar.xMax <= br.xMin && OverlapRange(ar.yMin, ar.yMax, br.yMin, br.yMax, out int y))
        {
            dA = new Vector2Int(ar.xMax - 1, y); dB = new Vector2Int(br.xMin, y);
        }
        else if (br.xMax <= ar.xMin && OverlapRange(ar.yMin, ar.yMax, br.yMin, br.yMax, out int y2))
        {
            dA = new Vector2Int(ar.xMin, y2); dB = new Vector2Int(br.xMax - 1, y2);
        }
        else if (ar.yMax <= br.yMin && OverlapRange(ar.xMin, ar.xMax, br.xMin, br.xMax, out int x))
        {
            dA = new Vector2Int(x, ar.yMax - 1); dB = new Vector2Int(x, br.yMin);
        }
        else if (br.yMax <= ar.yMin && OverlapRange(ar.xMin, ar.xMax, br.xMin, br.xMax, out int x2))
        {
            dA = new Vector2Int(x2, ar.yMin); dB = new Vector2Int(x2, br.yMax - 1);
        }
        else
        {
            dA = Vector2Int.RoundToInt(ar.center); dB = Vector2Int.RoundToInt(br.center);
        }

        // create guarded path from dA to dB (no tile inside rooms)
        var p = GuardedLPath(dA, dB, a, b);
        if (p != null && p.Count > 0)
        {
            doors.Add(dA); doors.Add(dB);
            corridors.Add(new Corridor(p));
        }
    }

    private List<Vector2Int> GuardedLPath(Vector2Int a, Vector2Int b, Room A, Room B)
    {
        var p = new List<Vector2Int>();
        bool first = Random.value > .5f;

        if (first)
        {
            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x))
            {
                var t = new Vector2Int(x, a.y);
                if (!IsPointInsideAnyRoom(t, A) && !IsPointInsideAnyRoom(t, B)) p.Add(t);
            }

            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y))
            {
                var t = new Vector2Int(b.x, y);
                if (!IsPointInsideAnyRoom(t, A) && !IsPointInsideAnyRoom(t, B)) p.Add(t);
            }
        }
        else
        {
            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y))
            {
                var t = new Vector2Int(a.x, y);
                if (!IsPointInsideAnyRoom(t, A) && !IsPointInsideAnyRoom(t, B)) p.Add(t);
            }

            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x))
            {
                var t = new Vector2Int(x, b.y);
                if (!IsPointInsideAnyRoom(t, A) && !IsPointInsideAnyRoom(t, B)) p.Add(t);
            }
        }

        p.Add(b);
        return p;
    }

    // ------------------- MST over neighbors fallback (if adjacency didn't fully connect) -------------------
    // This runs SafeConnectRooms for any remaining disconnected leaves.
    private void ConnectNeighborsMST_Fallback()
    {
        if (leaves == null || leaves.Count == 0) return;

        // Build adjacency MST like before but ensure every node is connected using SafeConnectRooms (BFS fallback)
        HashSet<Partition> visited = new HashSet<Partition>();
        Partition start = leaves[Random.Range(0, leaves.Count)];
        visited.Add(start);

        while (visited.Count < leaves.Count)
        {
            float best = float.MaxValue;
            Partition bestA = null;
            Partition bestB = null;

            foreach (var v in visited)
            {
                if (v == null || v.neighbors == null) continue;
                foreach (var n in v.neighbors)
                {
                    if (n == null || visited.Contains(n)) continue;
                    if (v.room == null || n.room == null) continue;

                    float d = Vector2.Distance(v.room.rect.center, n.room.rect.center);
                    if (d < best)
                    {
                        best = d;
                        bestA = v;
                        bestB = n;
                    }
                }
            }

            if (bestA == null || bestB == null) break;

            // connect via BFS-capable SafeConnectRooms
            SafeConnectRooms(bestA.room, bestB.room);
            visited.Add(bestB);
        }
    }

    // expose a public call if adjacency fails to connect all components
    private void ConnectNeighborsMST_FallbackPublic()
    {
        ConnectNeighborsMST_Fallback();
    }

    // ------------------- Utility / helpers -------------------
    bool OverlapRange(int aMin, int aMax, int bMin, int bMax, out int v)
    {
        int min = Mathf.Max(aMin, bMin);
        int max = Mathf.Min(aMax - 1, bMax - 1);

        v = (max < min) ? ((aMin + aMax) / 2) : Random.Range(min, max + 1);

        return true;
    }

    List<Vector2Int> Path(Vector2Int a, Vector2Int b)
    {
        // Preserve the old L-path generator for fallback/compatibility
        List<Vector2Int> p = new List<Vector2Int>();
        bool first = Random.value > .5f;

        if (first)
        {
            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x))
                p.Add(new Vector2Int(x, a.y));

            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y))
                p.Add(new Vector2Int(b.x, y));
        }
        else
        {
            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y))
                p.Add(new Vector2Int(a.x, y));

            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x))
                p.Add(new Vector2Int(x, b.y));
        }

        p.Add(b);
        return p;
    }

    Room FindRoom(Partition p)
        => p?.room ?? FindRoom(p.left) ?? FindRoom(p.right);

    // Ensure a door point is outside the given room interior and inside bounds.
    // If door lies inside some room, nudge it outward away from that room center until free.
    private Vector2Int EnsureDoorIsOutsideAndInBounds(Vector2Int door, Room room)
    {
        // clamp to bounds first
        door.x = Mathf.Clamp(door.x, 0, Width - 1);
        door.y = Mathf.Clamp(door.y, 0, Height - 1);

        // If door is already not inside *any* room, we're good.
        if (!IsPointInsideAnyRoom(door, (Room)null)) return door;

        // If it's inside the target room specifically, push it one tile outward from the room center
        Vector2 dir = (new Vector2(door.x, door.y) - new Vector2(room.rect.center.x, room.rect.center.y)).normalized;
        Vector2Int step = new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(Mathf.Sign(dir.x)), -1, 1),
                                        Mathf.Clamp(Mathf.RoundToInt(Mathf.Sign(dir.y)), -1, 1));

        // Try stepping up to 4 tiles outward to find a free spot
        for (int i = 1; i <= 4; i++)
        {
            var candidate = new Vector2Int(door.x + step.x * i, door.y + step.y * i);
            if (!InBounds(candidate.x, candidate.y)) continue;
            if (!IsPointInsideAnyRoom(candidate, (Room)null)) return candidate;
        }

        // If that fails, search outward in a small ring for any free tile
        for (int r = 1; r <= 4; r++)
        {
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                    var candidate = new Vector2Int(door.x + dx, door.y + dy);
                    if (!InBounds(candidate.x, candidate.y)) continue;
                    if (!IsPointInsideAnyRoom(candidate, (Room)null)) return candidate;
                }
        }

        // As a final fallthrough, clamp to nearest in-bounds tile (may still be inside but unlikely)
        door.x = Mathf.Clamp(door.x, 0, Width - 1);
        door.y = Mathf.Clamp(door.y, 0, Height - 1);
        return door;
    }

    // If desired door is not included in tiles, snap it to the nearest tile in the list (by Manhattan distance)
    private Vector2Int SnapDoorToNearestTile(Vector2Int desired, List<Vector2Int> tiles)
    {
        if (tiles == null || tiles.Count == 0) return desired;
        int best = int.MaxValue;
        Vector2Int pick = tiles[0];
        foreach (var t in tiles)
        {
            int dist = Mathf.Abs(t.x - desired.x) + Mathf.Abs(t.y - desired.y);
            if (dist < best)
            {
                best = dist;
                pick = t;
            }
        }
        return pick;
    }

    // ------------------- Gizmos / debug drawing -------------------
    void OnDrawGizmos()
    {
        if (root == null) return;

        // Partitions (wire)
        Gizmos.color = new Color(1, 1, 1, .08f);
        DrawBounds(root);

        // Rooms
        Gizmos.color = Color.green;
        foreach (var r in rooms)
        {
            Gizmos.DrawWireCube(new Vector3(r.rect.center.x, 0, r.rect.center.y),
                                new Vector3(r.rect.width, .12f, r.rect.height));
        }

        // Corridors
        Gizmos.color = Color.white;
        foreach (var c in corridors)
        {
            if (c?.tiles == null) continue;
            foreach (var t in c.tiles)
                Gizmos.DrawCube(new Vector3(t.x + .5f, 0, t.y + .5f),
                                new Vector3(.9f, .05f, .9f));
        }

        // Doors
        Gizmos.color = new Color(.4f, .2f, 0);
        foreach (var d in doors)
            Gizmos.DrawCube(new Vector3(d.x + .5f, 0, d.y + .5f),
                            new Vector3(.7f, .1f, .7f));
    }

    void DrawBounds(Partition p)
    {
        if (p == null) return;

        Gizmos.DrawWireCube(new Vector3(p.rect.center.x, 0, p.rect.center.y),
                            new Vector3(p.rect.width, .03f, p.rect.height));

        if (p.left != null) DrawBounds(p.left);
        if (p.right != null) DrawBounds(p.right);
    }
}

// ------------------- Simple data classes -------------------
public class Partition
{
    public RectInt rect;
    public Partition left;
    public Partition right;
    public Room room;

    // spatial neighbors
    public List<Partition> neighbors;

    public Partition(RectInt r)
    {
        rect = r;
        neighbors = new List<Partition>();
    }
}

public class Room
{
    public RectInt rect;
    public Room(RectInt r) => rect = r;
}

public class Corridor
{
    public List<Vector2Int> tiles;
    public Corridor(List<Vector2Int> t) => tiles = t;
}