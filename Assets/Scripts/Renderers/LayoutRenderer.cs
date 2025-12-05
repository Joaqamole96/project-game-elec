// ================================================== //
// Scripts/Renderers/LayoutRenderer.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

public class LayoutRenderer
{
    // Base prefab references (from Resources/Layout/)
    private GameObject _floorPrefab;
    private GameObject _wallPrefab;
    private GameObject _cornerPrefab;
    private GameObject _doorwayPrefab;

    public LayoutRenderer()
    {
        LoadBasePrefabs();
    }

    private void LoadBasePrefabs()
    {
        try
        {
            _floorPrefab = ResourceService.LoadFloorPrefab();
            _wallPrefab = ResourceService.LoadWallPrefab();
            _cornerPrefab = ResourceService.LoadCornerPrefab();
            _doorwayPrefab = ResourceService.LoadDoorwayPrefab();
            
            Debug.Log("LayoutRenderer: Base prefabs loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Critical error loading base prefabs: {ex.Message}");
        }
    }
    
    // ==========================================
    // MAIN RENDERING METHOD
    // ==========================================

    public void RenderAllRooms(LevelModel layout, List<RoomModel> rooms, Transform parent, string biome)
    {
        try
        {
            Debug.Log($"LayoutRenderer: Starting room rendering for {rooms.Count} rooms in {biome} biome");
            // Get biome materials with validation
            Material floorMaterial = ResourceService.LoadFloorMaterial(biome);
            Material wallMaterial = ResourceService.LoadWallMaterial(biome);
            Material doorMaterial = ResourceService.LoadDoorMaterial(biome);
            
            int roomsRendered = 0;
            int roomsFailed = 0;

            foreach (var room in rooms)
            {
                if (room?.Bounds == null)
                {
                    Debug.LogWarning("LayoutRenderer: Skipping room with null bounds");
                    roomsFailed++;
                    continue;
                }
                if (RenderRoom(room, layout, parent, floorMaterial, wallMaterial, doorMaterial)) roomsRendered++;
                else roomsFailed++;
            }
            Debug.Log($"LayoutRenderer: Rendering completed - {roomsRendered} rooms rendered, {roomsFailed} failed");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Critical error during room rendering: {ex.Message}");
        }
    }
    
    // ==========================================
    // ROOM RENDERING
    // ==========================================
    
    private bool RenderRoom(RoomModel room, LevelModel layout, Transform parent, Material floorMat, Material wallMat, Material doorMat)
    {
        try
        {
            // Validate prefabs before rendering
            if (_floorPrefab == null || _wallPrefab == null || _cornerPrefab == null || _doorwayPrefab == null)
            {
                Debug.LogError($"LayoutRenderer: Missing prefabs for room {room.ID}");
                return false;
            }
            // Create room container
            GameObject roomContainer = new($"Room_{room.ID}_{room.Type}");
            roomContainer.transform.SetParent(parent);
            roomContainer.transform.localPosition = Vector3.zero;
            // Render room components
            RenderRoomFloor(room, roomContainer.transform, floorMat);
            RenderRoomCorners(room, roomContainer.transform, wallMat);
            RenderRoomWalls(room, layout, roomContainer.transform, wallMat);
            RenderRoomDoorways(room, layout, roomContainer.transform, wallMat, doorMat);
            Debug.Log($"LayoutRenderer: Room {room.ID} rendered successfully");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering room {room.ID}: {ex.Message}");
            return false;
        }
    }
    
    // ==========================================
    // FLOOR RENDERING
    // ==========================================
    
