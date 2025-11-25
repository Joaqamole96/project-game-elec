// -------------------------------------------------- //
// Scripts/Renderers/OptimizedPrefabRenderer.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class OptimizedPrefabRenderer
{
    private BiomeManager _biomeManager;
    private string _currentBiome;
    private MeshGenerator _meshCombiner;
    private MeshGenerator _wallCombiner;

    public OptimizedPrefabRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
        _meshCombiner = new();
        _wallCombiner = new();
    }

    public void SetBiomeForFloor(int floorLevel)
    {
        _currentBiome = _biomeManager.GetBiomeForFloor(floorLevel);
        Debug.Log($"OptimizedRenderer: Using biome '{_currentBiome}' for floor {floorLevel}");
    }

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
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                Vector3 worldPos = new(wallPos.x + 0.5f, 4.5f, wallPos.y + 0.5f);
                Quaternion rotation = GetWallRotation(wallType);
                _wallCombiner.AddMesh(wallMesh, worldPos, rotation, wallScale, wallMaterial);
            }
    }

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
                
                var door = Object.Instantiate(doorPrefab, worldPos, rotation, parent);
                door.name = $"Door_{doorPos.x}_{doorPos.y}";
                
                // Ensure DoorController is properly configured
                if (!door.TryGetComponent<DoorController>(out var doorController))
                {
                    doorController = door.AddComponent<DoorController>();
                }
            }
        }

        // Render door tops
        var doorTopPrefab = _biomeManager.GetDoorTopPrefab(_currentBiome);
        if (doorTopPrefab != null)
        {
            foreach (var doorPos in layout.AllDoorTiles)
            {
                Vector3 topPos = new(doorPos.x + 0.5f, 6f, doorPos.y + 0.5f);
                var doorTop = Object.Instantiate(doorTopPrefab, topPos, Quaternion.identity, parent);
                doorTop.name = $"DoorTop_{doorPos.x}_{doorPos.y}";
            }
        }
        else
        {
            RenderDoorTopsAsPrimitives(layout, parent);
        }
    }

    private void RenderDoorTopsAsPrimitives(LevelModel layout, Transform parent)
    {
        foreach (var doorPos in layout.AllDoorTiles)
        {
            GameObject doorTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorTop.transform.position = new Vector3(doorPos.x + 0.5f, 2.5f, doorPos.y + 0.5f);
            doorTop.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            doorTop.transform.SetParent(parent);
            doorTop.name = $"DoorTop_{doorPos.x}_{doorPos.y}";
            
            Renderer renderer = doorTop.GetComponent<Renderer>();
            Material material = new(Shader.Find("Standard"))
            {
                color = Color.gray
            };
            renderer.sharedMaterial = material;
        }
    }

    public void FinalizeRendering(Transform floorParent, Transform wallParent)
    {
        var floorObjects = _meshCombiner.BuildAllCombinedMeshes(floorParent);
        var wallObjects = _wallCombiner.BuildAllCombinedMeshes(wallParent);
        
        Debug.Log($"Finalized rendering: {floorObjects.Count} floor meshes, {wallObjects.Count} wall meshes");
    }

    public void RenderCeilingOptimized(LevelModel layout, Transform parent)
    {
        if (layout?.OverallBounds == null) return;

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "MainCeiling";
        ceiling.transform.SetParent(parent);
        
        BoundsInt dungeonBounds = layout.OverallBounds;
        Vector3 center = new(dungeonBounds.center.x, 9f, dungeonBounds.center.y);
        ceiling.transform.position = center;
        
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.1f); 
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.1f);
        ceiling.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        Renderer renderer = ceiling.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateCeilingMaterial();

        Object.DestroyImmediate(ceiling.GetComponent<Collider>());
    }

    public void RenderVoidPlane(LevelModel layout, Transform parent)
    {
        if (layout?.OverallBounds == null) return;

        GameObject voidPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        voidPlane.name = "VoidPlane";
        voidPlane.transform.SetParent(parent);
        
        BoundsInt dungeonBounds = layout.OverallBounds;
        Vector3 center = new(dungeonBounds.center.x, -5f, dungeonBounds.center.y);
        voidPlane.transform.position = center;
        
        float scaleX = Mathf.Ceil(dungeonBounds.size.x * 0.2f); 
        float scaleZ = Mathf.Ceil(dungeonBounds.size.y * 0.2f);
        voidPlane.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
        
        Renderer renderer = voidPlane.GetComponent<Renderer>();
        Material voidMaterial = new(Shader.Find("Standard"))
        {
            color = Color.black
        };
        voidMaterial.SetFloat("_Metallic", 0f);
        voidMaterial.SetFloat("_Glossiness", 0f);
        renderer.sharedMaterial = voidMaterial;
    }

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
        var adjacentRoom = FindAdjacentRoom(layout, doorPos);
        if (adjacentRoom != null) return GetDoorRotationFromRoom(adjacentRoom, doorPos);

        Vector2Int[] directions = new Vector2Int[]
        {
            new(0, 1), new(0, -1), new(1, 0), new(-1, 0)
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = doorPos + dir;
            bool isRoom = layout.GetRoomAtPosition(checkPos) != null;
            
            if (isRoom) return GetRotationFromDirection(-dir);
        }

        return Quaternion.Euler(0, 0, 0);
    }

    private Quaternion GetRotationFromDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1) || direction == new Vector2Int(0, -1))
            return Quaternion.Euler(0, 90, 0);
        return Quaternion.Euler(0, 0, 0);
    }

    private RoomModel FindAdjacentRoom(LevelModel layout, Vector2Int doorPos)
    {
        Vector2Int[] checkDirections = new Vector2Int[]
        {
            new(0, 1), new(0, -1), new(1, 0), new(-1, 0)
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
            return Quaternion.Euler(0, 90, 0);
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

    private Material CreateCeilingMaterial()
    {
        Material material = new(Shader.Find("Standard"))
        {
            color = new Color(0.5f, 0.5f, 0.5f)
        };
        material.SetFloat("_Metallic", 0.1f);
        material.SetFloat("_Glossiness", 0.1f);
        return material;
    }

    private void RenderWallsAsPrimitives(LevelModel layout, Transform parent)
    {
        foreach (var wallPos in layout.AllWallTiles)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(wallPos.x + 0.5f, 4.5f, wallPos.y + 0.5f);
            wall.transform.localScale = new Vector3(1f, 8f, 1f);
            wall.transform.SetParent(parent);
            wall.name = $"Wall_{wallPos.x}_{wallPos.y}";
            
            Renderer renderer = wall.GetComponent<Renderer>();
            Material material = new(Shader.Find("Standard"))
            {
                color = Color.gray
            };
            renderer.sharedMaterial = material;
        }
    }

    public void RenderFloorsByRoom(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        if (_currentBiome == null || layout?.AllFloorTiles == null || rooms == null)
        {
            Debug.LogError("Cannot render floors: missing data");
            return;
        }

        var floorPrefab = _biomeManager.GetFloorPrefab(_currentBiome);
        if (floorPrefab == null)
        {
            Debug.LogWarning("Floor prefab not found, using primitives");
            RenderFloorsAsPrimitives(layout, parent);
            return;
        }

        Mesh floorMesh = GetPrefabMesh(floorPrefab);
        Material floorMaterial = GetPrefabMaterial(floorPrefab);
        Vector3 floorScale = floorPrefab.transform.localScale;

        if (floorMesh == null || floorMaterial == null)
        {
            Debug.LogWarning("Floor mesh/material not found, using primitives");
            RenderFloorsAsPrimitives(layout, parent);
            return;
        }

        Dictionary<RoomModel, List<Vector2Int>> roomFloorTiles = new();
        foreach (var room in rooms) roomFloorTiles[room] = new List<Vector2Int>();
        List<Vector2Int> corridorTiles = new();

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
                roomFloorTiles[containingRoom].Add(floorPos);
            else
                corridorTiles.Add(floorPos);
        }

        foreach (var roomGroup in roomFloorTiles)
        {
            if (roomGroup.Value.Count > 0)
            {
                var roomCombiner = new MeshGenerator();
                
                foreach (var floorPos in roomGroup.Value)
                {
                    Vector3 worldPos = new(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
                    roomCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
                }
                
                var roomMeshObjects = roomCombiner.BuildAllCombinedMeshes(parent);
                foreach (var meshObj in roomMeshObjects)
                    meshObj.name = $"Room_{roomGroup.Key.ID}_{roomGroup.Key.Type}_Floors";
            }
        }

        if (corridorTiles.Count > 0)
        {
            var corridorCombiner = new MeshGenerator();
            
            foreach (var floorPos in corridorTiles)
            {
                Vector3 worldPos = new(floorPos.x + 0.5f, 0.5f, floorPos.y + 0.5f);
                corridorCombiner.AddMesh(floorMesh, worldPos, Quaternion.identity, floorScale, floorMaterial);
            }
            
            var corridorMeshObjects = corridorCombiner.BuildAllCombinedMeshes(parent);
            foreach (var meshObj in corridorMeshObjects)
                meshObj.name = "Corridor_Floors";
        }

        Debug.Log($"Rendered floors: {roomFloorTiles.Count} rooms, {corridorTiles.Count} corridor tiles");
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
            
            Renderer renderer = floor.GetComponent<Renderer>();
            Material material = new(Shader.Find("Standard"))
            {
                color = Color.white
            };
            renderer.sharedMaterial = material;
            
            floor.AddComponent<BoxCollider>();
        }
        
        Debug.Log($"Created {layout.AllFloorTiles.Count} primitive floors");
    }
}