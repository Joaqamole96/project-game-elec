// ================================= //
// FloorController.cs
//
// Contains the logic for the Binary Space Partitioning (BSP) algorithm,
// managing the division of the floor space into partitions.
// 
// ** FIX: Updated path generation loops (CalculateLPathTiles) to use System.Math.Sign 
// ** and robust break conditions to prevent infinite loops (OutOfMemoryException).
// ================================= //

using UnityEngine;
using System.Collections.Generic;

public class FloorController 
{
    // ----- Constants ----- //
    
    public const int MIN_PART_SIZE = 5; 
    public const int MAX_PART_SIZE = 10;
    public const float PART_SIZE_VAR_CHANCE = 0.25f;
    
    // Room Generation Constants
    public const int MIN_ROOM_SIZE = 3;
    public const int MIN_PADDING = 1;
    public const int MAX_PADDING_RAND = 2;
    public const int PATH_WIDTH = 1; 

    // ----- New Tile Structure for Unified Map Generation ----- //
    // This enum defines the possible state of any single tile in the grid.
    public enum TileType { Empty, Floor, Wall }

    // --- Should Stop Split ---
    public static bool ShouldStopSplit(Partition partition, int floorMaxDepth)
    {
        if (partition.Area.width <= MIN_PART_SIZE || partition.Area.height <= MIN_PART_SIZE) return true;
        else if (partition.Depth >= floorMaxDepth) return true;
        else if (partition.Area.width <= MAX_PART_SIZE && partition.Area.height <= MAX_PART_SIZE && Random.value < PART_SIZE_VAR_CHANCE) return true;
        return false;
    }

    // --- Calculate Split ---
    private static int CalculateSplit(int areaLength)
    {
        float minRatio = 0.4f;
        float maxRatio = 0.6f;
        
        int minSplit = Mathf.FloorToInt(areaLength * minRatio);
        int maxSplit = Mathf.CeilToInt(areaLength * maxRatio);

        minSplit = Mathf.Max(minSplit, MIN_PART_SIZE);
        maxSplit = Mathf.Min(maxSplit, areaLength - MIN_PART_SIZE);
        
        if (minSplit >= maxSplit)
        {
            return areaLength / 2;
        }

        return Random.Range(minSplit, maxSplit + 1);
    }

    // --- Split Partition ---
    public static void SplitPartition(Partition parentPartition, int floorMaxDepth)
    {
        if (ShouldStopSplit(parentPartition, floorMaxDepth))
        {
            parentPartition.IsLeaf = true;
            return;
        }

        SplitType split;
        bool splitVertically = parentPartition.Area.width >= parentPartition.Area.height;

        if (parentPartition.Area.width == parentPartition.Area.height || Random.value < 0.1f)
        {
            split = (SplitType)Random.Range(0, 2);
            splitVertically = (split == SplitType.Vertical);
        }
        else
        {
            split = splitVertically ? SplitType.Vertical : SplitType.Horizontal;
        }

        if (splitVertically)
        {
            int splitX = CalculateSplit(parentPartition.Area.width);

            parentPartition.LeftChild = new Partition(
                new RectInt(parentPartition.Area.x, parentPartition.Area.y, splitX, parentPartition.Area.height),
                parentPartition.Depth + 1
            );

            parentPartition.RightChild = new Partition(
                new RectInt(parentPartition.Area.x + splitX, parentPartition.Area.y, parentPartition.Area.width - splitX, parentPartition.Area.height),
                parentPartition.Depth + 1
            );
        }
        else
        {
            int splitY = CalculateSplit(parentPartition.Area.height);

            parentPartition.LeftChild = new Partition(
                new RectInt(parentPartition.Area.x, parentPartition.Area.y, parentPartition.Area.width, splitY),
                parentPartition.Depth + 1
            );

            parentPartition.RightChild = new Partition(
                new RectInt(parentPartition.Area.x, parentPartition.Area.y + splitY, parentPartition.Area.width, parentPartition.Area.height - splitY),
                parentPartition.Depth + 1
            );
        }

        SplitPartition(parentPartition.LeftChild, floorMaxDepth);
        SplitPartition(parentPartition.RightChild, floorMaxDepth);
    }
    
