// GizmoFloorRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders floors in Gizmo mode using colored cubes for debugging and visualization.
/// </summary>
public class GizmoFloorRenderer : IFloorRenderer
{
    private MaterialManager _materialManager;
    private MeshCombiner _meshCombiner;

    public GizmoFloorRenderer(MaterialManager materialManager)
    {
        _materialManager = materialManager;
        _meshCombiner = new MeshCombiner();
    }

    /// <summary>
    /// Renders floors as combined meshes grouped by room type for optimal performance.
    /// </summary>
    public List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        if (layout?.AllFloorTiles == null || rooms == null)
        {
            Debug.LogError("Cannot render combined floors: layout or rooms is null");
            return new List<GameObject>();
        }

        var floorGroups = GroupFloorTilesByRoomType(layout, rooms);
        var meshContainers = new List<GameObject>();

        Debug.Log($"Combining floors by room type: {floorGroups.Count} room types found");
        
        foreach (var floorGroup in floorGroups)
        {
            if (floorGroup.Value.Count > 0)
            {
                Debug.Log($"Creating combined mesh for {floorGroup.Key}: {floorGroup.Value.Count} tiles");
                var combinedMesh = _meshCombiner.CreateCombinedMesh(floorGroup.Value, $"Floors_{floorGroup.Key}", parent);
                if (combinedMesh != null)
                {
                    ApplyRoomMaterial(combinedMesh, floorGroup.Key);
                    meshContainers.Add(combinedMesh);
                    Debug.Log($"Successfully created combined mesh for {floorGroup.Key}");
                }
                else
                {
                    Debug.LogError($"Failed to create combined mesh for {floorGroup.Key}");
                }
            }
            else
            {
                Debug.LogWarning($"No floor tiles found for room type: {floorGroup.Key}");
            }
        }

        Debug.Log($"Created {meshContainers.Count} combined floor meshes");
        return meshContainers;
    }

    /// <summary>
    /// Renders floors as individual cubes for detailed debugging.
    /// </summary>
    public void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision)
    {
        if (layout?.AllFloorTiles == null || rooms == null)
        {
            Debug.LogError("Cannot render individual floors: layout or rooms is null");
            return;
        }

        foreach (var floorPos in layout.AllFloorTiles)
        {
            var floor = CreateFloorAtPosition(floorPos);
            if (floor != null)
            {
                floor.transform.SetParent(parent);
                
                var roomType = GetRoomTypeAtPosition(floorPos, rooms, layout);
                ApplyRoomMaterial(floor, roomType);
                
                if (enableCollision)
                    AddCollisionToObject(floor, "Floor");
            }
        }
    }

    private Dictionary<RoomType, List<Vector3>> GroupFloorTilesByRoomType(LevelModel layout, List<RoomModel> rooms)
    {
        var floorGroups = new Dictionary<RoomType, List<Vector3>>();
        
        foreach (var floorPos in layout.AllFloorTiles)
        {
            var roomType = GetRoomTypeAtPosition(floorPos, rooms, layout);
            
            if (!floorGroups.ContainsKey(roomType))
                floorGroups[roomType] = new List<Vector3>();
                
            floorGroups[roomType].Add(new Vector3(floorPos.x + 0.5f, 0, floorPos.y + 0.5f));
        }

        return floorGroups;
    }

    private RoomType GetRoomTypeAtPosition(Vector2Int position, List<RoomModel> rooms, LevelModel layout)
    {
        if (layout == null || rooms == null) return RoomType.Combat;
        
        var room = layout.GetRoomAtPosition(position);
        return room?.Type ?? RoomType.Combat;
    }

    private GameObject CreateFloorAtPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0, gridPos.y + 0.5f);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = worldPos;
        floor.transform.localScale = new Vector3(1f, 0.1f, 1f);
        floor.name = $"Floor_{gridPos.x}_{gridPos.y}";
        return floor;
    }

    private void ApplyRoomMaterial(GameObject obj, RoomType roomType)
    {
        if (obj == null) return;
        
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = _materialManager.GetRoomMaterial(roomType);
        }
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}