// OptimizedPrefabRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-performance prefab renderer that uses mesh combining for optimal rendering.
/// Supports biome themes and efficient geometry batching.
/// </summary>
public class OptimizedPrefabRenderer
{
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;
    private int _currentFloor;
    private AdvancedMeshCombiner _meshCombiner;

    public OptimizedPrefabRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        _meshCombiner = new AdvancedMeshCombiner();
    }

    public void SetThemeForFloor(int floorLevel, int seed)
    {
        _currentFloor = floorLevel;
        _currentTheme = _biomeManager.GetThemeForFloor(floorLevel, seed);
        Debug.Log($"Using theme: {_currentTheme?.Name} for floor {floorLevel}");
    }

    /// <summary>
    /// Renders all floor tiles using combined meshes for optimal performance.
    /// </summary>
    public void RenderFloorsOptimized(LevelModel layout, Transform parent)
    {
        if (_currentTheme == null)
        {
            Debug.LogError("Current theme is null in RenderFloorsOptimized!");
            return;
        }

        if (layout?.AllFloorTiles == null)
        {
            Debug.LogError("Layout or AllFloorTiles is null in RenderFloorsOptimized!");
            return;
        }

        Debug.Log($"=== FLOOR RENDERING DEBUG ===");
        Debug.Log($"Theme: {_currentTheme.Name}");
        Debug.Log($"Floor prefab path: {_currentTheme.FloorPrefabPath}");
        Debug.Log($"Floor tiles count: {layout.AllFloorTiles.Count}");

        var floorPrefab = _biomeManager.GetFloorPrefab(_currentTheme);

        if (floorPrefab == null)
        {
            Debug.LogError($"Floor prefab is NULL for theme: {_currentTheme.Name}");
            Debug.LogError($"Tried to load from: {_currentTheme.FloorPrefabPath}");
            
            // Test if Resources loading works at all
            var testPrefab = Resources.Load<GameObject>("Themes/Default/FloorPrefab");
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
            Vector3 worldPos = new Vector3(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
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
        if (_currentTheme == null || layout?.AllWallTiles == null) return;

        var wallPrefab = _biomeManager.GetWallPrefab(_currentTheme);

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
                Vector3 worldPos = new Vector3(wallPos.x + 0.5f, 4.5f, wallPos.y + 0.5f);
                Quaternion rotation = GetWallRotation(wallType);
                _meshCombiner.AddMesh(wallMesh, worldPos, rotation, wallScale, wallMaterial);
            }
        }
    }

    /// <summary>
    /// Renders all door tiles and door tops using combined meshes.
    /// </summary>
    public void RenderDoorsOptimized(LevelModel layout, Transform parent)
    {
        if (_currentTheme == null || layout?.AllDoorTiles == null) return;

        // Doors
        var doorPrefab = _biomeManager.GetDoorPrefab(_currentTheme);
        if (doorPrefab != null)
        {
            Mesh doorMesh = GetPrefabMesh(doorPrefab);
            Material doorMaterial = GetPrefabMaterial(doorPrefab);
            Vector3 doorScale = doorPrefab.transform.localScale;

            if (doorMesh != null && doorMaterial != null)
            {
                foreach (var doorPos in layout.AllDoorTiles)
                {
                    Vector3 worldPos = new Vector3(doorPos.x + 0.5f, 1f, doorPos.y + 0.5f);
                    Quaternion rotation = GetDoorRotation(layout, doorPos);
                    _meshCombiner.AddMesh(doorMesh, worldPos, rotation, doorScale, doorMaterial);
                }
            }
        }

        // Door tops
        var doorTopPrefab = _biomeManager.GetDoorTopPrefab(_currentTheme);
        if (doorTopPrefab != null)
        {
            Mesh doorTopMesh = GetPrefabMesh(doorTopPrefab);
            Material doorTopMaterial = GetPrefabMaterial(doorTopPrefab);
            Vector3 doorTopScale = doorTopPrefab.transform.localScale;

            if (doorTopMesh != null && doorTopMaterial != null)
            {
                foreach (var doorPos in layout.AllDoorTiles)
                {
                    Vector3 worldPos = new Vector3(doorPos.x + 0.5f, 6f, doorPos.y + 0.5f);
                    _meshCombiner.AddMesh(doorTopMesh, worldPos, Quaternion.identity, doorTopScale, doorTopMaterial);
                }
            }
        }
    }

    /// <summary>
    /// Finalizes all combined meshes and builds them in the scene.
    /// </summary>
    public void FinalizeRendering(Transform parent)
    {
        // Build all combined meshes at once
        var combinedObjects = _meshCombiner.BuildAllCombinedMeshes(parent);
        Debug.Log($"Finalized rendering with {combinedObjects.Count} combined mesh objects");
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
        Vector3 center = new Vector3(dungeonBounds.center.x, 9f, dungeonBounds.center.y);
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
        Vector3 center = new Vector3(dungeonBounds.center.x, -5f, dungeonBounds.center.y);
        voidPlane.transform.position = center;
        
        // Scale to cover entire dungeon + large buffer
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.2f); 
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.2f);
        voidPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        // Pure black material
        Renderer renderer = voidPlane.GetComponent<Renderer>();
        Material voidMaterial = new Material(Shader.Find("Standard"));
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
        // Check adjacent tiles to determine which way the door should face
        // Door should face INTO the room (away from corridors)
        
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // North
            new Vector2Int(0, -1), // South  
            new Vector2Int(1, 0),  // East
            new Vector2Int(-1, 0)  // West
        };

        // Check which adjacent positions are rooms vs corridors
        foreach (var dir in directions)
        {
            Vector2Int checkPos = doorPos + dir;
            Vector2Int oppositePos = doorPos - dir;
            
            bool checkIsRoom = layout.GetRoomAtPosition(checkPos) != null;
            bool oppositeIsRoom = layout.GetRoomAtPosition(oppositePos) != null;
            
            // If one side is room and other side is corridor, face into the room
            if (checkIsRoom && !oppositeIsRoom)
            {
                // Face away from the room (toward corridor)
                return GetRotationFromDirection(-dir);
            }
            else if (!checkIsRoom && oppositeIsRoom)
            {
                // Face toward the room (away from corridor)
                return GetRotationFromDirection(dir);
            }
        }

        // Fallback: Check if door is on room perimeter
        var room = FindAdjacentRoom(layout, doorPos);
        if (room != null)
        {
            // Determine which wall the door is on and face inward
            return GetDoorRotationFromRoom(room, doorPos);
        }

        // Ultimate fallback: face north
        return Quaternion.Euler(0, 0, 0);
    }

    private Quaternion GetRotationFromDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return Quaternion.Euler(0, 0, 0);    // North
        if (direction == new Vector2Int(0, -1)) return Quaternion.Euler(0, 180, 0);  // South
        if (direction == new Vector2Int(1, 0)) return Quaternion.Euler(0, 90, 0);    // East  
        if (direction == new Vector2Int(-1, 0)) return Quaternion.Euler(0, 270, 0);  // West
        
        return Quaternion.identity;
    }

    private RoomModel FindAdjacentRoom(LevelModel layout, Vector2Int doorPos)
    {
        Vector2Int[] checkDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
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
        
        // Determine which wall the door is on and face inward
        if (doorPos.y == bounds.yMax - 1) return Quaternion.Euler(0, 180, 0); // North wall - face south
        if (doorPos.y == bounds.yMin) return Quaternion.Euler(0, 0, 0);       // South wall - face north
        if (doorPos.x == bounds.xMax - 1) return Quaternion.Euler(0, 270, 0); // East wall - face west  
        if (doorPos.x == bounds.xMin) return Quaternion.Euler(0, 90, 0);      // West wall - face east
        
        return Quaternion.Euler(0, 0, 0);
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
        Material material = new Material(Shader.Find("Standard"));
        
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
                Material sharedMaterial = new Material(Shader.Find("Standard"));
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
                Material sharedMaterial = new Material(Shader.Find("Standard"));
                sharedMaterial.color = Color.white;
                renderer.sharedMaterial = sharedMaterial;
            }
            
            // Add collision
            floor.AddComponent<BoxCollider>();
        }
        
        Debug.Log($"Created {layout.AllFloorTiles.Count} primitive floors as fallback");
    }
    #endregion
}