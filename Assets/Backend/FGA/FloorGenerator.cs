// FloorGenerator_v3.cs // Corridor Door Upgrade (Random Door Placement)
// Minimal BSP Floor Generator — Commit A
// - Asymmetric random insets for rooms (version B)
// - Global corridor list for visualization
// - Keeps geometry aligned to partition grid (Option 2)

using UnityEngine;
using System.Collections.Generic;

public class FloorGenerator : MonoBehaviour
{
    [Header("Floor Settings")]
    public int width = 50;
    public int height = 50;
    [Tooltip("Partitions smaller than this won't split.")]
    public int minPartitionSize = 10;

    [Header("Room Insets (cells)")]
    public int minInset = 1; // minimum inset on any side
    public int maxInset = 3; // maximum inset on any side

    private Partition root;
    private List<Partition> leafPartitions = new List<Partition>();
    private List<Room> rooms = new List<Room>();
    private List<Corridor> corridors = new List<Corridor>();

    void Start()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        // clear old
        leafPartitions.Clear();
        rooms.Clear();
        corridors.Clear();

        root = new Partition(new RectInt(0, 0, width, height));
        SplitPartition(root);
        CreateRooms();
        ConnectRooms(root);
    }

    void SplitPartition(Partition p)
    {
        // If partition is already too small, make it a leaf
        if (p.rect.width <= minPartitionSize || p.rect.height <= minPartitionSize)
        {
            leafPartitions.Add(p);
            return;
        }

        bool splitVert = p.rect.width > p.rect.height;

        if (splitVert)
        {
            int split = p.rect.width / 2; // fixed 50/50 for this commit
            p.left = new Partition(new RectInt(p.rect.x, p.rect.y, split, p.rect.height));
            p.right = new Partition(new RectInt(p.rect.x + split, p.rect.y, p.rect.width - split, p.rect.height));
        }
        else
        {
            int split = p.rect.height / 2;
            p.left = new Partition(new RectInt(p.rect.x, p.rect.y, p.rect.width, split));
            p.right = new Partition(new RectInt(p.rect.x, p.rect.y + split, p.rect.width, p.rect.height - split));
        }

        SplitPartition(p.left);
        SplitPartition(p.right);
    }

    void CreateRooms()
    {
        foreach (Partition p in leafPartitions)
        {
            // choose asymmetric insets per side (version B)
            int leftInset = Random.Range(minInset, maxInset + 1);
            int rightInset = Random.Range(minInset, maxInset + 1);
            int bottomInset = Random.Range(minInset, maxInset + 1);
            int topInset = Random.Range(minInset, maxInset + 1);

            // compute room bounds clamped to ensure valid size
            int rx = p.rect.x + leftInset;
            int ry = p.rect.y + bottomInset;
            int rWidth = p.rect.width - (leftInset + rightInset);
            int rHeight = p.rect.height - (bottomInset + topInset);

            // safety: ensure rooms are at least 1x1
            rWidth = Mathf.Max(rWidth, 1);
            rHeight = Mathf.Max(rHeight, 1);

            // if insets were too large, shift the room to fit within partition
            if (rx + rWidth > p.rect.x + p.rect.width)
            {
                rx = p.rect.x + p.rect.width - rWidth;
            }
            if (ry + rHeight > p.rect.y + p.rect.height)
            {
                ry = p.rect.y + p.rect.height - rHeight;
            }

            RectInt r = new RectInt(rx, ry, rWidth, rHeight);

            Room room = new Room(r);
            p.room = room;
            rooms.Add(room);
        }
    }

    void ConnectRooms(Partition p)
    {
        if (p == null) return;

        // if both children exist and each subtree has a room, connect them
        if (p.left != null && p.right != null)
        {
            Room leftRoom = FindRoomInSubtree(p.left);
            Room rightRoom = FindRoomInSubtree(p.right);

            if (leftRoom != null && rightRoom != null)
            {
                corridors.Add(new Corridor(leftRoom, rightRoom));
            }
        }

        ConnectRooms(p.left);
        ConnectRooms(p.right);
    }

    // find a room within a subtree: prefer leaf's own room, otherwise search children
    Room FindRoomInSubtree(Partition p)
    {
        if (p == null) return null;
        if (p.room != null) return p.room;
        Room r = FindRoomInSubtree(p.left);
        if (r != null) return r;
        return FindRoomInSubtree(p.right);
    }

    void OnDrawGizmos()
    {
        if (root == null) return;

        // draw partition bounds faintly for debugging
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        DrawPartitionBounds(root);

        // rooms
        Gizmos.color = Color.green;
        foreach (Room room in rooms)
        {
            Vector3 center = new Vector3(room.rect.center.x, 0, room.rect.center.y);
            Vector3 size = new Vector3(room.rect.width, 0, room.rect.height);
            Gizmos.DrawWireCube(center, size);
        }

        // corridors (center-to-center LATER we'll make door-to-door)
        Gizmos.color = Color.white;
        foreach (var c in corridors)
        {
            Vector3 a = new Vector3(c.a.rect.center.x, 0, c.a.rect.center.y);
            Vector3 b = new Vector3(c.b.rect.center.x, 0, c.b.rect.center.y);
            Gizmos.DrawLine(a, b);
        }
    }

    void DrawPartitionBounds(Partition p)
    {
        if (p == null) return;
        Vector3 center = new Vector3(p.rect.center.x, 0, p.rect.center.y);
        Vector3 size = new Vector3(p.rect.width, 0, p.rect.height);
        Gizmos.DrawWireCube(center, size);

        DrawPartitionBounds(p.left);
        DrawPartitionBounds(p.right);
    }
}

[System.Serializable]
public class Partition
{
    public RectInt rect;
    public Partition left, right;
    public Room room;

    public Partition(RectInt rect)
    {
        this.rect = rect;
    }
}

[System.Serializable]
public class Room
{
    public RectInt rect;

    public Room(RectInt rect)
    {
        this.rect = rect;
    }
}

[System.Serializable]
public struct Corridor
{
    public Room a;
    public Room b;

    public Corridor(Room a, Room b)
    {
        this.a = a;
        this.b = b;
    }
}
