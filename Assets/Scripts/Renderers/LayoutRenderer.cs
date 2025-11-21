// LayoutRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders all structural layout elements: Floors, Walls, Doors, Ceiling, and Void Plane.
/// Uses mesh combining for optimal performance and throws errors for missing assets.
/// </summary>
public class LayoutRenderer
{
    private BiomeManager _biomeManager;
    private MeshCombiner _meshCombiner;

    public LayoutRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        _meshCombiner = new MeshCombiner();
    }

    /// <summary>
    /// Renders the complete layout including floors, walls, doors, ceiling and void plane.
    /// </summary>
    public void RenderCompleteLayout(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout == null) throw new System.ArgumentNullException(nameof(layout));
        if (biome == null) throw new System.ArgumentNullException(nameof(biome));
        if (parent == null) throw new System.ArgumentNullException(nameof(parent));

        Debug.Log($"Starting complete layout rendering for biome: {biome.Name}");

        RenderFloors(layout, biome, parent);
        RenderWalls(layout, biome, parent);
        RenderDoors(layout, biome, parent);
        RenderCeiling(layout, biome, parent);
        RenderVoidPlane(layout, biome, parent);

        // Finalize all combined meshes
        _meshCombiner.FinalizeRendering(parent);
        
        Debug.Log("Complete layout rendering finished");
    }

    /// <summary>
    /// Renders all floor tiles using combined meshes for optimal performance.
    /// </summary>
    public void RenderFloors(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.AllFloorTiles == null) 
            throw new System.ArgumentNullException("Layout or AllFloorTiles is null");

        var floorPrefab = _biomeManager.GetFloorPrefab(biome);
        if (floorPrefab == null)
            throw new System.MissingReferenceException($"Floor prefab not found for biome: {biome.Name}");

        Mesh floorMesh = GetPrefabMesh(floorPrefab);
        Material floorMaterial = GetPrefabMaterial(floorPrefab);
        Vector3 floorScale = floorPrefab.transform.localScale;

        if (floorMesh == null)
            throw new System.MissingReferenceException($"Mesh not found on floor prefab for biome: {biome.Name}");

        if (floorMaterial == null)
            throw new System.MissingReferenceException($"Material not found on floor prefab for biome: {biome.Name}");

        int floorsProcessed = 0;
        foreach (var floorPos in layout.AllFloorTiles)
        {
            Vector3 worldPos = new Vector3(floorPos.x + 0.5f, 0f, floorPos.y + 0.5f);
            _meshCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
            floorsProcessed++;
        }

        Debug.Log($"Processed {floorsProcessed} floor positions for combining");
    }

    /// <summary>
    /// Renders all wall tiles using combined meshes with proper orientation.
    /// </summary>
    public void RenderWalls(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null)
            throw new System.ArgumentNullException("Layout wall data is null");

        var wallPrefab = _biomeManager.GetWallPrefab(biome);
        if (wallPrefab == null)
            throw new System.MissingReferenceException($"Wall prefab not found for biome: {biome.Name}");

        Mesh wallMesh = GetPrefabMesh(wallPrefab);
        Material wallMaterial = GetPrefabMaterial(wallPrefab);
        Vector3 wallScale = wallPrefab.transform.localScale;

        if (wallMesh == null)
            throw new System.MissingReferenceException($"Mesh not found on wall prefab for biome: {biome.Name}");

        if (wallMaterial == null)
            throw new System.MissingReferenceException($"Material not found on wall prefab for biome: {biome.Name}");

        int wallsProcessed = 0;
        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                Vector3 worldPos = new Vector3(wallPos.x + 0.5f, 1f, wallPos.y + 0.5f);
                Quaternion rotation = GetWallRotation(wallType);
                _meshCombiner.AddMesh(wallMesh, worldPos, rotation, wallScale, wallMaterial);
                wallsProcessed++;
            }
        }

        Debug.Log($"Processed {wallsProcessed} wall positions for combining");
    }

    /// <summary>
    /// Renders all door tiles as individual prefab instances (not combined).
    /// </summary>
    public void RenderDoors(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.AllDoorTiles == null)
            throw new System.ArgumentNullException("Layout or AllDoorTiles is null");

        var doorPrefab = _biomeManager.GetDoorPrefab(biome);
        if (doorPrefab == null)
            throw new System.MissingReferenceException($"Door prefab not found for biome: {biome.Name}");

        int doorsCreated = 0;
        foreach (var doorPos in layout.AllDoorTiles)
        {
            Vector3 worldPos = new Vector3(doorPos.x + 0.5f, 0f, doorPos.y + 0.5f);
            Quaternion rotation = GetDoorRotation(layout, doorPos);
            
            var door = GameObject.Instantiate(doorPrefab, worldPos, rotation, parent);
            door.name = $"Door_{doorPos.x}_{doorPos.y}";
            
            // Ensure DoorController is properly configured
            var doorController = door.GetComponent<DoorController>();
            if (doorController == null)
            {
                doorController = door.AddComponent<DoorController>();
            }

            doorsCreated++;
        }

        Debug.Log($"Created {doorsCreated} door instances");
    }

    /// <summary>
    /// Renders a ceiling plane above the entire dungeon.
    /// </summary>
    public void RenderCeiling(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.OverallBounds == null)
            throw new System.ArgumentNullException("Layout or OverallBounds is null");

        var ceilingPrefab = _biomeManager.GetCeilingPrefab(biome);
        if (ceilingPrefab != null)
        {
            // Use biome-specific ceiling prefab
            BoundsInt dungeonBounds = layout.OverallBounds;
            Vector3 center = new Vector3(dungeonBounds.center.x, 9f, dungeonBounds.center.y);
            var ceiling = GameObject.Instantiate(ceilingPrefab, center, Quaternion.identity, parent);
            ceiling.name = "Ceiling";
            
            // Scale to cover entire dungeon
            float scaleX = Mathf.Ceil(dungeonBounds.size.x);
            float scaleZ = Mathf.Ceil(dungeonBounds.size.y);
            ceiling.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        }
        else
        {
            // Fallback to procedural ceiling (one-sided mirror)
            CreateProceduralCeiling(layout, parent);
        }
    }

    /// <summary>
    /// Renders a void plane below the dungeon to catch falling objects.
    /// </summary>
    public void RenderVoidPlane(LevelModel layout, BiomeModel biome, Transform parent)
    {
        if (layout?.OverallBounds == null)
            throw new System.ArgumentNullException("Layout or OverallBounds is null");

        BoundsInt dungeonBounds = layout.OverallBounds;
        Vector3 center = new Vector3(dungeonBounds.center.x, -5f, dungeonBounds.center.y);
        
        GameObject voidPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        voidPlane.name = "VoidPlane";
        voidPlane.transform.SetParent(parent);
        voidPlane.transform.position = center;
        
        // Scale to cover entire dungeon + large buffer
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.2f);
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.2f);
        voidPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        // Apply void material
        Renderer renderer = voidPlane.GetComponent<Renderer>();
        Material voidMaterial = new Material(Shader.Find("Standard"));
        voidMaterial.color = Color.black;
        voidMaterial.SetFloat("_Metallic", 0f);
        voidMaterial.SetFloat("_Glossiness", 0f);
        renderer.sharedMaterial = voidMaterial;

        // Remove collider - void shouldn't block movement
        GameObject.DestroyImmediate(voidPlane.GetComponent<Collider>());
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

    private Quaternion GetDoorRotation(LevelModel layout, Vector2Int doorPos)
    {
        // Determine door orientation based on adjacent walls/rooms
        var adjacentRoom = FindAdjacentRoom(layout, doorPos);
        if (adjacentRoom != null)
        {
            return GetDoorRotationFromRoom(adjacentRoom, doorPos);
        }

        // Fallback: analyze adjacent tiles to guess orientation
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // North
            new Vector2Int(0, -1),  // South  
            new Vector2Int(1, 0),   // East
            new Vector2Int(-1, 0)   // West
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = doorPos + dir;
            bool isRoom = layout.GetRoomAtPosition(checkPos) != null;
            
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
        if (direction == new Vector2Int(0, 1) || direction == new Vector2Int(0, -1)) 
            return Quaternion.Euler(0, 90, 0);   // North/South walls - face east/west
        else 
            return Quaternion.Euler(0, 0, 0);    // East/West walls - face north/south
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
        
        if (doorPos.y == bounds.yMax - 1 || doorPos.y == bounds.yMin) 
            return Quaternion.Euler(0, 90, 0);   // North/South wall - face east/west
        else 
            return Quaternion.Euler(0, 0, 0);    // East/West wall - face north/south
    }

    private void CreateProceduralCeiling(LevelModel layout, Transform parent)
    {
        BoundsInt dungeonBounds = layout.OverallBounds;
        
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(parent);
        
        Vector3 center = new Vector3(dungeonBounds.center.x, 9f, dungeonBounds.center.y);
        ceiling.transform.position = center;
        
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.1f);
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.1f);
        ceiling.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        // Apply one-sided mirror material
        Renderer renderer = ceiling.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateOneSidedMirrorMaterial();
        
        // Remove collider - ceiling shouldn't block movement
        GameObject.DestroyImmediate(ceiling.GetComponent<Collider>());
    }

    private Material CreateOneSidedMirrorMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        
        #if UNITY_EDITOR
        // Transparent cyan in editor so you can see through it
        material.color = new Color(0, 1, 1, 0.3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        #else
        // Opaque at runtime
        material.color = Color.gray;
        material.SetFloat("_Metallic", 0.1f);
        material.SetFloat("_Glossiness", 0.1f);
        #endif

        return material;
    }

    #endregion
}