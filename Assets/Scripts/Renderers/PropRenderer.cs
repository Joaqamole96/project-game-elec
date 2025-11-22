// PropRenderer.cs - FIXED VERSION
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders functional props: Entrance/Exit portals, Shop structures, Treasure pedestals, etc.
/// Throws errors for missing assets - no fallbacks.
/// </summary>
public class PropRenderer
{
    private BiomeManager _biomeManager;

    public PropRenderer(BiomeManager biomeManager)
    {
        _biomeManager = biomeManager;
    }

    /// <summary>
    /// Renders all props for the level based on room types and prop models.
    /// </summary>
    public void RenderAllProps(LevelModel layout, List<RoomModel> rooms, BiomeModel biome, Transform parent)
    {
        if (layout == null) throw new System.ArgumentNullException(nameof(layout));
        if (rooms == null) throw new System.ArgumentNullException(nameof(rooms));
        if (biome == null) throw new System.ArgumentNullException(nameof(biome));
        if (parent == null) throw new System.ArgumentNullException(nameof(parent));

        Debug.Log($"Starting prop rendering for biome: {biome.Name}");

        int propsCreated = 0;

        // Render room-specific props (entrance, exit, shop, treasure, boss)
        foreach (var room in rooms)
        {
            if (room != null && IsSpecialRoomType(room.Type))
            {
                if (RenderRoomProp(room, biome, parent))
                {
                    propsCreated++;
                }
            }
        }

        // FIXED: Simplified AdditionalProps handling
        if (layout.AdditionalProps != null && layout.AdditionalProps.Count > 0)
        {
            Debug.LogWarning($"AdditionalProps system not yet fully implemented ({layout.AdditionalProps.Count} props pending)");
        }

        Debug.Log($"Created {propsCreated} prop instances");
    }

    /// <summary>
    /// Renders a specific room's main prop (entrance, exit, etc.).
    /// </summary>
    public bool RenderRoomProp(RoomModel room, BiomeModel biome, Transform parent)
    {
        if (room == null) throw new System.ArgumentNullException(nameof(room));

        GameObject propPrefab = GetRoomPropPrefab(room.Type, biome);
        if (propPrefab == null)
        {
            // FIXED: Use NullReferenceException instead of MissingReferenceException
            throw new System.NullReferenceException($"Prop prefab not found for room type: {room.Type} in biome: {biome.Name}");
        }

        Vector3 position = CalculatePropPosition(room, propPrefab);
        Quaternion rotation = CalculatePropRotation(room, propPrefab);

        var prop = GameObject.Instantiate(propPrefab, position, rotation, parent);
        prop.name = $"{room.Type}_Prop_{room.ID}";

        // Configure prop components based on type
        ConfigurePropComponents(prop, room.Type);

        Debug.Log($"Created {room.Type} prop at room {room.ID}");
        return true;
    }

    #region Helper Methods

    private bool IsSpecialRoomType(RoomType roomType)
    {
        return roomType == RoomType.Entrance || roomType == RoomType.Exit || 
               roomType == RoomType.Shop || roomType == RoomType.Treasure || 
               roomType == RoomType.Boss;
    }

    private GameObject GetRoomPropPrefab(RoomType roomType, BiomeModel biome)
    {
        return roomType switch
        {
            RoomType.Entrance => _biomeManager.GetEntrancePropPrefab(biome),
            RoomType.Exit => _biomeManager.GetExitPropPrefab(biome),
            RoomType.Shop => _biomeManager.GetShopPropPrefab(biome),
            RoomType.Treasure => _biomeManager.GetTreasurePropPrefab(biome),
            RoomType.Boss => _biomeManager.GetBossPropPrefab(biome),
            _ => null
        };
    }

    private Vector3 CalculatePropPosition(RoomModel room, GameObject propPrefab)
    {
        // Center the prop in the room, with optional height adjustment
        Vector3 basePosition = new Vector3(room.Center.x, 0f, room.Center.y);
        
        // Adjust height based on prop's bounds if needed
        var renderer = propPrefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            float heightOffset = renderer.bounds.extents.y;
            basePosition.y += heightOffset;
        }

        return basePosition;
    }

    private Quaternion CalculatePropRotation(RoomModel room, GameObject propPrefab)
    {
        // Default to identity, can be customized per room type or prop type
        return Quaternion.identity;
    }

    private void ConfigurePropComponents(GameObject prop, RoomType roomType)
    {
        // Add specific components based on prop type
        switch (roomType)
        {
            case RoomType.Entrance:
                AddEntranceComponents(prop);
                break;
            case RoomType.Exit:
                AddExitComponents(prop);
                break;
            case RoomType.Shop:
                AddShopComponents(prop);
                break;
            case RoomType.Treasure:
                AddTreasureComponents(prop);
                break;
            case RoomType.Boss:
                AddBossComponents(prop);
                break;
        }
    }

    private void AddEntranceComponents(GameObject prop)
    {
        // Ensure entrance has appropriate components
        if (prop.GetComponent<EntranceController>() == null)
            prop.AddComponent<EntranceController>();
    }

    private void AddExitComponents(GameObject prop)
    {
        // Ensure exit has appropriate components
        if (prop.GetComponent<ExitController>() == null)
            prop.AddComponent<ExitController>();
    }

    private void AddShopComponents(GameObject prop)
    {
        // Ensure shop has appropriate components
        if (prop.GetComponent<ShopController>() == null)
            prop.AddComponent<ShopController>();
    }

    private void AddTreasureComponents(GameObject prop)
    {
        // Ensure treasure has appropriate components
        if (prop.GetComponent<TreasureController>() == null)
            prop.AddComponent<TreasureController>();
    }

    private void AddBossComponents(GameObject prop)
    {
        // Ensure boss room prop has appropriate components
        if (prop.GetComponent<BossRoomController>() == null)
            prop.AddComponent<BossRoomController>();
    }

    #endregion
}

// Temporary data structure for props until fully implemented
[System.Serializable]
public class PropData
{
    public string PropType;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale = Vector3.one;
}