    // --- Collect Leaf Partitions ---
    public static void CollectLeafPartitions(Partition current, List<Partition> leaves)
    {
        if (current == null) return;

        if (current.IsLeaf)
        {
            leaves.Add(current);
            return;
        }

        CollectLeafPartitions(current.LeftChild, leaves);
        CollectLeafPartitions(current.RightChild, leaves);
    }
    
    // --- Create Room In Partition ---
    public static void CreateRoomInPartition(Partition partition)
    {
        if (!partition.IsLeaf) return;
        
        int availableWidth = partition.Area.width - (2 * MIN_PADDING);
        int availableHeight = partition.Area.height - (2 * MIN_PADDING);

        if (availableWidth < MIN_ROOM_SIZE || availableHeight < MIN_ROOM_SIZE)
        {
            return;
        }

        int maxWidth = Mathf.Max(MIN_ROOM_SIZE, availableWidth);
        int maxHeight = Mathf.Max(MIN_ROOM_SIZE, availableHeight);

        int roomWidth = Random.Range(MIN_ROOM_SIZE, maxWidth + 1);
        int roomHeight = Random.Range(MIN_ROOM_SIZE, maxHeight + 1);

        int maxOffsetX = partition.Area.width - roomWidth - MIN_PADDING;
        int maxOffsetY = partition.Area.height - roomHeight - MIN_PADDING;
        
        int offsetX = Random.Range(MIN_PADDING, maxOffsetX + 1);
        int offsetY = Random.Range(MIN_PADDING, maxOffsetY + 1);
        
        RectInt roomArea = new RectInt(
            partition.Area.x + offsetX,
            partition.Area.y + offsetY,
            roomWidth,
            roomHeight
        );
        
        // This is where the Room property is set, ensuring discrete placement
        partition.Room = roomArea;
    }

    // --- Get Room In Subtree ---
    public static Partition GetRoomInSubtree(Partition current)
    {
        if (current == null) return null;
        if (current.IsLeaf && current.Room.HasValue)
        {
            return current;
        }

        Partition room = GetRoomInSubtree(current.LeftChild);
        if (room != null)
        {
            return room;
        }

        return GetRoomInSubtree(current.RightChild);
    }

    // --- NEW: Find the Entry/Exit point on the Room Boundary (Corridor Smoothing Logic) ---
    private static Vector2Int FindEntryPointOnRoom(RectInt room, Vector2Int startCenter, Vector2Int endCenter, bool xFirst)
    {
        // 1. Determine the path direction from center to center
        Vector2Int direction = endCenter - startCenter;

        if (xFirst)
        {
            // Horizontal path segment first. Connection point is on East/West walls.
            // We align the Y-coordinate with the room's center Y.
            
            int roomCenterY = Mathf.RoundToInt(room.center.y);
            int pathY = roomCenterY;

            // Clamp the Y coordinate to be on the inner tiles of the room's vertical bounds.
            pathY = Mathf.Clamp(pathY, room.yMin, room.yMax - 1); 
            
            if (direction.x > 0) // Moving Right: Connect to East wall (MaxX - 1)
            {
                return new Vector2Int(room.xMax - 1, pathY);
            }
            else // Moving Left: Connect to West wall (MinX)
            {
                return new Vector2Int(room.xMin, pathY);
            }
        }
        else
        {
            // Vertical path segment first. Connection point is on North/South walls.
            // We align the X-coordinate with the room's center X.
            
            int roomCenterX = Mathf.RoundToInt(room.center.x);
            int pathX = roomCenterX;

            // Clamp the X coordinate to be on the inner tiles of the room's horizontal bounds.
            pathX = Mathf.Clamp(pathX, room.xMin, room.xMax - 1); 

            if (direction.y > 0) // Moving Up: Connect to North wall (MaxY - 1)
            {
                return new Vector2Int(pathX, room.yMax - 1);
            }
            else // Moving Down: Connect to South wall (MinY)
            {
                return new Vector2Int(pathX, room.yMin);
            }
        }
    }

