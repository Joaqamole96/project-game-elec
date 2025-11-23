// -------------------------------------------------- //
// Scripts/Renderers/LandmarkRenderer.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class LandmarkRenderer
{
    private GameObject _defaultEntrancePrefab;
    private GameObject _defaultExitPrefab;
    private BiomeManager _biomeManager;

    public LandmarkRenderer(GameObject entrancePrefab, GameObject exitPrefab, BiomeManager biomeManager)
    {
        _defaultEntrancePrefab = entrancePrefab;
        _defaultExitPrefab = exitPrefab;
        _biomeManager = biomeManager;
    }
    
    public void RenderLandmarks(List<RoomModel> rooms, Transform parent)
    {
        if (rooms == null || parent == null) throw new("Cannot render special objects: rooms or parent is null");

        int landmarksCreated = 0;
        foreach (var room in rooms)
            if (room != null && IsSpecialRoomType(room.Type) && RenderRoomLandmark(room, parent)) landmarksCreated++;

        Debug.Log($"Created {landmarksCreated} special room objects");
    }

    private bool IsSpecialRoomType(RoomType roomType)
    {
        return roomType == RoomType.Entrance || roomType == RoomType.Exit || 
               roomType == RoomType.Shop || roomType == RoomType.Treasure || 
               roomType == RoomType.Boss;
    }

    private bool RenderRoomLandmark(RoomModel room, Transform parent)
    {
        GameObject prefab = GetSpecialRoomPrefab(room.Type);

        if (prefab == null) prefab = GetDefaultSpecialRoomPrefab(room.Type);
        if (prefab != null)
        {
            Bounds prefabBounds = GetPrefabBounds(prefab);
            float floorHeight = 1f;
            float objectHeight = prefabBounds.size.y;
            
            Vector3 position = new(room.Center.x, floorHeight + (objectHeight * 0.5f), room.Center.y);
            var landmark = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            landmark.name = $"{room.Type}_{room.ID}";
            return true;
        }
        else
        {
            Debug.LogWarning($"No prefab available for {room.Type} room {room.ID}");
            return false;
        }
    }

    private Bounds GetPrefabBounds(GameObject prefab)
    {
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null) return renderer.bounds;
        return new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f));
    }

    private GameObject GetSpecialRoomPrefab(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Entrance => _biomeManager.GetPrefab("Landmarks/EntrancePrefab"),
            RoomType.Exit => _biomeManager.GetPrefab("Landmarks/ExitPrefab"),
            RoomType.Shop => _biomeManager.GetPrefab("Landmarks/ShopPrefab"),
            RoomType.Treasure => _biomeManager.GetPrefab("Landmarks/TreasurePrefab"),
            _ => null
        };
    }

    private GameObject GetDefaultSpecialRoomPrefab(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Entrance => _defaultEntrancePrefab,
            RoomType.Exit => _defaultExitPrefab,
            _ => null
        };
    }
}