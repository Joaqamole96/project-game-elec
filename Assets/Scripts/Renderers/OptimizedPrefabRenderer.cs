// OptimizedPrefabRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-performance prefab renderer that uses mesh combining for optimal rendering.
/// Supports biome biomes and efficient geometry batching.
/// </summary>
public class OptimizedPrefabRenderer
{
    private BiomeManager _biomeManager;
    private BiomeModel _currentBiome;
    private int _currentFloor;
    private AdvancedMeshCombiner _meshCombiner;
    private AdvancedMeshCombiner _wallCombiner;

    public OptimizedPrefabRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        _meshCombiner = new AdvancedMeshCombiner();
        _wallCombiner = new AdvancedMeshCombiner();
    }

    public void SetBiomeForFloor(int floorLevel)
    {
        _currentFloor = floorLevel;
        _currentBiome = _biomeManager.GetBiomeForFloor(floorLevel);
        Debug.Log($"Using biome: {_currentBiome?.Name} for floor {floorLevel}");
    }

    /// <summary>
    /// Renders all floor tiles using combined meshes for optimal performance.
    /// </summary>
    public void RenderFloorsOptimized(LevelModel layout, Transform parent)
    {
        if (_currentBiome == null)
        {
            Debug.LogError("Current biome is null in RenderFloorsOptimized!");
            return;
        }

        if (layout?.AllFloorTiles == null)
        {
            Debug.LogError("Layout or AllFloorTiles is null in RenderFloorsOptimized!");
            return;
        }

        Debug.Log($"=== FLOOR RENDERING DEBUG ===");
        Debug.Log($"Biome: {_currentBiome.Name}");
        Debug.Log($"Floor prefab path: {_currentBiome.FloorPrefabPath}");
        Debug.Log($"Floor tiles count: {layout.AllFloorTiles.Count}");

        var floorPrefab = _biomeManager.GetFloorPrefab(_currentBiome);

        if (floorPrefab == null)
        {
            Debug.LogError($"Floor prefab is NULL for biome: {_currentBiome.Name}");
            Debug.LogError($"Tried to load from: {_currentBiome.FloorPrefabPath}");
            
            // Test if Resources loading works at all
            var testPrefab = Resources.Load<GameObject>("Biomes/Default/FloorPrefab");
            Debug.Log($"Direct Resources load result: {testPrefab != null}");
            
            RenderFloorsAsPrimitives(layout, parent);
            return;
        }

        Debug.Log($"✓ Successfully loaded floor prefab: {floorPrefab.name}");

        Mesh floorMesh = GetPrefabMesh(floorPrefab);
        Material floorMaterial = GetPrefabMaterial(floorPrefab);
        Vector3 floorScale = floorPrefab.transform.localScale;

        Debug.Log($"Floor mesh: {floorMesh != null}");
        Debug.Log($"Floor material: {floorMaterial != null}");
        Debug.Log($"Floor scale: {floorScale}");

        if (floorMesh == null)
        {
            Debug.LogError("Could not get mesh from floor prefab!");
            // Check what's actually on the prefab
            var meshFilter = floorPrefab.GetComponentInChildren<MeshFilter>();
            Debug.Log($"MeshFilter on prefab: {meshFilter != null}");
            if (meshFilter != null) Debug.Log($"MeshFilter mesh: {meshFilter.sharedMesh != null}");
            return;
        }

        if (floorMaterial == null)
        {
            Debug.LogError("Could not get material from floor prefab!");
            return;
        }

        int floorsRendered = 0;
        foreach (var floorPos in layout.AllFloorTiles)
        {
            Vector3 worldPos = new(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
            _meshCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
            floorsRendered++;
        }

        Debug.Log($"✓ Processed {floorsRendered} floor positions for combining");
        Debug.Log($"=== END FLOOR DEBUG ===");
    }

    /// <summary>
    /// Renders all wall tiles using combined meshes for optimal performance.
    /// </summary>
    public void RenderWallsOptimized(LevelModel layout, Transform parent)
    {
        if (_currentBiome == null || layout?.AllWallTiles == null) return;

        var wallPrefab = _biomeManager.GetWallPrefab(_currentBiome);

        if (wallPrefab == null)
        {
            Debug.LogWarning("Wall prefab not found, using fallback logic");
            RenderWallsAsPrimitives(layout, parent);
            return;
        }

        Mesh wallMesh = GetPrefabMesh(wallPrefab);
        Material wallMaterial = GetPrefabMaterial(wallPrefab);
        Vector3 wallScale = wallPrefab.transform.localScale;

        if (wallMesh == null || wallMaterial == null) return;

        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                Vector3 worldPos = new(wallPos.x + 0.5f, 4.5f, wallPos.y + 0.5f);
                Quaternion rotation = GetWallRotation(wallType);
                _wallCombiner.AddMesh(wallMesh, worldPos, rotation, wallScale, wallMaterial); // Use _wallCombiner
            }
        }
    }

    /// <summary>
    /// Renders all door tiles and door tops using combined meshes.
    /// </summary>
    public void RenderDoorsOptimized(LevelModel layout, Transform parent)
    {
        if (_currentBiome == null || layout?.AllDoorTiles == null) return;

        // Render door frames
        var doorPrefab = _biomeManager.GetDoorPrefab(_currentBiome);
        if (doorPrefab != null)
        {
            foreach (var doorPos in layout.AllDoorTiles)
            {
                Vector3 worldPos = new(doorPos.x + 0.5f, 1.5f, doorPos.y + 0.5f);
                Quaternion rotation = GetDoorRotation(layout, doorPos);
                
                var door = GameObject.Instantiate(doorPrefab, worldPos, rotation, parent);
                door.name = $"Door_{doorPos.x}_{doorPos.y}";
                
                // Ensure DoorController is properly configured
                var doorController = door.GetComponent<DoorController>();
                if (doorController == null)
                {
                    doorController = door.AddComponent<DoorController>();
                }
            }
        }

        // NEW: Render door tops
        var doorTopPrefab = _biomeManager.GetPrefab("Biomes/Default/DoorTopPrefab");
        if (doorTopPrefab == null)
        {
            // Fallback: create simple door tops
            RenderDoorTopsAsPrimitives(layout, parent);
        }
        else
        {
            foreach (var doorPos in layout.AllDoorTiles)
            {
                Vector3 topPos = new(doorPos.x + 0.5f, 6f, doorPos.y + 0.5f); // Position above door
                var doorTop = GameObject.Instantiate(doorTopPrefab, topPos, Quaternion.identity, parent);
                doorTop.name = $"DoorTop_{doorPos.x}_{doorPos.y}";
            }
        }
    }

    private void RenderDoorTopsAsPrimitives(LevelModel layout, Transform parent)
    {
        foreach (var doorPos in layout.AllDoorTiles)
        {
            GameObject doorTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorTop.transform.position = new Vector3(doorPos.x + 0.5f, 2.5f, doorPos.y + 0.5f);
            doorTop.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f); // Thin horizontal piece
            doorTop.transform.SetParent(parent);
            doorTop.name = $"DoorTop_{doorPos.x}_{doorPos.y}";
            
            // Apply material
            Renderer renderer = doorTop.GetComponent<Renderer>();
            if (Application.isPlaying)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.gray;
            }
            else
            {
                Material sharedMaterial = new Material(Shader.Find("Standard"));
                sharedMaterial.color = Color.gray;
                renderer.sharedMaterial = sharedMaterial;
            }
        }
    }

    /// <summary>
    /// Finalizes all combined meshes and builds them in the scene.
    /// </summary>
    public void FinalizeRendering(Transform floorParent, Transform wallParent)
    {
        // Build floor combined meshes under floor parent
        var floorObjects = _meshCombiner.BuildAllCombinedMeshes(floorParent);
        
        // Build wall combined meshes under wall parent  
        var wallObjects = _wallCombiner.BuildAllCombinedMeshes(wallParent);
        
        Debug.Log($"Finalized rendering with {floorObjects.Count} floor meshes and {wallObjects.Count} wall meshes");
    }

    /// <summary>
    /// Renders a ceiling plane above the entire dungeon.
    /// </summary>
    public void RenderCeilingOptimized(LevelModel layout, Transform parent)
    {
        if (layout?.OverallBounds == null) return;

        // Create one main ceiling plane at Y=9
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "MainCeiling";
        ceiling.transform.SetParent(parent);
        
        // Position at top of walls
        BoundsInt dungeonBounds = layout.OverallBounds;
        Vector3 center = new(dungeonBounds.center.x, 9f, dungeonBounds.center.y);
        ceiling.transform.position = center;
        
        // Scale to cover entire dungeon
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.1f); 
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.1f);
        ceiling.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        // Apply one-sided mirror material
        Renderer renderer = ceiling.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateOneSidedMirrorMaterial();
        
        // Remove collider - ceiling shouldn't block movement
        GameObject.DestroyImmediate(ceiling.GetComponent<Collider>());
    }

    /// <summary>
    /// Renders a void plane below the dungeon to catch falling objects.
    /// </summary>
    public void RenderVoidPlane(LevelModel layout, Transform parent)
    {
        if (layout?.OverallBounds == null) return;

        GameObject voidPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        voidPlane.name = "VoidPlane";
        voidPlane.transform.SetParent(parent);
        
        // Position well below everything
        BoundsInt dungeonBounds = layout.OverallBounds;
        Vector3 center = new(dungeonBounds.center.x, -5f, dungeonBounds.center.y);
        voidPlane.transform.position = center;
        
        // Scale to cover entire dungeon + large buffer
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.2f); 
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.2f);
        voidPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        // Pure black material
        Renderer renderer = voidPlane.GetComponent<Renderer>();
        Material voidMaterial = new(Shader.Find("Standard"));
        voidMaterial.color = Color.black;
        voidMaterial.SetFloat("_Metallic", 0f);
        voidMaterial.SetFloat("_Glossiness", 0f);
        renderer.sharedMaterial = voidMaterial;
    }

    #region Helper Methods

    private Mesh GetPrefabMesh(GameObject prefab)
    {
        if (prefab == null) return null;
        var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
        return meshFilter != null ? meshFilter.sharedMesh : null;
    }

    private Material GetPrefabMaterial(GameObject prefab)
    {
        if (prefab == null) return null;
        var renderer = prefab.GetComponentInChildren<Renderer>();
        return renderer != null ? renderer.sharedMaterial : null;
    }

    private Quaternion GetDoorRotation(LevelModel layout, Vector2Int doorPos)
    {
        // First, determine which wall the door is actually on
        var adjacentRoom = FindAdjacentRoom(layout, doorPos);
        if (adjacentRoom != null)
        {
            return GetDoorRotationFromRoom(adjacentRoom, doorPos);
        }

        // Fallback: analyze adjacent tiles to guess orientation
        Vector2Int[] directions = new Vector2Int[]
        {
            new(0, 1),  // North
            new(0, -1), // South  
            new(1, 0),  // East
            new(-1, 0)  // West
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = doorPos + dir;
            bool isRoom = layout.GetRoomAtPosition(checkPos) != null;
            
            // If this direction leads to a room, the door should face away from it
            if (isRoom)
            {
                // Door should face OPPOSITE the room (toward corridor)
                return GetRotationFromDirection(-dir);
            }
        }

        // Ultimate fallback: face north
        return Quaternion.Euler(0, 0, 0);
    }

    private Quaternion GetRotationFromDirection(Vector2Int direction)
    {
        // Doors should be parallel to the wall they're on
        // If door is on north/south wall, it should face east/west (90° rotation)
        // If door is on east/west wall, it should face north/south (0° rotation)
        
        if (direction == new Vector2Int(0, 1) || direction == new Vector2Int(0, -1)) 
            return Quaternion.Euler(0, 90, 0);   // North/South walls - face east/west
        else 
            return Quaternion.Euler(0, 0, 0);    // East/West walls - face north/south
    }

    private RoomModel FindAdjacentRoom(LevelModel layout, Vector2Int doorPos)
    {
        Vector2Int[] checkDirections = new Vector2Int[]
        {
            new(0, 1), new(0, -1),
            new(1, 0), new(-1, 0)
        };

        foreach (var dir in checkDirections)
        {
            var room = layout.GetRoomAtPosition(doorPos + dir);
            if (room != null) return room;
        }
        
        return null;
    }

    private Quaternion GetDoorRotationFromRoom(RoomModel room, Vector2Int doorPos)
    {
        var bounds = room.Bounds;
        
        // Determine which wall the door is on and make door parallel to that wall
        if (doorPos.y == bounds.yMax - 1 || doorPos.y == bounds.yMin) 
            return Quaternion.Euler(0, 90, 0);   // North/South wall - face east/west
        else 
            return Quaternion.Euler(0, 0, 0);    // East/West wall - face north/south
    }

    private Quaternion GetWallRotation(WallType wallType)
    {
        return wallType switch
        {
            WallType.North => Quaternion.Euler(0, 0, 0),
            WallType.South => Quaternion.Euler(0, 180, 0),
            WallType.East => Quaternion.Euler(0, 90, 0),
            WallType.West => Quaternion.Euler(0, 270, 0),
            _ => Quaternion.identity
        };
    }

    private Material CreateOneSidedMirrorMaterial()
    {
        // Create a material that's transparent in editor but opaque at runtime
        Material material = new(Shader.Find("Standard"));
        
        #if UNITY_EDITOR
        // Transparent cyan in editor so you can see through it
        material.color = new Color(0, 1, 1, 0.3f); // Cyan, semi-transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        #else
        // Opaque at runtime
        material.color = Color.gray; // Or your ceiling color
        material.SetFloat("_Metallic", 0.1f);
        material.SetFloat("_Glossiness", 0.1f);
        #endif

        return material;
    }

    private void RenderWallsAsPrimitives(LevelModel layout, Transform parent)
    {
        // Fallback: Create primitive cubes if prefabs aren't available
        foreach (var wallPos in layout.AllWallTiles)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(wallPos.x + 0.5f, 4.5f, wallPos.y + 0.5f);
            wall.transform.localScale = new Vector3(1f, 8f, 1f);
            wall.transform.SetParent(parent);
            wall.name = $"Wall_{wallPos.x}_{wallPos.y}";
            
            // FIX: Use sharedMaterial to avoid material leaks
            Renderer renderer = wall.GetComponent<Renderer>();
            if (Application.isPlaying)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.gray;
            }
            else
            {
                Material sharedMaterial = new(Shader.Find("Standard"));
                sharedMaterial.color = Color.gray;
                renderer.sharedMaterial = sharedMaterial;
            }
        }
    }

    private void RenderFloorsAsPrimitives(LevelModel layout, Transform parent)
    {
        Debug.Log("Using primitive fallback for floor rendering");
        
        foreach (var floorPos in layout.AllFloorTiles)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
            floor.transform.localScale = new Vector3(1f, 1f, 1f);
            floor.transform.SetParent(parent);
            floor.name = $"Floor_{floorPos.x}_{floorPos.y}";
            
            // FIX: Use sharedMaterial to avoid material leaks
            Renderer renderer = floor.GetComponent<Renderer>();
            if (Application.isPlaying)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.white;
            }
            else
            {
                Material sharedMaterial = new(Shader.Find("Standard"));
                sharedMaterial.color = Color.white;
                renderer.sharedMaterial = sharedMaterial;
            }
            
            // Add collision
            floor.AddComponent<BoxCollider>();
        }
        
        Debug.Log($"Created {layout.AllFloorTiles.Count} primitive floors as fallback");
    }

    public void RenderFloorsByRoom(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        if (_currentBiome == null || layout?.AllFloorTiles == null || rooms == null)
        {
            Debug.LogError("Cannot render floors by room: missing data");
            return;
        }

        var floorPrefab = _biomeManager.GetFloorPrefab(_currentBiome);
        if (floorPrefab == null)
        {
            Debug.LogError("Floor prefab is null!");
            RenderFloorsAsPrimitives(layout, parent);
            return;
        }

        Mesh floorMesh = GetPrefabMesh(floorPrefab);
        Material floorMaterial = GetPrefabMaterial(floorPrefab);
        Vector3 floorScale = floorPrefab.transform.localScale;

        if (floorMesh == null || floorMaterial == null)
        {
            Debug.LogError("Could not get floor mesh or material");
            return;
        }

        // Group floor tiles by room
        Dictionary<RoomModel, List<Vector2Int>> roomFloorTiles = new();
        
        // Initialize dictionary with all rooms
        foreach (var room in rooms)
        {
            roomFloorTiles[room] = new List<Vector2Int>();
        }
        
        // Add corridor tiles to a separate group
        List<Vector2Int> corridorTiles = new();

        // Assign each floor tile to its room
        foreach (var floorPos in layout.AllFloorTiles)
        {
            RoomModel containingRoom = null;
            foreach (var room in rooms)
            {
                if (room.Bounds.Contains(floorPos))
                {
                    containingRoom = room;
                    break;
                }
            }
            
            if (containingRoom != null)
            {
                roomFloorTiles[containingRoom].Add(floorPos);
            }
            else
            {
                corridorTiles.Add(floorPos);
            }
        }

        // Create combined mesh for each room
        foreach (var roomGroup in roomFloorTiles)
        {
            if (roomGroup.Value.Count > 0)
            {
                var roomCombiner = new AdvancedMeshCombiner();
                
                foreach (var floorPos in roomGroup.Value)
                {
                    Vector3 worldPos = new(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
                    roomCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
                }
                
                var roomMeshObjects = roomCombiner.BuildAllCombinedMeshes(parent);
                foreach (var meshObj in roomMeshObjects)
                {
                    meshObj.name = $"Room_{roomGroup.Key.ID}_{roomGroup.Key.Type}_Floors";
                }
            }
        }

        // Create combined mesh for corridors
        if (corridorTiles.Count > 0)
        {
            var corridorCombiner = new AdvancedMeshCombiner();
            
            foreach (var floorPos in corridorTiles)
            {
                Vector3 worldPos = new(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
                corridorCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
            }
            
            var corridorMeshObjects = corridorCombiner.BuildAllCombinedMeshes(parent);
            foreach (var meshObj in corridorMeshObjects)
            {
                meshObj.name = "Corridor_Floors";
            }
        }

        Debug.Log($"Rendered floors by room: {roomFloorTiles.Count} rooms, {corridorTiles.Count} corridor tiles");
    }
    #endregion
}