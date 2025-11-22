// SpecialRoomRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders special room objects (entrance, exit, shop, treasure, boss) using prefabs.
/// Supports biome biomes and proper object placement.
/// </summary>
public class SpecialRoomRenderer
{
    private GameObject _defaultEntrancePrefab;
    private GameObject _defaultExitPrefab;
    private BiomeManager _biomeManager;
    private BiomeModel _currentBiome;

    public SpecialRoomRenderer(GameObject entrancePrefab, GameObject exitPrefab, BiomeManager biomeManager)
    {
        _defaultEntrancePrefab = entrancePrefab;
        _defaultExitPrefab = exitPrefab;
        _biomeManager = biomeManager;
    }

    /// <summary>
    /// Sets the current biome biome for special room prefab selection.
    /// </summary>
    public void SetBiome(BiomeModel biome)
    {
        _currentBiome = biome;
    }

    /// <summary>
    /// Renders special objects for all special rooms in the level.
    /// </summary>
    public void RenderSpecialObjects(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        if (rooms == null || parent == null)
        {
            Debug.LogError("Cannot render special objects: rooms or parent is null");
            return;
        }

        int specialObjectsCreated = 0;
        foreach (var room in rooms)
        {
            if (room != null && IsSpecialRoomType(room.Type))
            {
                if (RenderRoomSpecialObject(room, parent))
                {
                    specialObjectsCreated++;
                }
            }
        }

        Debug.Log($"Created {specialObjectsCreated} special room objects");
    }

    private bool IsSpecialRoomType(RoomType roomType)
    {
        return roomType == RoomType.Entrance || roomType == RoomType.Exit || 
               roomType == RoomType.Shop || roomType == RoomType.Treasure || 
               roomType == RoomType.Boss;
    }

    private bool RenderRoomSpecialObject(RoomModel room, Transform parent)
    {
        GameObject prefab = GetSpecialRoomPrefab(room.Type);

        // Fallback to default prefabs if biome doesn't provide one
        if (prefab == null)
        {
            prefab = GetDefaultSpecialRoomPrefab(room.Type);
        }

        if (prefab != null)
        {
            Vector3 position = new(room.Center.x, 0, room.Center.y);
            var specialObject = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            specialObject.name = $"{room.Type}_{room.ID}";
            return true;
        }
        else
        {
            Debug.LogWarning($"No prefab available for {room.Type} room {room.ID}");
            return false;
        }
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
            _ => null // Only entrance/exit have default fallbacks
        };
    }
}