    private void RenderRoomFloor(RoomModel room, Transform parent, Material material)
    {
        try
        {
            // Calculate floor dimensions (inner area, excluding walls)
            int width = room.Bounds.width - 2;
            int depth = room.Bounds.height - 2;
            if (width <= 0 || depth <= 0)
            {
                Debug.LogWarning($"LayoutRenderer: Room {room.ID} has invalid dimensions for floor: {width}x{depth}");
                return;
            }
            // Spawn floor at room center (Y=0.5 for floor level)
            Vector3 centerPos = new(
                room.Bounds.center.x,
                0.5f,
                room.Bounds.center.y
            );
            GameObject floor = Object.Instantiate(_floorPrefab, centerPos, Quaternion.identity, parent);
            floor.name = "Floor";
            // Stretch floor - only X and Z, Y stays at 1
            floor.transform.localScale = new Vector3(width, 1, depth);
            ApplyMaterialToObject(floor, material);
            Debug.Log($"LayoutRenderer: Floor rendered for room {room.ID} at {centerPos} with scale {width}x1x{depth}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering floor for room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // CORNER RENDERING
    // ==========================================
    
    private void RenderRoomCorners(RoomModel room, Transform parent, Material material)
    {
        try
        {
            var bounds = room.Bounds;
            // Define 4 corner positions (at exact wall intersections)
            Vector3[] cornerPositions = new Vector3[]
            {
                new(bounds.xMin + 0.5f, 5.5f, bounds.yMin + 0.5f), // SW
                new(bounds.xMin + 0.5f, 5.5f, bounds.yMax - 0.5f), // NW
                new(bounds.xMax - 0.5f, 5.5f, bounds.yMin + 0.5f), // SE
                new(bounds.xMax - 0.5f, 5.5f, bounds.yMax - 0.5f)  // NE
            };
            string[] cornerNames = { "SW_Corner", "NW_Corner", "SE_Corner", "NE_Corner" };
            for (int i = 0; i < 4; i++)
            {
                GameObject corner = Object.Instantiate(_cornerPrefab, cornerPositions[i], Quaternion.identity, parent);
                corner.name = cornerNames[i];
                ApplyMaterialToObject(corner, material);
            }
            Debug.Log($"LayoutRenderer: 4 corners rendered for room {room.ID}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering corners for room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // WALL RENDERING
    // ==========================================
    
    private void RenderRoomWalls(RoomModel room, LevelModel layout, Transform parent, Material material)
    {
        try
        {
            var bounds = room.Bounds;
            // Find all doorway positions for this room
            HashSet<Vector2Int> doorwayPositions = GetDoorwayPositionsForRoom(room, layout);
            // Render each wall side, splitting around doorways
            RenderWallSide(bounds.xMin, bounds.yMin + 1, bounds.yMax - 1, true, false, doorwayPositions, parent, material, "West");
            RenderWallSide(bounds.xMax - 1, bounds.yMin + 1, bounds.yMax - 1, true, true, doorwayPositions, parent, material, "East");
            RenderWallSide(bounds.yMin, bounds.xMin + 1, bounds.xMax - 1, false, false, doorwayPositions, parent, material, "South");
            RenderWallSide(bounds.yMax - 1, bounds.xMin + 1, bounds.xMax - 1, false, true, doorwayPositions, parent, material, "North");
            Debug.Log($"LayoutRenderer: Walls rendered for room {room.ID} with {doorwayPositions.Count} doorways");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering walls for room {room.ID}: {ex.Message}");
        }
    }
    
    private void RenderWallSide(int fixedCoord, int start, int end, bool isVertical, bool isPositiveSide, HashSet<Vector2Int> doorways, Transform parent, Material material, string sideName)
    {
        try
        {
            List<WallSegment> segments = CalculateWallSegments(fixedCoord, start, end, isVertical, doorways, isPositiveSide);
            if (segments.Count == 0)
            {
                Debug.LogWarning($"LayoutRenderer: No wall segments to render for {sideName} side");
                return;
            }
            int segmentIndex = 0;
            foreach (var segment in segments)
            {
                int baseLength = segment.end - segment.start;
                int adjustedLength = baseLength;
                Vector3 position = CalculateWallPosition(segment, isVertical, isPositiveSide, ref adjustedLength);
                Quaternion rotation = GetWallRotation(isVertical);
                if (adjustedLength <= 0)
                {
                    Debug.LogWarning($"LayoutRenderer: Skipping zero-length wall segment on {sideName} side");
                    continue;
                }
                GameObject wall = Object.Instantiate(_wallPrefab, position, rotation, parent);
                wall.name = $"{sideName}_Wall_{segmentIndex++}";
                // ALL walls stretch along X-scale only, Y and Z remain 1
                wall.transform.localScale = new Vector3(adjustedLength, 1, 1);
                ApplyMaterialToObject(wall, material);
            }
            Debug.Log($"LayoutRenderer: {sideName} wall rendered with {segments.Count} segments");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering {sideName} wall: {ex.Message}");
        }
    }
    
    // ==========================================
    // DOORWAY RENDERING
    // ==========================================
    
    private void RenderRoomDoorways(RoomModel room, LevelModel layout, Transform parent, Material wallMat, Material doorMat)
    {
        try
        {
            if (layout.AllDoorTiles == null || layout.AllDoorTiles.Count == 0)
            {
                Debug.LogWarning($"LayoutRenderer: No door tiles found in layout for room {room.ID}");
                return;
            }
            var bounds = room.Bounds;
            int doorsRendered = 0;
            foreach (var doorPos in layout.AllDoorTiles) if (IsOnRoomPerimeter(doorPos, bounds) && RenderSingleDoorway(doorPos, bounds, parent, wallMat, doorMat)) doorsRendered++;
            Debug.Log($"LayoutRenderer: {doorsRendered} doorways rendered for room {room.ID}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering doorways for room {room.ID}: {ex.Message}");
        }
    }
    
    private bool RenderSingleDoorway(Vector2Int doorPos, RectInt bounds, Transform parent, Material wallMat, Material doorMat)
    {
        try
        {
            // Doorway at ground level (Y=0)
            Vector3 worldPos = new(doorPos.x + 0.5f, 0f, doorPos.y + 0.5f);
            Quaternion rotation = GetDoorRotation(doorPos, bounds);
            GameObject doorway = Object.Instantiate(_doorwayPrefab, worldPos, rotation, parent);
            doorway.name = $"Doorway_{doorPos.x}_{doorPos.y}";
            // Apply materials and setup door controller
            ApplyMaterialToDoorway(doorway, wallMat, doorMat);
            SetupDoorController(doorway, doorPos);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error rendering doorway at {doorPos}: {ex.Message}");
            return false;
        }
    }
    
    private void SetupDoorController(GameObject doorway, Vector2Int doorPos)
    {
        try
        {
            Transform doorFrame = doorway.transform.Find("DoorFrame");
            if (doorFrame == null)
            {
                Debug.LogWarning($"LayoutRenderer: No 'DoorFrame' child found in doorway prefab at {doorPos}");
                return;
            }
            // Ensure collider exists before adding DoorController
            if (doorFrame.GetComponent<Collider>() == null)
            {
                doorFrame.gameObject.AddComponent<BoxCollider>();
                Debug.Log($"LayoutRenderer: Added BoxCollider to DoorFrame at {doorPos}");
            }
            DoorController controller = doorFrame.GetComponent<DoorController>();
            if (controller == null) controller = doorFrame.gameObject.AddComponent<DoorController>();
            // Setup door references
            Transform door = doorway.transform.Find("Door");
            if (door != null)
            {
                controller.doorModel = door.gameObject;
                Debug.Log($"LayoutRenderer: DoorController setup completed for door at {doorPos}");
            }
            else Debug.LogWarning($"LayoutRenderer: No 'Door' child found in doorway prefab at {doorPos}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error setting up door controller at {doorPos}: {ex.Message}");
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
        public bool isPositiveSide;
        public bool isVertical;
    }
    
    private List<WallSegment> CalculateWallSegments(int fixedCoord, int start, int end, bool isVertical, HashSet<Vector2Int> doorways, bool isPositiveSide)
    {
        try
        {
            List<WallSegment> segments = new();
            int currentStart = start;
            for (int i = start; i < end; i++)
            {
                Vector2Int checkPos = isVertical ? new Vector2Int(fixedCoord, i) : new Vector2Int(i, fixedCoord);
                if (doorways.Contains(checkPos))
                {
                    // Found doorway - close current segment at doorway start
                    if (i - 1 > currentStart) segments.Add(new WallSegment { 
                        fixedCoord = fixedCoord, 
                        start = currentStart, 
                        end = i - 1,  // Stop before doorway
                        isPositiveSide = isPositiveSide,
                        isVertical = isVertical
                    });
                    // Skip doorway (3 units wide)
                    i += 2;
                    currentStart = i;  // Start after doorway
                    if (i >= end) break;
                }
            }
            // Add final segment
            if (currentStart < end) segments.Add(new WallSegment { 
                fixedCoord = fixedCoord, 
                start = currentStart, 
                end = end,
                isPositiveSide = isPositiveSide,
                isVertical = isVertical
            });
            return segments;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error calculating wall segments: {ex.Message}");
            return new List<WallSegment>();
        }
    }
    
    private Vector3 CalculateWallPosition(WallSegment segment, bool isVertical, bool isPositiveSide, ref int adjustedLength)
    {
        try
        {
            float midpoint = (segment.start + segment.end) / 2f;
            // Walls are 11 units high, positioned at y=5.5
            if (isVertical)
            {
                // West/East walls
                float zPos = midpoint - 0.5f; // Fixed Z-pos correction
                if (!isPositiveSide) // West wall (-X direction)
                {
                    adjustedLength += 1;
                    zPos += 0.5f;
                }
                if (segment.isPositiveSide) // East wall (+X direction)
                {
                    adjustedLength += 1;
                    zPos += 0.5f;
                }
                return new Vector3(segment.fixedCoord + 0.5f, 5.5f, zPos);
            }
            else
            {
                // North/South walls
                float xPos = midpoint - 0.5f; // Fixed X-pos correction
                if (!isPositiveSide) // South wall (-Z direction)
                {
                    adjustedLength += 1;
                    xPos += 0.5f;
                }
                if (segment.isPositiveSide) // North wall (+Z direction)
                {
                    adjustedLength += 1;
                    xPos += 0.5f;
                }
                return new Vector3(xPos, 5.5f, segment.fixedCoord + 0.5f);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error calculating wall position: {ex.Message}");
            return Vector3.zero;
        }
    }
    
    private Quaternion GetWallRotation(bool isVertical)
        => isVertical ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
    
    private Quaternion GetDoorRotation(Vector2Int doorPos, RectInt bounds)
    {
        try
        {
            bool isOnEastWest = (doorPos.x == bounds.xMin || doorPos.x == bounds.xMax - 1);
            // Door on East/West wall - Y-rotation should be 90
            if (isOnEastWest) return Quaternion.Euler(0, 90, 0);
            // Door on North/South wall - Y-rotation should be 0
            else return Quaternion.identity;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error getting door rotation: {ex.Message}");
            return Quaternion.identity;
        }
    }
    
    private HashSet<Vector2Int> GetDoorwayPositionsForRoom(RoomModel room, LevelModel layout)
    {
        try
        {
            HashSet<Vector2Int> doorways = new();
            if (layout.AllDoorTiles == null) return doorways;
            var bounds = room.Bounds;
            foreach (var doorPos in layout.AllDoorTiles) if (IsOnRoomPerimeter(doorPos, bounds)) doorways.Add(doorPos);
            return doorways;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error getting doorway positions: {ex.Message}");
            return new HashSet<Vector2Int>();
        }
    }
    
    private bool IsOnRoomPerimeter(Vector2Int pos, RectInt bounds)
        => (pos.x == bounds.xMin || 
            pos.x == bounds.xMax - 1 || 
            pos.y == bounds.yMin || 
            pos.y == bounds.yMax - 1) &&
                pos.x >= bounds.xMin && 
                pos.x < bounds.xMax &&
                pos.y >= bounds.yMin && 
                pos.y < bounds.yMax;
    
    private void ApplyMaterialToObject(GameObject obj, Material material)
    {
        try
        {
            if (obj == null || material == null)
            {
                Debug.LogWarning("LayoutRenderer: Cannot apply material - null object or material");
                return;
            }
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) renderer.sharedMaterial = material;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error applying material to object: {ex.Message}");
        }
    }
    
    private void ApplyMaterialToDoorway(GameObject doorway, Material wallMat, Material doorMat)
    {
        try
        {
            if (doorway == null) return;
            // Apply wall material to frame and wall components
            Transform frame = doorway.transform.Find("DoorFrame/Frame");
            Transform wall = doorway.transform.Find("DoorFrame/Wall");
            Transform floor = doorway.transform.Find("Floor");
            if (frame != null) ApplyMaterialToObject(frame.gameObject, wallMat);
            if (wall != null) ApplyMaterialToObject(wall.gameObject, wallMat);
            if (floor != null) ApplyMaterialToObject(floor.gameObject, wallMat);
            // Apply door material to door component
            Transform door = doorway.transform.Find("Door");
            if (door != null) ApplyMaterialToObject(door.gameObject, doorMat);
            Debug.Log("LayoutRenderer: Materials applied to doorway components");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LayoutRenderer: Error applying materials to doorway: {ex.Message}");
        }
    }
}