    // --- Calculate L-Shaped Path Tiles (Corridor Smoothing Logic) ---
    private static List<Vector2Int> CalculateLPathTiles(RectInt roomA, RectInt roomB, bool xFirst)
    {
        // 1. Get Room Centers (only used for determining initial path direction)
        Vector2Int center1 = Vector2Int.RoundToInt(roomA.center);
        Vector2Int center2 = Vector2Int.RoundToInt(roomB.center);
        
        // 2. Determine Boundary Points
        // boundaryStart is the tile * inside * Room A where the path originates.
        Vector2Int boundaryStart = FindEntryPointOnRoom(roomA, center1, center2, xFirst);
        // boundaryEnd is the tile * inside * Room B where the path terminates.
        Vector2Int boundaryEnd = FindEntryPointOnRoom(roomB, center2, center1, !xFirst); 

        // 3. Define Corridor Start and Corner Point
        Vector2Int current = boundaryStart;
        Vector2Int cornerPoint = Vector2Int.zero; 

        // Path Direction (from A to B)
        Vector2Int dir = center2 - center1;

        // Step 1: Find the first tile **OUTSIDE** Room A (start of the actual corridor).
        if (xFirst)
        {
            // First segment is horizontal (X changes, Y is fixed).
            current.x += (dir.x > 0) ? 1 : -1; // Step one tile horizontally out
            
            // The corner point has the X from the destination room's boundary-adjacent point
            // and the fixed Y from the start of the corridor.
            cornerPoint = new Vector2Int(boundaryEnd.x, current.y);
        }
        else
        {
            // First segment is vertical (Y changes, X is fixed).
            current.y += (dir.y > 0) ? 1 : -1; // Step one tile vertically out

            // The corner point has the Y from the destination room's boundary-adjacent point
            // and the fixed X from the start of the corridor.
            cornerPoint = new Vector2Int(current.x, boundaryEnd.y);
        }
        
        // --- Core L-Path Generation Logic from Start (outside A) to Corner and then to B's entrance ---
        List<Vector2Int> path = new List<Vector2Int>();

        if (xFirst)
        {
            // 1. Horizontal Segment (X changes, Y is constant)
            // Use Math.Sign for robust direction
            int dirX = System.Math.Sign(cornerPoint.x - current.x);
            
            // FIX 1: Check for zero length segment to prevent unnecessary loop entry
            if (dirX != 0) 
            {
                while (current.x != cornerPoint.x)
                {
                    path.Add(current);
                    current.x += dirX;
                }
            }
            // Add the corner tile (which is the current position)
            path.Add(current); 
            
            // 2. Vertical Segment (Y changes, X is constant)
            int dirY = System.Math.Sign(boundaryEnd.y - current.y);
            
            // FIX 2: Check for zero length segment and use a safer termination check.
            if (dirY != 0) 
            {
                while (current.y != boundaryEnd.y)
                {
                    // FIX 3: If the next step would be the room entrance, we break. 
                    // This prevents overshooting and infinite loops if logic failed to align perfectly.
                    if (System.Math.Abs(boundaryEnd.y - current.y) == 1)
                    {
                        break; 
                    }
                    
                    current.y += dirY;
                    path.Add(current);
                }
            }
        }
        else // Y-segment first
        {
            // 1. Vertical Segment (Y changes, X is constant)
            int dirY = System.Math.Sign(cornerPoint.y - current.y);
            
            if (dirY != 0)
            {
                while (current.y != cornerPoint.y)
                {
                    path.Add(current);
                    current.y += dirY;
                }
            }
            // Add the corner tile
            path.Add(current);

            // 2. Horizontal Segment (X changes, Y is constant)
            int dirX = System.Math.Sign(boundaryEnd.x - current.x);

            if (dirX != 0)
            {
                while (current.x != boundaryEnd.x) 
                {
                    // FIX 3: If the next step would be the room entrance, we break.
                    if (System.Math.Abs(boundaryEnd.x - current.x) == 1)
                    {
                        break;
                    }
                    
                    current.x += dirX;
                    path.Add(current);
                }
            }
        }
        
        return path;
    }
    
    // --- Create Path (Updated to calculate and store tiles) ---
    public static void CreatePath(FloorModel floor, Partition p1, Partition p2)
    {
        if (!p1.Room.HasValue || !p2.Room.HasValue)
        {
            return;
        }

        // Determine the L-path type randomly
        bool xFirst = Random.value < 0.5f;

        // Calculate the discrete tile path from boundary-adjacent point to boundary-adjacent point
        List<Vector2Int> pathTiles = CalculateLPathTiles(p1.Room.Value, p2.Room.Value, xFirst);

        // Store the path data with the calculated tile list
        floor.Paths.Add(new Path(p1, p2, pathTiles));
    }

