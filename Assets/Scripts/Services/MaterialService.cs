// -------------------------------------------------- //
// Scripts/Services/MaterialService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class MaterialService
{
    private readonly Dictionary<RoomType, Material> _roomTypeMaterials = new();
    private readonly Dictionary<WallType, Material> _wallTypeMaterials = new();
    
    private readonly Material _defaultFloorMaterial;
    private readonly Material _defaultWallMaterial;
    private readonly Material _defaultDoorMaterial;

    public MaterialService(Material defaultFloorMaterial, Material defaultWallMaterial, Material defaultDoorMaterial)
    {
        _defaultFloorMaterial = defaultFloorMaterial;
        _defaultWallMaterial = defaultWallMaterial;
        _defaultDoorMaterial = defaultDoorMaterial;
    }

    /// <summary>
    /// Initializes the material cache with colors for all room and wall types.
    /// </summary>
    public void InitializeMaterialCache()
    {
        CleanupMaterialCache();
        InitializeRoomTypeMaterials();
        InitializeWallTypeMaterials();
    }

    private void InitializeRoomTypeMaterials()
    {
        _roomTypeMaterials[RoomType.Entrance] = CreateMaterial(Color.green);
        _roomTypeMaterials[RoomType.Exit] = CreateMaterial(Color.red);
        _roomTypeMaterials[RoomType.Empty] = CreateMaterial(Color.gray);
        _roomTypeMaterials[RoomType.Combat] = CreateMaterial(Color.white);
        _roomTypeMaterials[RoomType.Shop] = CreateMaterial(Color.blue);
        _roomTypeMaterials[RoomType.Treasure] = CreateMaterial(Color.yellow);
        _roomTypeMaterials[RoomType.Boss] = CreateMaterial(Color.magenta);
    }

    private void InitializeWallTypeMaterials()
    {
        var wallColor = Color.black;
        var interiorColor = Color.white;
        var doorwayColor = Color.gray;
        
        // External walls
        _wallTypeMaterials[WallType.North] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.South] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.East] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.West] = CreateMaterial(wallColor);
        
        // Corners
        _wallTypeMaterials[WallType.NorthEastCorner] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.NorthWestCorner] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.SouthEastCorner] = CreateMaterial(wallColor);
        _wallTypeMaterials[WallType.SouthWestCorner] = CreateMaterial(wallColor);
        
        // Special types
        _wallTypeMaterials[WallType.Interior] = CreateMaterial(interiorColor);
        _wallTypeMaterials[WallType.Doorway] = CreateMaterial(doorwayColor);
        _wallTypeMaterials[WallType.Corridor] = CreateMaterial(wallColor);
    }

    private Material CreateMaterial(Color color)
    {
        if (_defaultFloorMaterial != null)
        {
            Material newMaterial = new(_defaultFloorMaterial);
            newMaterial.color = color;
            return newMaterial;
        }
        else
        {
            Material newMaterial = new(Shader.Find("Standard"));
            newMaterial.color = color;
            return newMaterial;
        }
    }

    /// <summary>Gets the material for a specific room type.</summary>
    public Material GetRoomMaterial(RoomType roomType)
    {
        return _roomTypeMaterials.GetValueOrDefault(roomType, _defaultFloorMaterial);
    }

    /// <summary>Gets the material for a specific wall type.</summary>
    public Material GetWallMaterial(WallType wallType)
    {
        return _wallTypeMaterials.GetValueOrDefault(wallType, _defaultWallMaterial);
    }

    /// <summary>Gets the material for doors.</summary>
    public Material GetDoorMaterial()
    {
        return _wallTypeMaterials.GetValueOrDefault(WallType.Doorway, _defaultDoorMaterial ?? _defaultWallMaterial);
    }

    /// <summary>Clears the material cache.</summary>
    public void CleanupMaterialCache()
    {
        _roomTypeMaterials.Clear();
        _wallTypeMaterials.Clear();
    }
}