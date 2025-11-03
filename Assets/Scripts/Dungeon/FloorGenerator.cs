// FloorGenerator_v4.cs
// Phase 3 — Door-to-door corridors, wall-accurate endpoints, subtle split randomness, door gizmos (brown)

using UnityEngine;
using System.Collections.Generic;

public class FloorGenerator : MonoBehaviour
{
    [Header("Floor Settings")] public int width = 50, height = 50;
    [Tooltip("Partitions smaller than this won't split.")] public int minPartitionSize = 12;

    [Header("Room Insets")] public int minInset = 1, maxInset = 3;

    Partition root;
    List<Partition> leaves = new();
    List<Room> rooms = new();
    List<Corridor> corridors = new();
    List<Vector2Int> doors = new();

    void Start() => Generate();
    [ContextMenu("Generate")] void Generate()
    {
        leaves.Clear(); rooms.Clear(); corridors.Clear(); doors.Clear();
        root = new Partition(new RectInt(0, 0, width, height));
        Split(root);
        CreateRooms();
        Connect(root);
    }

    void Split(Partition p)
    {
        if (p.rect.width <= minPartitionSize || p.rect.height <= minPartitionSize)
        { leaves.Add(p); return; }

        bool vert = p.rect.width > p.rect.height;
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

    // TODO: Phase 3.5 room drift will be added here
    void CreateRooms()
    {
        foreach (var p in leaves)
        {
            int l = Random.Range(minInset, maxInset + 1), r = Random.Range(minInset, maxInset + 1);
            int b = Random.Range(minInset, maxInset + 1), t = Random.Range(minInset, maxInset + 1);
            RectInt rr = new(
                p.rect.x + l,
                p.rect.y + b,
                Mathf.Max(1, p.rect.width - (l + r)),
                Mathf.Max(1, p.rect.height - (b + t))
            );
            var room = new Room(rr); p.room = room; rooms.Add(room);
        }
    }

    void Connect(Partition p)
    {
        if (p?.left == null || p.right == null) return;

        var A = FindRoom(p.left); var B = FindRoom(p.right);
        if (A != null && B != null) MakeCorridor(A, B);

        Connect(p.left); Connect(p.right);
    }

    void MakeCorridor(Room a, Room b)
    {
        var ar = a.rect; var br = b.rect;
        Vector2Int dA, dB;

        if (ar.xMax <= br.xMin && OverlapRange(ar.yMin, ar.yMax, br.yMin, br.yMax, out int y))
        {
            dA = new(ar.xMax - 1, y); dB = new(br.xMin, y);
        }
        else if (br.xMax <= ar.xMin && OverlapRange(ar.yMin, ar.yMax, br.yMin, br.yMax, out int y2))
        {
            dA = new(ar.xMin, y2); dB = new(br.xMax - 1, y2);
        }
        else if (ar.yMax <= br.yMin && OverlapRange(ar.xMin, ar.xMax, br.xMin, br.xMax, out int x))
        {
            dA = new(x, ar.yMax - 1); dB = new(x, br.yMin);
        }
        else if (br.yMax <= ar.yMin && OverlapRange(ar.xMin, ar.xMax, br.xMin, br.xMax, out int x2))
        {
            dA = new(x2, ar.yMin); dB = new(x2, br.yMax - 1);
        }
        else
        {
            dA = Vector2Int.RoundToInt(ar.center); dB = Vector2Int.RoundToInt(br.center);
        }

        doors.Add(dA); doors.Add(dB);
        corridors.Add(new Corridor(Path(dA, dB)));
    }

    bool OverlapRange(int aMin, int aMax, int bMin, int bMax, out int v)
    {
        int min = Mathf.Max(aMin, bMin), max = Mathf.Min(aMax - 1, bMax - 1);
        v = (max < min) ? (aMin + aMax) / 2 : Random.Range(min, max + 1);
        return true;
    }

    List<Vector2Int> Path(Vector2Int a, Vector2Int b)
    {
        List<Vector2Int> p = new(); bool first = Random.value > .5f;
        if (first)
        {
            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x)) p.Add(new(x, a.y));
            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y)) p.Add(new(b.x, y));
        }
        else
        {
            for (int y = a.y; y != b.y; y += (int)Mathf.Sign(b.y - a.y)) p.Add(new(a.x, y));
            for (int x = a.x; x != b.x; x += (int)Mathf.Sign(b.x - a.x)) p.Add(new(x, b.y));
        }
        p.Add(b);
        return p;
    }

    Room FindRoom(Partition p) => p?.room ?? FindRoom(p.left) ?? FindRoom(p.right);

    void OnDrawGizmos()
    {
        if (root == null) return;
        Gizmos.color = new(1, 1, 1, .05f); DrawBounds(root);

        Gizmos.color = Color.green;
        foreach (var r in rooms) Gizmos.DrawWireCube(new(r.rect.center.x, 0, r.rect.center.y), new(r.rect.width, .01f, r.rect.height));

        Gizmos.color = Color.white;
        foreach (var c in corridors)
            foreach (var t in c.tiles) Gizmos.DrawCube(new(t.x + .5f, 0, t.y + .5f), new(.9f, .05f, .9f));

        Gizmos.color = new Color(.4f, .2f, 0); // brown doors
        foreach (var d in doors) Gizmos.DrawCube(new(d.x + .5f, 0, d.y + .5f), new(.7f, .1f, .7f));
    }

    void DrawBounds(Partition p)
    {
        Gizmos.DrawWireCube(new(p.rect.center.x, 0, p.rect.center.y), new(p.rect.width, .01f, p.rect.height));
        if (p.left != null) DrawBounds(p.left);
        if (p.right != null) DrawBounds(p.right);
    }
}

public class Partition { public RectInt rect; public Partition left, right; public Room room; public Partition(RectInt r) => rect = r; }
public class Room { public RectInt rect; public Room(RectInt r) => rect = r; }
public class Corridor { public List<Vector2Int> tiles; public Corridor(List<Vector2Int> t) => tiles = t; }