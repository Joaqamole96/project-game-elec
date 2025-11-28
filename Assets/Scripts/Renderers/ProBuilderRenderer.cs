// ================================================== //
// Scripts/Renderers/ProBuilderRoomRenderer.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders rooms using stretched ProBuilder prefabs (1 floor, 4 walls, 4 corners, doorways)
/// Much more efficient than tile-by-tile rendering
/// </summary>
public class ProBuilderRoomRenderer
{
    private BiomeManager _biomeManager;
    
    // Base prefab references (from Resources/Layout/)
    private GameObject _floorPrefab;
    private GameObject _wallPrefab;
    private GameObject _cornerPrefab;
    private GameObject _doorwayPrefab;
    
    public ProBuilderRoomRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        LoadBasePrefabs();
    }
    
    private void LoadBasePrefabs()
    {
        _floorPrefab = Resources.Load<GameObject>("Layout/pf_Floor");
        _wallPrefab = Resources.Load<GameObject>("Layout/pf_Wall");
        _cornerPrefab = Resources.Load<GameObject>("Layout/pf_Corner");
        _doorwayPrefab = Resources.Load<GameObject>("Layout/pf_Doorway");
        
        if (_floorPrefab == null) Debug.LogError("ProBuilder: pf_Floor not found!");
        if (_wallPrefab == null) Debug.LogError("ProBuilder: pf_Wall not found!");
        if (_cornerPrefab == null) Debug.LogError("ProBuilder: pf_Corner not found!");
        if (_doorwayPrefab == null) Debug.LogError("ProBuilder: pf_Doorway not found!");
    }
    
    // ==========================================
    // MAIN RENDERING METHOD
    // ==========================================
    
    public void RenderAllRooms(LevelModel layout, List<RoomModel> rooms, Transform parent, string biome)
    {
        if (rooms == null || parent == null) return;
        
        // Get biome materials
        Material floorMaterial = GetBiomeMaterial(biome, "Floor");
        Material wallMaterial = GetBiomeMaterial(biome, "Wall");
        Material doorMaterial = GetBiomeMaterial(biome, "Door");
        
        int roomsRendered = 0;
        
        foreach (var room in rooms)
        {
            RenderRoom(room, layout, parent, floorMaterial, wallMaterial, doorMaterial);
            roomsRendered++;
        }
        
        Debug.Log($"ProBuilderRenderer: Rendered {roomsRendered} rooms optimally");
    }
    
    // ==========================================
    // ROOM RENDERING
    // ==========================================
    
    private void RenderRoom(RoomModel room, LevelModel layout, Transform parent, 
                            Material floorMat, Material wallMat, Material doorMat)
    {
        // Create room container
        GameObject roomContainer = new GameObject($"Room_{room.ID}_{room.Type}");
        roomContainer.transform.SetParent(parent);
        roomContainer.transform.localPosition = Vector3.zero;
        
        // 1. Render stretched floor (1 object for entire room)
        RenderRoomFloor(room, roomContainer.transform, floorMat);
        
        // 2. Render 4 corners
        RenderRoomCorners(room, roomContainer.transform, wallMat);
        
        // 3. Render walls (stretched between corners, excluding doorways)
        RenderRoomWalls(room, layout, roomContainer.transform, wallMat);
        
        // 4. Render doorways
        RenderRoomDoorways(room, layout, roomContainer.transform, wallMat, doorMat);
    }
    
    // ==========================================
    // FLOOR RENDERING
    // ==========================================
    
    private void RenderRoomFloor(RoomModel room, Transform parent, Material material)
    {
        if (_floorPrefab == null) return;
        
        // Calculate floor dimensions (inner area, excluding walls)
        int width = room.Bounds.width - 2; // Subtract 2 for walls
        int depth = room.Bounds.height - 2;
        
        if (width <= 0 || depth <= 0) return;
        
        // Spawn floor at room center (Y=0.5 for floor level)
        Vector3 centerPos = new Vector3(
            room.Bounds.center.x,
            0.5f, // Floor height
            room.Bounds.center.y
        );
        
        GameObject floor = Object.Instantiate(_floorPrefab, centerPos, Quaternion.identity, parent);
        floor.name = "Floor";
        
        // Stretch floor - only X and Z, Y stays at 1
        floor.transform.localScale = new Vector3(width, 1, depth);
        
        // Apply biome material
        ApplyMaterialToObject(floor, material);
    }
    
    // ==========================================
    // CORNER RENDERING
    // ==========================================
    
    private void RenderRoomCorners(RoomModel room, Transform parent, Material material)
    {
        if (_cornerPrefab == null) return;
        
        var bounds = room.Bounds;
        
        // Define 4 corner positions (at exact wall intersections)
        // Y=5 because corner is 10 units tall (0-10) + 1 unit floor
        Vector3[] cornerPositions = new Vector3[]
        {
            new Vector3(bounds.xMin + 0.5f, 5f, bounds.yMin + 0.5f), // SW
            new Vector3(bounds.xMin + 0.5f, 5f, bounds.yMax - 0.5f), // NW
            new Vector3(bounds.xMax - 0.5f, 5f, bounds.yMin + 0.5f), // SE
            new Vector3(bounds.xMax - 0.5f, 5f, bounds.yMax - 0.5f)  // NE
        };
        
        string[] cornerNames = { "SW_Corner", "NW_Corner", "SE_Corner", "NE_Corner" };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject corner = Object.Instantiate(_cornerPrefab, cornerPositions[i], Quaternion.identity, parent);
            corner.name = cornerNames[i];
            ApplyMaterialToObject(corner, material);
        }
    }
    
    // ==========================================
    // WALL RENDERING
    // ==========================================
    
    private void RenderRoomWalls(RoomModel room, LevelModel layout, Transform parent, Material material)
    {
        if (_wallPrefab == null) return;
        
        var bounds = room.Bounds;
        
        // Find all doorway positions for this room
        HashSet<Vector2Int> doorwayPositions = GetDoorwayPositionsForRoom(room, layout);
        
        // Render each wall side, splitting around doorways
        RenderWallSide(bounds.xMin, bounds.yMin + 1, bounds.yMax - 1, true, false, doorwayPositions, parent, material, "West");
        RenderWallSide(bounds.xMax - 1, bounds.yMin + 1, bounds.yMax - 1, true, false, doorwayPositions, parent, material, "East");
        RenderWallSide(bounds.yMin, bounds.xMin + 1, bounds.xMax - 1, false, false, doorwayPositions, parent, material, "South");
        RenderWallSide(bounds.yMax - 1, bounds.xMin + 1, bounds.xMax - 1, false, true, doorwayPositions, parent, material, "North");
    }
    
    private void RenderWallSide(int fixedCoord, int start, int end, bool isVertical, bool isNorth,
                                HashSet<Vector2Int> doorways, Transform parent, Material material, string sideName)
    {
        List<WallSegment> segments = CalculateWallSegments(fixedCoord, start, end, isVertical, doorways);
        
        int segmentIndex = 0;
        foreach (var segment in segments)
        {
            Vector3 position = CalculateWallPosition(segment, isVertical);
            Quaternion rotation = GetWallRotation(isVertical, isNorth);
            int length = segment.end - segment.start;
            
            GameObject wall = Object.Instantiate(_wallPrefab, position, rotation, parent);
            wall.name = $"{sideName}_Wall_{segmentIndex++}";
            
            // Stretch wall to cover segment
            // Wall base is 1x11x0.75, stretch along appropriate axis
            if (isVertical)
            {
                // East/West walls stretch along Z axis
                wall.transform.localScale = new Vector3(1, 1, length);
            }
            else
            {
                // North/South walls stretch along X axis  
                wall.transform.localScale = new Vector3(length, 1, 1);
            }
            
            ApplyMaterialToObject(wall, material);
        }
    }
    
    // ==========================================
    // DOORWAY RENDERING
    // ==========================================
    
    private void RenderRoomDoorways(RoomModel room, LevelModel layout, Transform parent, Material wallMat, Material doorMat)
    {
        if (_doorwayPrefab == null || layout.AllDoorTiles == null) return;
        
        var bounds = room.Bounds;
        
        foreach (var doorPos in layout.AllDoorTiles)
        {
            // Check if door is on this room's perimeter
            if (IsOnRoomPerimeter(doorPos, bounds))
            {
                // Doorway at ground level (Y=0)
                Vector3 worldPos = new Vector3(doorPos.x + 0.5f, 0f, doorPos.y + 0.5f);
                Quaternion rotation = GetDoorRotation(doorPos, bounds);
                
                GameObject doorway = Object.Instantiate(_doorwayPrefab, worldPos, rotation, parent);
                doorway.name = $"Doorway_{doorPos.x}_{doorPos.y}";
                
                // Apply materials to doorway parts
                ApplyMaterialToDoorway(doorway, wallMat, doorMat);
                
                // Ensure DoorController exists on DoorFrame
                Transform doorFrame = doorway.transform.Find("DoorFrame");
                if (doorFrame != null && !doorFrame.TryGetComponent<DoorController>(out _))
                {
                    DoorController controller = doorFrame.gameObject.AddComponent<DoorController>();
                    
                    // Setup door references
                    Transform door = doorway.transform.Find("Door");
                    if (door != null)
                    {
                        Debug.Log($"controller != null: {controller != null}");
                        Debug.Log($"controller.doorModel != null: {controller.doorModel != null}"); // throws error here
                        Debug.Log($"door != null: {door != null}");
                        Debug.Log($"door.gameObject != null: {door.gameObject != null}");
                        controller.doorModel = door.gameObject;
                    }
                }
            }
        }
    }
    
    // ==========================================
    // UTILITY METHODS
    // ==========================================
    
    private struct WallSegment
    {
        public int fixedCoord;
        public int start;
        public int end;
    }
    
    private List<WallSegment> CalculateWallSegments(int fixedCoord, int start, int end, 
                                                     bool isVertical, HashSet<Vector2Int> doorways)
    {
        List<WallSegment> segments = new List<WallSegment>();
        int currentStart = start;
        
        for (int i = start; i < end; i++)
        {
            Vector2Int checkPos = isVertical ? new Vector2Int(fixedCoord, i) : new Vector2Int(i, fixedCoord);
            
            if (doorways.Contains(checkPos))
            {
                // Found doorway - close current segment
                if (i > currentStart)
                {
                    segments.Add(new WallSegment { fixedCoord = fixedCoord, start = currentStart, end = i });
                }
                
                // Skip doorway (3 units wide)
                i += 2;
                currentStart = i + 1;
            }
        }
        
        // Add final segment
        if (currentStart < end)
        {
            segments.Add(new WallSegment { fixedCoord = fixedCoord, start = currentStart, end = end });
        }
        
        return segments;
    }
    
    private Vector3 CalculateWallPosition(WallSegment segment, bool isVertical)
    {
        float midpoint = (segment.start + segment.end) / 2f;
        
        // Y position = 5 (center of 10-unit tall wall + 1 floor unit)
        if (isVertical)
        {
            // East/West walls
            return new Vector3(segment.fixedCoord + 0.5f, 5f, midpoint + 0.5f);
        }
        else
        {
            // North/South walls
            return new Vector3(midpoint + 0.5f, 5f, segment.fixedCoord + 0.5f);
        }
    }
    
    private Quaternion GetWallRotation(bool isVertical, bool isNorth)
    {
        if (isVertical)
        {
            // East/West walls - no rotation (facing along Z axis)
            return Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // North/South walls - rotate 90° to face along X axis
            return Quaternion.Euler(0, 90, 0);
        }
    }
    
    private Quaternion GetDoorRotation(Vector2Int doorPos, RectInt bounds)
    {
        // Check which wall the door is on
        bool isOnEastWest = (doorPos.x == bounds.xMin || doorPos.x == bounds.xMax - 1);
        bool isOnNorthSouth = (doorPos.y == bounds.yMin || doorPos.y == bounds.yMax - 1);
        
        if (isOnEastWest)
        {
            // Door on East/West wall - no rotation
            return Quaternion.Euler(0, 0, 0);
        }
        else if (isOnNorthSouth)
        {
            // Door on North/South wall - rotate 90°
            return Quaternion.Euler(0, 90, 0);
        }
        
        return Quaternion.identity;
    }
    
    private HashSet<Vector2Int> GetDoorwayPositionsForRoom(RoomModel room, LevelModel layout)
    {
        HashSet<Vector2Int> doorways = new HashSet<Vector2Int>();
        
        if (layout.AllDoorTiles == null) return doorways;
        
        var bounds = room.Bounds;
        
        foreach (var doorPos in layout.AllDoorTiles)
        {
            if (IsOnRoomPerimeter(doorPos, bounds))
            {
                doorways.Add(doorPos);
            }
        }
        
        return doorways;
    }
    
    private bool IsOnRoomPerimeter(Vector2Int pos, RectInt bounds)
    {
        return (pos.x == bounds.xMin || pos.x == bounds.xMax - 1 || 
                pos.y == bounds.yMin || pos.y == bounds.yMax - 1) &&
               pos.x >= bounds.xMin && pos.x < bounds.xMax &&
               pos.y >= bounds.yMin && pos.y < bounds.yMax;
    }
    
    // ==========================================
    // PROBUILDER MESH STRETCHING - REMOVED
    // ==========================================
    
    // ProBuilder meshes are stretched via localScale directly
    // No need for custom StretchProBuilderMesh method
    
    // ==========================================
    // MATERIAL APPLICATION
    // ==========================================
    
    private Material GetBiomeMaterial(string biome, string type)
    {
        string path = $"Layout/{biome}/{type}Material";
        Material mat = Resources.Load<Material>(path);
        
        if (mat == null)
        {
            Debug.LogWarning($"Material not found at {path}, using default");
            mat = new Material(Shader.Find("Standard"));
        }
        
        return mat;
    }
    
    private void ApplyMaterialToObject(GameObject obj, Material material)
    {
        if (obj == null || material == null) return;
        
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = material;
        }
    }
    
    private void ApplyMaterialToDoorway(GameObject doorway, Material wallMat, Material doorMat)
    {
        if (doorway == null) return;
        
        // Apply wall material to frame and wall
        Transform frame = doorway.transform.Find("DoorFrame/Frame");
        Transform wall = doorway.transform.Find("DoorFrame/Wall");
        Transform floor = doorway.transform.Find("Floor");
        
        if (frame != null) ApplyMaterialToObject(frame.gameObject, wallMat);
        if (wall != null) ApplyMaterialToObject(wall.gameObject, wallMat);
        if (floor != null) ApplyMaterialToObject(floor.gameObject, wallMat);
        
        // Apply door material to door
        Transform door = doorway.transform.Find("Door");
        if (door != null) ApplyMaterialToObject(door.gameObject, doorMat);
    }
}