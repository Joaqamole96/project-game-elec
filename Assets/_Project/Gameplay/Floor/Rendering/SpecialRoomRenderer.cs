using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpecialRoomRenderer
{
    private GameObject _defaultEntrancePrefab;
    private GameObject _defaultExitPrefab;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public SpecialRoomRenderer(GameObject entrancePrefab, GameObject exitPrefab, BiomeManager biomeManager)
    {
        _defaultEntrancePrefab = entrancePrefab;
        _defaultExitPrefab = exitPrefab;
        _biomeManager = biomeManager;
    }

    // NEW: Set the current theme
    public void SetTheme(BiomeTheme theme)
    {
        _currentTheme = theme;
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
        GameObject prefab = null;
        
        // Use the current theme's special room prefab
        switch (room.Type)
        {
            case RoomType.Entrance:
                prefab = _biomeManager.GetPrefab(_currentTheme?.EntrancePrefabPath);
                break;
            case RoomType.Exit:
                prefab = _biomeManager.GetPrefab(_currentTheme?.ExitPrefabPath);
                break;
            case RoomType.Shop:
                prefab = _biomeManager.GetPrefab(_currentTheme?.ShopPrefabPath);
                break;
            case RoomType.Treasure:
                prefab = _biomeManager.GetPrefab(_currentTheme?.TreasurePrefabPath);
                break;
            case RoomType.Boss:
                prefab = _biomeManager.GetPrefab(_currentTheme?.BossPrefabPath);
                break;
        }

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