    // --- Traverse And Connect ---
    public static void TraverseAndConnect(FloorModel floor, Partition current)
    {
        if (current == null || current.IsLeaf)
        {
            return;
        }

        TraverseAndConnect(floor, current.LeftChild);
        TraverseAndConnect(floor, current.RightChild);

        Partition roomA = GetRoomInSubtree(current.LeftChild);
        Partition roomB = GetRoomInSubtree(current.RightChild);

        if (roomA != null && roomB != null)
        {
            CreatePath(floor, roomA, roomB);
        }
    }
    
    // --- NEW: Generate Unified Tile Map ---
    // Processes all data (rooms and paths) into a single map, eliminating redundancy.
    public static TileType[,] GenerateTileMap(FloorModel floor, int floorSize)
    {
        // Initialize the map with 'Empty' tiles
        TileType[,] map = new TileType[floorSize, floorSize];
        
        // Helper to check bounds
        bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < floorSize && y >= 0 && y < floorSize;
        }

        // 1. Process Rooms (Mark Floor and surrounding Walls)
        foreach (var partition in floor.Partitions)
        {
            if (!partition.Room.HasValue) continue;

            RectInt room = partition.Room.Value;

            // Mark Floor Tiles
            for (int x = room.xMin; x < room.xMax; x++)
            {
                for (int y = room.yMin; y < room.yMax; y++)
                {
                    if (IsInBounds(x, y)) map[x, y] = TileType.Floor;
                }
            }

            // Mark Surrounding Wall Tiles (One-tile border around the room)
            for (int x = room.xMin - 1; x <= room.xMax; x++)
            {
                for (int y = room.yMin - 1; y <= room.yMax; y++)
                {
                    if (IsInBounds(x, y) && map[x, y] != TileType.Floor)
                    {
                        // Mark as Wall, BUT only if it's not already a Floor tile
                        map[x, y] = TileType.Wall;
                    }
                }
            }
        }
        
        // 2. Process Paths (Corridors)
        foreach (var path in floor.Paths)
        {
            if (path.PathTiles == null) continue;

            foreach (Vector2Int tile in path.PathTiles)
            {
                // Mark the path tile as Floor (it connects rooms)
                if (IsInBounds(tile.x, tile.y))
                {
                    map[tile.x, tile.y] = TileType.Floor;
                }
                
                // Mark Walls around the corridor (1-tile thick boundary)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        int x = tile.x + dx;
                        int y = tile.y + dy;
                        
                        if (IsInBounds(x, y) && map[x, y] == TileType.Empty)
                        {
                            // Only overwrite 'Empty' tiles with 'Wall'
                            map[x, y] = TileType.Wall;
                        }
                    }
                }
            }
        }

        return map;
    }


    // --- Generate Floor ---
    // The return type is changed to a custom struct to return both the Model and the Map.
    public struct FloorGenerationResult
    {
        public FloorModel Model;
        public TileType[,] TileMap;
    }
    
    public static FloorGenerationResult GenerateFloor(int floorSize, int floorMaxDepth, int seed)
    {
        // 1. Initialize Random Seed
        if (seed != 0)
        {
            Random.InitState(seed);
        }
        else
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        
        FloorModel floor = new FloorModel();

        // 2. Initialize Root Partition
        RectInt floorBounds = new RectInt(0, 0, floorSize, floorSize);
        floor.RootPartition = new Partition(floorBounds, 0);

        // 3. Perform BSP Split
        SplitPartition(floor.RootPartition, floorMaxDepth);

        // 4. Collect Leaf Partitions
        CollectLeafPartitions(floor.RootPartition, floor.Partitions);
        
        // 5. Create Rooms within Leaf Partitions
        foreach (var partition in floor.Partitions)
        {
            CreateRoomInPartition(partition);
        }

        // 6. Connect Rooms/Partitions with Paths
        TraverseAndConnect(floor, floor.RootPartition);

        // 7. Generate the FINAL Unified Tile Map (The new performance-focused step)
        TileType[,] finalMap = GenerateTileMap(floor, floorSize);

        return new FloorGenerationResult { Model = floor, TileMap = finalMap };
    }
}
