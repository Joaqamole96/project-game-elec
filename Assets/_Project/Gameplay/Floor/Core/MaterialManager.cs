using UnityEngine;
using System.Collections.Generic;

public class MaterialManager
{
    private Dictionary<RoomType, Material> _roomTypeMaterials = new();
    private Dictionary<WallType, Material> _wallTypeMaterials = new();
    
    private Material _defaultFloorMaterial;
    private Material _defaultWallMaterial;
    private Material _defaultDoorMaterial;

    public MaterialManager(Material defaultFloorMaterial, Material defaultWallMaterial, Material defaultDoorMaterial)
    {
        _defaultFloorMaterial = defaultFloorMaterial;
        _defaultWallMaterial = defaultWallMaterial;
        _defaultDoorMaterial = defaultDoorMaterial;
    }

    public void InitializeMaterialCache()
    {
        // Clear any existing materials first
        CleanupMaterialCache();

        // Room type materials
        _roomTypeMaterials[RoomType.Entrance] = GetOrCreateMaterial(Color.green);
        _roomTypeMaterials[RoomType.Exit] = GetOrCreateMaterial(Color.red);
        _roomTypeMaterials[RoomType.Empty] = GetOrCreateMaterial(Color.gray);
        _roomTypeMaterials[RoomType.Combat] = GetOrCreateMaterial(Color.white);
        _roomTypeMaterials[RoomType.Shop] = GetOrCreateMaterial(Color.blue);
        _roomTypeMaterials[RoomType.Treasure] = GetOrCreateMaterial(Color.yellow);
        _roomTypeMaterials[RoomType.Boss] = GetOrCreateMaterial(Color.magenta);
        
        // Wall type materials
        _wallTypeMaterials[WallType.North] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.South] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.East] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.West] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.NorthEastCorner] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.NorthWestCorner] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.SouthEastCorner] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.SouthWestCorner] = GetOrCreateMaterial(Color.black);
        _wallTypeMaterials[WallType.Interior] = GetOrCreateMaterial(Color.white);
        _wallTypeMaterials[WallType.Doorway] = GetOrCreateMaterial(Color.gray);
        _wallTypeMaterials[WallType.Corridor] = GetOrCreateMaterial(Color.black);
    }

    private Material GetOrCreateMaterial(Color color)
    {
        if (_defaultFloorMaterial != null)
        {
            Material newMaterial = new Material(_defaultFloorMaterial);
            newMaterial.color = color;
            return newMaterial;
        }
        else
        {
            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.color = color;
            return newMaterial;
        }
    }

    public Material GetRoomMaterial(RoomType roomType)
    {
        return _roomTypeMaterials.ContainsKey(roomType) ? _roomTypeMaterials[roomType] : _defaultFloorMaterial;
    }

    public Material GetWallMaterial(WallType wallType)
    {
        return _wallTypeMaterials.ContainsKey(wallType) ? _wallTypeMaterials[wallType] : _defaultWallMaterial;
    }

    public Material GetDoorMaterial()
    {
        if (_wallTypeMaterials.ContainsKey(WallType.Doorway))
            return _wallTypeMaterials[WallType.Doorway];
        return _defaultDoorMaterial ?? _defaultWallMaterial;
    }

    public void CleanupMaterialCache()
    {
        // Just clear the dictionaries - materials will be garbage collected
        _roomTypeMaterials.Clear();
        _wallTypeMaterials.Clear();
    }
}