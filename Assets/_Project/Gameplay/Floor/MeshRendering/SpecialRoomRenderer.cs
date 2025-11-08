// MeshRendering/SpecialRoomRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpecialRoomRenderer
{
    private GameObject _defaultEntrancePrefab;
    private GameObject _defaultExitPrefab;
    private BiomeManager _biomeManager;

    public SpecialRoomRenderer(GameObject entrancePrefab, GameObject exitPrefab)
    {
        _defaultEntrancePrefab = entrancePrefab;
        _defaultExitPrefab = exitPrefab;
        _biomeManager = new BiomeManager();
    }

    public void RenderSpecialObjects(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        foreach (var room in rooms)
        {
            if (room.Type == RoomType.Entrance || room.Type == RoomType.Exit)
            {
                RenderRoomSpecialObject(room, parent);
            }
        }
    }

    private void RenderRoomSpecialObject(RoomModel room, Transform parent)
    {
        GameObject prefab = _biomeManager.GetSpecialRoomPrefab(room.Type);
        
        // Fallback to default prefabs if biome doesn't provide one
        if (prefab == null)
        {
            prefab = room.Type == RoomType.Entrance ? _defaultEntrancePrefab : _defaultExitPrefab;
        }

        if (prefab != null)
        {
            Vector3 position = new Vector3(room.Center.x, 0, room.Center.y);
            var specialObject = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            specialObject.name = $"{room.Type}_{room.ID}";
        }
    }
}