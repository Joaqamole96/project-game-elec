// -------------------------------------------------- //
// Scripts/Renderers/LandmarkRenderer.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class LandmarkRenderer
{
    private BiomeManager _biomeManager;

    public LandmarkRenderer(GameObject entrancePrefab, GameObject exitPrefab, BiomeManager biomeManager)
    {
        // Legacy constructor - prefabs now loaded from Resources
        _biomeManager = biomeManager;
    }
    
    public void RenderLandmarks(List<RoomModel> rooms, Transform parent)
    {
        if (rooms == null || parent == null) throw new System.Exception("Cannot render special objects: rooms or parent is null");
        int landmarksCreated = 0;
        foreach (var room in rooms) if (room != null && IsSpecialRoomType(room.Type) && RenderRoomLandmark(room, parent)) landmarksCreated++;
        Debug.Log($"Created {landmarksCreated} special room objects");
    }

    private bool IsSpecialRoomType(RoomType roomType)
        => roomType == RoomType.Entrance || 
            roomType == RoomType.Exit || 
            roomType == RoomType.Shop || 
            roomType == RoomType.Treasure || 
            roomType == RoomType.Boss;

    private bool RenderRoomLandmark(RoomModel room, Transform parent)
    {
        GameObject prefab = ResourceService.LoadLandmarkPrefab(room.Type);
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
}