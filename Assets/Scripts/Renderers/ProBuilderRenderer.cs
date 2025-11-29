// ================================================== //
// Scripts/Renderers/ProBuilderRoomRenderer.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

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
    
    // Cache for biome materials to avoid repeated Resources.Load calls
    private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
    
    public ProBuilderRoomRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        LoadBasePrefabs();
    }
    
    /// <summary>
    /// Loads the base ProBuilder prefabs used for room rendering
    /// </summary>
    private void LoadBasePrefabs()
    {
        try
        {
            _floorPrefab = Resources.Load<GameObject>("Layout/pf_Floor");
            _wallPrefab = Resources.Load<GameObject>("Layout/pf_Wall");
            _cornerPrefab = Resources.Load<GameObject>("Layout/pf_Corner");
            _doorwayPrefab = Resources.Load<GameObject>("Layout/pf_Doorway");
            
            if (_floorPrefab == null) Debug.LogError("ProBuilderRoomRenderer: pf_Floor prefab not found in Resources/Layout/!");
            if (_wallPrefab == null) Debug.LogError("ProBuilderRoomRenderer: pf_Wall prefab not found in Resources/Layout/!");
            if (_cornerPrefab == null) Debug.LogError("ProBuilderRoomRenderer: pf_Corner prefab not found in Resources/Layout/!");
            if (_doorwayPrefab == null) Debug.LogError("ProBuilderRoomRenderer: pf_Doorway prefab not found in Resources/Layout/!");
            
            Debug.Log("ProBuilderRoomRenderer: Base prefabs loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error loading base prefabs: {ex.Message}");
        }
    }
    
    // ==========================================
    // MAIN RENDERING METHOD
    // ==========================================
    
    /// <summary>
    /// Renders all rooms in the level using ProBuilder prefabs
    /// </summary>
    /// <param name="layout">The level layout data</param>
    /// <param name="rooms">List of rooms to render</param>
    /// <param name="parent">Parent transform for room objects</param>
    /// <param name="biome">Biome to use for materials</param>
    public void RenderAllRooms(LevelModel layout, List<RoomModel> rooms, Transform parent, string biome)
    {
        try
        {
            if (rooms == null || parent == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render rooms - null rooms list or parent transform");
                return;
            }
            
            if (layout == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render rooms - null layout");
                return;
            }
            
            Debug.Log($"ProBuilderRoomRenderer: Starting room rendering for {rooms.Count} rooms in {biome} biome");

            // Get biome materials
            Material floorMaterial = GetBiomeMaterial(biome, "Floor");
            Material wallMaterial = GetBiomeMaterial(biome, "Wall");
            Material doorMaterial = GetBiomeMaterial(biome, "Door");
            
            int roomsRendered = 0;
            int roomsFailed = 0;
            
            foreach (var room in rooms)
            {
                if (room?.Bounds == null)
                {
                    Debug.LogWarning("ProBuilderRoomRenderer: Skipping room with null bounds");
                    roomsFailed++;
                    continue;
                }
                
                RenderRoom(room, layout, parent, floorMaterial, wallMaterial, doorMaterial);
                roomsRendered++;
            }
            
            Debug.Log($"ProBuilderRoomRenderer: Rendering completed - {roomsRendered} rooms rendered, {roomsFailed} failed");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error during room rendering: {ex.Message}");
        }
    }
    
    // ==========================================
    // ROOM RENDERING
    // ==========================================
    
    /// <summary>
    /// Renders a single room with floor, walls, corners, and doorways
    /// </summary>
    private void RenderRoom(RoomModel room, LevelModel layout, Transform parent, 
                            Material floorMat, Material wallMat, Material doorMat)
    {
        try
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
            
            Debug.Log($"ProBuilderRoomRenderer: Room {room.ID} rendered successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // FLOOR RENDERING
    // ==========================================
    
    /// <summary>
    /// Renders a stretched floor prefab for the entire room
    /// </summary>
    private void RenderRoomFloor(RoomModel room, Transform parent, Material material)
    {
        try
        {
            if (_floorPrefab == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render floor - floor prefab is null");
                return;
            }
            
            // Calculate floor dimensions (inner area, excluding walls)
            int width = room.Bounds.width - 2; // Subtract 2 for walls
            int depth = room.Bounds.height - 2;
            
            if (width <= 0 || depth <= 0)
            {
                Debug.LogWarning($"ProBuilderRoomRenderer: Room {room.ID} has invalid dimensions for floor: {width}x{depth}");
                return;
            }
            
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
            
            Debug.Log($"ProBuilderRoomRenderer: Floor rendered for room {room.ID} at {centerPos} with scale {width}x1x{depth}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering floor for room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // CORNER RENDERING
    // ==========================================
    
    /// <summary>
    /// Renders the four corner pieces of the room
    /// </summary>
    private void RenderRoomCorners(RoomModel room, Transform parent, Material material)
    {
        try
        {
            if (_cornerPrefab == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render corners - corner prefab is null");
                return;
            }
            
            var bounds = room.Bounds;
            
            // Define 4 corner positions (at exact wall intersections)
            // Walls and corners are 11 units high, positioned at y=5.5
            Vector3[] cornerPositions = new Vector3[]
            {
                new Vector3(bounds.xMin + 0.5f, 5.5f, bounds.yMin + 0.5f), // SW
                new Vector3(bounds.xMin + 0.5f, 5.5f, bounds.yMax - 0.5f), // NW
                new Vector3(bounds.xMax - 0.5f, 5.5f, bounds.yMin + 0.5f), // SE
                new Vector3(bounds.xMax - 0.5f, 5.5f, bounds.yMax - 0.5f)  // NE
            };
            
            string[] cornerNames = { "SW_Corner", "NW_Corner", "SE_Corner", "NE_Corner" };
            
            for (int i = 0; i < 4; i++)
            {
                GameObject corner = Object.Instantiate(_cornerPrefab, cornerPositions[i], Quaternion.identity, parent);
                corner.name = cornerNames[i];
                ApplyMaterialToObject(corner, material);
            }
            
            Debug.Log($"ProBuilderRoomRenderer: 4 corners rendered for room {room.ID}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering corners for room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // WALL RENDERING
    // ==========================================
    
    /// <summary>
    /// Renders all walls for the room, splitting around doorways
    /// </summary>
    private void RenderRoomWalls(RoomModel room, LevelModel layout, Transform parent, Material material)
    {
        try
        {
            if (_wallPrefab == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render walls - wall prefab is null");
                return;
            }
            
            var bounds = room.Bounds;
            
            // Find all doorway positions for this room
            HashSet<Vector2Int> doorwayPositions = GetDoorwayPositionsForRoom(room, layout);
            
            // Render each wall side, splitting around doorways
            RenderWallSide(bounds.xMin, bounds.yMin + 1, bounds.yMax - 1, true, false, doorwayPositions, parent, material, "West");
            RenderWallSide(bounds.xMax - 1, bounds.yMin + 1, bounds.yMax - 1, true, true, doorwayPositions, parent, material, "East");
            RenderWallSide(bounds.yMin, bounds.xMin + 1, bounds.xMax - 1, false, false, doorwayPositions, parent, material, "South");
            RenderWallSide(bounds.yMax - 1, bounds.xMin + 1, bounds.xMax - 1, false, true, doorwayPositions, parent, material, "North");
            
            Debug.Log($"ProBuilderRoomRenderer: Walls rendered for room {room.ID} with {doorwayPositions.Count} doorways");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering walls for room {room.ID}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Renders a single wall side, splitting it into segments around doorways
    /// </summary>
    private void RenderWallSide(int fixedCoord, int start, int end, bool isVertical, bool isPositiveSide,
                                HashSet<Vector2Int> doorways, Transform parent, Material material, string sideName)
    {
        try
        {
            List<WallSegment> segments = CalculateWallSegments(fixedCoord, start, end, isVertical, doorways, isPositiveSide);
            
            int segmentIndex = 0;
            foreach (var segment in segments)
            {
                // Calculate base length
                int baseLength = segment.end - segment.start;
                
                // Apply scaling and positioning corrections
                int adjustedLength = baseLength;
                Vector3 position = CalculateWallPosition(segment, isVertical, isPositiveSide, ref adjustedLength);
                Quaternion rotation = GetWallRotation(isVertical, isPositiveSide);
                
                if (adjustedLength <= 0)
                {
                    Debug.LogWarning($"ProBuilderRoomRenderer: Skipping zero-length wall segment on {sideName} side");
                    continue;
                }
                
                GameObject wall = Object.Instantiate(_wallPrefab, position, rotation, parent);
                wall.name = $"{sideName}_Wall_{segmentIndex++}";
                
                // ALL walls stretch along X-scale only, Y and Z remain 1
                wall.transform.localScale = new Vector3(adjustedLength, 1, 1);
                
                ApplyMaterialToObject(wall, material);
                
                Debug.Log($"ProBuilderRoomRenderer: {sideName} wall segment {segmentIndex-1} at {position} with scale {adjustedLength}x1x1");
            }
            
            Debug.Log($"ProBuilderRoomRenderer: {sideName} wall rendered with {segments.Count} segments");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering {sideName} wall: {ex.Message}");
        }
    }
    
    // ==========================================
    // DOORWAY RENDERING
    // ==========================================
    
    /// <summary>
    /// Renders all doorways for the room
    /// </summary>
    private void RenderRoomDoorways(RoomModel room, LevelModel layout, Transform parent, Material wallMat, Material doorMat)
    {
        try
        {
            if (_doorwayPrefab == null)
            {
                Debug.LogError("ProBuilderRoomRenderer: Cannot render doorways - doorway prefab is null");
                return;
            }
            
            if (layout.AllDoorTiles == null)
            {
                Debug.LogWarning($"ProBuilderRoomRenderer: No door tiles found in layout for room {room.ID}");
                return;
            }
            
            var bounds = room.Bounds;
            int doorsRendered = 0;
            
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
                    
                    // Ensure collider exists before adding DoorController
                    Transform doorFrame = doorway.transform.Find("DoorFrame");
                    if (doorFrame != null)
                    {
                        // Add collider if missing to prevent DoorController error
                        if (doorFrame.GetComponent<Collider>() == null)
                        {
                            doorFrame.gameObject.AddComponent<BoxCollider>();
                            Debug.Log($"ProBuilderRoomRenderer: Added BoxCollider to DoorFrame at {doorPos}");
                        }
                        
                        DoorController controller = doorFrame.GetComponent<DoorController>();
                        if (controller == null)
                        {
                            controller = doorFrame.gameObject.AddComponent<DoorController>();
                        }
                        
                        // Setup door references
                        Transform door = doorway.transform.Find("Door");
                        if (door != null)
                        {
                            controller.doorModel = door.gameObject;
                            Debug.Log($"ProBuilderRoomRenderer: DoorController setup completed for door at {doorPos}");
                        }
                        else
                        {
                            Debug.LogWarning($"ProBuilderRoomRenderer: No 'Door' child found in doorway prefab at {doorPos}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"ProBuilderRoomRenderer: No 'DoorFrame' child found in doorway prefab at {doorPos}");
                    }
                    
                    doorsRendered++;
                }
            }
            
            Debug.Log($"ProBuilderRoomRenderer: {doorsRendered} doorways rendered for room {room.ID}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error rendering doorways for room {room.ID}: {ex.Message}");
        }
    }
    
    // ==========================================
    // UTILITY METHODS
    // ==========================================
    
    /// <summary>
    /// Represents a segment of wall between doorways
    /// </summary>
    private struct WallSegment
    {
        public int fixedCoord;
        public int start;
        public int end;
        public bool isPositiveSide;
        public bool isVertical;
    }
    
    /// <summary>
    /// Calculates wall segments by splitting around doorway positions
    /// </summary>
    private List<WallSegment> CalculateWallSegments(int fixedCoord, int start, int end, 
                                                    bool isVertical, HashSet<Vector2Int> doorways, bool isPositiveSide)
    {
        try
        {
            List<WallSegment> segments = new List<WallSegment>();
            int currentStart = start;
            
            for (int i = start; i < end; i++)
            {
                Vector2Int checkPos = isVertical ? new Vector2Int(fixedCoord, i) : new Vector2Int(i, fixedCoord);
                
                if (doorways.Contains(checkPos))
                {
                    // Found doorway - close current segment at doorway start (not center)
                    // FIXED: Changed from 'i' to 'i-1' to stop 1 unit before doorway center
                    if (i - 1 > currentStart)
                    {
                        segments.Add(new WallSegment { 
                            fixedCoord = fixedCoord, 
                            start = currentStart, 
                            end = i - 1,  // Stop before doorway
                            isPositiveSide = isPositiveSide,
                            isVertical = isVertical
                        });
                    }
                    
                    // Skip doorway (3 units wide) - ensure we don't go beyond end
                    i += 2;
                    currentStart = i;  // Start after doorway
                    
                    if (i >= end) break;
                }
            }
            
            // Add final segment
            if (currentStart < end)
            {
                segments.Add(new WallSegment { 
                    fixedCoord = fixedCoord, 
                    start = currentStart, 
                    end = end,
                    isPositiveSide = isPositiveSide,
                    isVertical = isVertical
                });
            }
            
            return segments;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error calculating wall segments: {ex.Message}");
            return new List<WallSegment>();
        }
    }
    
    /// <summary>
    /// Calculates the world position for a wall segment with scaling and positioning corrections
    /// </summary>
    private Vector3 CalculateWallPosition(WallSegment segment, bool isVertical, bool isPositiveSide, ref int adjustedLength)
    {
        try
        {
            float midpoint = (segment.start + segment.end) / 2f;
            
            // Walls are 11 units high, positioned at y=5.5
            if (isVertical)
            {
                // West/East walls
                float zPos = midpoint;
                
                // FIXED: Z-pos of ALL West/East walls are over by 0.5 - decrease accordingly
                zPos -= 0.5f;
                
                // Apply base corrections for South/West walls (negative sides)
                if (!isPositiveSide) // West wall (-X direction)
                {
                    // FIXED: South and West walls x-scale is under by 1. Add scale by 1, add 0.5 to Z-pos of west walls
                    adjustedLength += 1;
                    zPos += 0.5f;
                }
                
                // Apply doorway adjacency corrections for West/East walls
                if (segment.isPositiveSide) // East wall (+X direction)
                {
                    // FIXED: +X side and +Z side doorway-adjacent walls x-scale are under by 1. Add scale by 1, add 0.5 to Z-pos of west walls
                    adjustedLength += 1;
                    zPos += 0.5f;
                }
                else // West wall (-X direction)  
                {
                    // West walls already got base correction above
                    // No additional doorway correction needed for negative sides
                }
                
                return new Vector3(segment.fixedCoord + 0.5f, 5.5f, zPos);
            }
            else
            {
                // North/South walls
                float xPos = midpoint;
                
                // FIXED: X-pos of ALL North/South walls are over by 0.5 - decrease accordingly
                xPos -= 0.5f;
                
                // Apply base corrections for South/West walls (negative sides)
                if (!isPositiveSide) // South wall (-Z direction)
                {
                    // FIXED: South and West walls x-scale is under by 1. Add scale by 1, add 0.5 to X-pos of south walls
                    adjustedLength += 1;
                    xPos += 0.5f;
                }
                
                // Apply doorway adjacency corrections for North/South walls
                if (segment.isPositiveSide) // North wall (+Z direction)
                {
                    // FIXED: +X side and +Z side doorway-adjacent walls x-scale are under by 1. Add scale by 1, add 0.5 to X-pos of south walls
                    adjustedLength += 1;
                    xPos += 0.5f;
                }
                else // South wall (-Z direction)
                {
                    // South walls already got base correction above
                    // No additional doorway correction needed for negative sides
                }
                
                return new Vector3(xPos, 5.5f, segment.fixedCoord + 0.5f);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error calculating wall position: {ex.Message}");
            return Vector3.zero;
        }
    }
    
    /// <summary>
    /// Gets the rotation for a wall based on orientation
    /// </summary>
    private Quaternion GetWallRotation(bool isVertical, bool isPositiveSide)
    {
        try
        {
            if (isVertical)
            {
                // West/East walls - Y-rotation should be 90
                return Quaternion.Euler(0, 90, 0);
            }
            else
            {
                // North/South walls - Y-rotation should be 0
                return Quaternion.identity;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error getting wall rotation: {ex.Message}");
            return Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Gets the rotation for a doorway based on wall position
    /// </summary>
    private Quaternion GetDoorRotation(Vector2Int doorPos, RectInt bounds)
    {
        try
        {
            bool isOnEastWest = (doorPos.x == bounds.xMin || doorPos.x == bounds.xMax - 1);
            bool isOnNorthSouth = (doorPos.y == bounds.yMin || doorPos.y == bounds.yMax - 1);
            
            if (isOnEastWest)
            {
                // Door on East/West wall - Y-rotation should be 90
                return Quaternion.Euler(0, 90, 0);
            }
            else if (isOnNorthSouth)
            {
                // Door on North/South wall - Y-rotation should be 0
                return Quaternion.identity;
            }
            
            Debug.LogWarning($"ProBuilderRoomRenderer: Door at {doorPos} is not on room perimeter");
            return Quaternion.identity;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error getting door rotation: {ex.Message}");
            return Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Gets all doorway positions for a specific room
    /// </summary>
    private HashSet<Vector2Int> GetDoorwayPositionsForRoom(RoomModel room, LevelModel layout)
    {
        try
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
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error getting doorway positions: {ex.Message}");
            return new HashSet<Vector2Int>();
        }
    }
    
    /// <summary>
    /// Checks if a position is on the room perimeter
    /// </summary>
    private bool IsOnRoomPerimeter(Vector2Int pos, RectInt bounds)
    {
        try
        {
            return (pos.x == bounds.xMin || pos.x == bounds.xMax - 1 || 
                    pos.y == bounds.yMin || pos.y == bounds.yMax - 1) &&
                   pos.x >= bounds.xMin && pos.x < bounds.xMax &&
                   pos.y >= bounds.yMin && pos.y < bounds.yMax;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error checking room perimeter: {ex.Message}");
            return false;
        }
    }
    
    // ==========================================
    // MATERIAL MANAGEMENT
    // ==========================================
    
    /// <summary>
    /// Gets biome-specific material with caching
    /// </summary>
    private Material GetBiomeMaterial(string biome, string type)
    {
        try
        {
            string cacheKey = $"{biome}_{type}";
            
            // Check cache first
            if (_materialCache.TryGetValue(cacheKey, out Material cachedMaterial))
            {
                return cachedMaterial;
            }
            
            string path = $"Layout/{biome}/{type}Material";
            Material mat = Resources.Load<Material>(path);
            
            if (mat == null)
            {
                Debug.LogWarning($"ProBuilderRoomRenderer: Material not found at {path}, using default Standard material");
                mat = new Material(Shader.Find("Standard"));
            }
            
            // Add to cache
            _materialCache[cacheKey] = mat;
            Debug.Log($"ProBuilderRoomRenderer: Loaded and cached material {cacheKey}");
            
            return mat;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error getting biome material {biome}/{type}: {ex.Message}");
            return new Material(Shader.Find("Standard"));
        }
    }
    
    /// <summary>
    /// Applies material to all renderers in a GameObject
    /// </summary>
    private void ApplyMaterialToObject(GameObject obj, Material material)
    {
        try
        {
            if (obj == null || material == null)
            {
                Debug.LogWarning("ProBuilderRoomRenderer: Cannot apply material - null object or material");
                return;
            }
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = material;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error applying material to object: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies appropriate materials to doorway components
    /// </summary>
    private void ApplyMaterialToDoorway(GameObject doorway, Material wallMat, Material doorMat)
    {
        try
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
            
            Debug.Log("ProBuilderRoomRenderer: Materials applied to doorway components");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error applying materials to doorway: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clears the material cache to free memory
    /// </summary>
    public void ClearMaterialCache()
    {
        try
        {
            _materialCache.Clear();
            Debug.Log("ProBuilderRoomRenderer: Material cache cleared");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ProBuilderRoomRenderer: Error clearing material cache: {ex.Message}");
        }
    }
}