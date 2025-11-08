using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrefabFloorRenderer : IFloorRenderer
{
    private GameObject _floorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;

    public PrefabFloorRenderer(GameObject floorPrefab, MaterialManager materialManager)
    {
        _floorPrefab = floorPrefab;
        _materialManager = materialManager;
        _biomeManager = new BiomeManager();
    }

    public List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        RenderIndividualFloors(layout, rooms, parent, false);
        return new List<GameObject>();
    }

    public void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision)
    {
        if (layout?.AllFloorTiles == null)
        {
            Debug.LogError("Cannot render floors - AllFloorTiles is null!");
            return;
        }

        int floorsRendered = 0;
        int floorsSkipped = 0;

        foreach (var floorPos in layout.AllFloorTiles)
        {
            try
            {
                var roomType = GetRoomTypeAtPosition(floorPos, rooms, layout);
                var floorPrefab = _biomeManager.GetFloorPrefab(roomType, floorPos);
                
                var floor = CreateFloorAtPosition(floorPos, floorPrefab);
                if (floor != null)
                {
                    floor.transform.SetParent(parent);
                    floorsRendered++;
                    
                    if (enableCollision)
                        AddCollisionToObject(floor, "Floor");
                }
                else
                {
                    floorsSkipped++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error rendering floor at {floorPos}: {ex.Message}");
                floorsSkipped++;
            }
        }

        Debug.Log($"Floor rendering: {floorsRendered} rendered, {floorsSkipped} skipped");
    }

    private GameObject CreateFloorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        // FIX: Set Y position to 0 for floors
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f); // Changed Y from 0.5f to 0f
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject floorPrefabToUse = prefab ?? _floorPrefab;
        
        var floor = Object.Instantiate(floorPrefabToUse, worldPos, Quaternion.identity);
        floor.name = $"Floor_{gridPos.x}_{gridPos.y}";
        
        // FIX: Ensure the floor is at the correct height after instantiation
        floor.transform.position = new Vector3(floor.transform.position.x, 0f, floor.transform.position.z);
        
        return floor;
    }

    private RoomType GetRoomTypeAtPosition(Vector2Int position, List<RoomModel> rooms, LevelModel layout)
    {
        var room = layout.GetRoomAtPosition(position);
        return room?.Type ?? RoomType.Combat;
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}