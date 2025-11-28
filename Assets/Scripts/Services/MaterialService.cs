// -------------------------------------------------- //
// Scripts/Services/MaterialService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Service for managing and caching materials for different room types, wall types, and architectural elements
/// Provides efficient material lookup and creation with proper cleanup
/// </summary>
public class MaterialService
{
    private readonly Dictionary<RoomType, Material> _roomTypeMaterials = new();
    private readonly Dictionary<WallType, Material> _wallTypeMaterials = new();
    
    private readonly Material _defaultFloorMaterial;
    private readonly Material _defaultWallMaterial;
    private readonly Material _defaultDoorMaterial;

    /// <summary>
    /// Initializes a new instance of MaterialService with default materials
    /// </summary>
    /// <param name="defaultFloorMaterial">Default material for floor surfaces</param>
    /// <param name="defaultWallMaterial">Default material for wall surfaces</param>
    /// <param name="defaultDoorMaterial">Default material for door elements</param>
    public MaterialService(Material defaultFloorMaterial, Material defaultWallMaterial, Material defaultDoorMaterial)
    {
        try
        {
            _defaultFloorMaterial = defaultFloorMaterial;
            _defaultWallMaterial = defaultWallMaterial;
            _defaultDoorMaterial = defaultDoorMaterial;

            Debug.Log("MaterialService: Service initialized with default materials");
            
            if (_defaultFloorMaterial == null)
            {
                Debug.LogWarning("MaterialService: Default floor material is null");
            }
            if (_defaultWallMaterial == null)
            {
                Debug.LogWarning("MaterialService: Default wall material is null");
            }
            if (_defaultDoorMaterial == null)
            {
                Debug.LogWarning("MaterialService: Default door material is null");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error during constructor initialization: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the material cache with colors for all room and wall types
    /// Call this before using the service to ensure all materials are ready
    /// </summary>
    public void InitializeMaterialCache()
    {
        try
        {
            Debug.Log("MaterialService: Initializing material cache...");
            
            CleanupMaterialCache();
            InitializeRoomTypeMaterials();
            InitializeWallTypeMaterials();
            
            Debug.Log($"MaterialService: Material cache initialized - {_roomTypeMaterials.Count} room materials, {_wallTypeMaterials.Count} wall materials");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error initializing material cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes materials for all room types with appropriate colors
    /// </summary>
    private void InitializeRoomTypeMaterials()
    {
        try
        {
            _roomTypeMaterials[RoomType.Entrance] = CreateMaterial(Color.green, "EntranceRoom");
            _roomTypeMaterials[RoomType.Exit] = CreateMaterial(Color.red, "ExitRoom");
            _roomTypeMaterials[RoomType.Empty] = CreateMaterial(Color.gray, "EmptyRoom");
            _roomTypeMaterials[RoomType.Combat] = CreateMaterial(Color.white, "CombatRoom");
            _roomTypeMaterials[RoomType.Shop] = CreateMaterial(Color.blue, "ShopRoom");
            _roomTypeMaterials[RoomType.Treasure] = CreateMaterial(Color.yellow, "TreasureRoom");
            _roomTypeMaterials[RoomType.Boss] = CreateMaterial(Color.magenta, "BossRoom");

            Debug.Log($"MaterialService: Created {_roomTypeMaterials.Count} room type materials");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error initializing room type materials: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes materials for all wall types with appropriate colors
    /// </summary>
    private void InitializeWallTypeMaterials()
    {
        try
        {
            var wallColor = Color.black;
            var interiorColor = Color.white;
            var doorwayColor = Color.gray;
            
            // External walls
            _wallTypeMaterials[WallType.North] = CreateMaterial(wallColor, "NorthWall");
            _wallTypeMaterials[WallType.South] = CreateMaterial(wallColor, "SouthWall");
            _wallTypeMaterials[WallType.East] = CreateMaterial(wallColor, "EastWall");
            _wallTypeMaterials[WallType.West] = CreateMaterial(wallColor, "WestWall");
            
            // Corners
            _wallTypeMaterials[WallType.NorthEastCorner] = CreateMaterial(wallColor, "NorthEastCorner");
            _wallTypeMaterials[WallType.NorthWestCorner] = CreateMaterial(wallColor, "NorthWestCorner");
            _wallTypeMaterials[WallType.SouthEastCorner] = CreateMaterial(wallColor, "SouthEastCorner");
            _wallTypeMaterials[WallType.SouthWestCorner] = CreateMaterial(wallColor, "SouthWestCorner");
            
            // Special types
            _wallTypeMaterials[WallType.Interior] = CreateMaterial(interiorColor, "InteriorWall");
            _wallTypeMaterials[WallType.Doorway] = CreateMaterial(doorwayColor, "Doorway");
            _wallTypeMaterials[WallType.Corridor] = CreateMaterial(wallColor, "CorridorWall");

            Debug.Log($"MaterialService: Created {_wallTypeMaterials.Count} wall type materials");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error initializing wall type materials: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new material with the specified color, using existing material as template if available
    /// </summary>
    /// <param name="color">Color for the new material</param>
    /// <param name="materialName">Optional name for the material for debugging</param>
    /// <returns>New Material instance with the specified color</returns>
    private Material CreateMaterial(Color color, string materialName = "UnnamedMaterial")
    {
        try
        {
            Material newMaterial;
            
            if (_defaultFloorMaterial != null)
            {
                // Use default floor material as template to preserve shader and properties
                newMaterial = new Material(_defaultFloorMaterial) { color = color };
            }
            else
            {
                // Fallback to standard shader if no template available
                Shader standardShader = Shader.Find("Standard");
                if (standardShader == null)
                {
                    Debug.LogError("MaterialService: Standard shader not found - using default fallback");
                    standardShader = Shader.Find("Legacy Shaders/Diffuse");
                }
                
                newMaterial = new Material(standardShader) { color = color };
            }
            
            newMaterial.name = $"MaterialService_{materialName}";
            Debug.Log($"MaterialService: Created material '{newMaterial.name}' with color {color}");
            
            return newMaterial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error creating material {materialName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the material for a specific room type
    /// </summary>
    /// <param name="roomType">The room type to get material for</param>
    /// <returns>Material for the room type, or default floor material if not found</returns>
    public Material GetRoomMaterial(RoomType roomType)
    {
        try
        {
            if (_roomTypeMaterials.TryGetValue(roomType, out Material material))
            {
                return material;
            }
            
            Debug.LogWarning($"MaterialService: No material found for room type {roomType}, using default");
            return _defaultFloorMaterial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error getting room material for {roomType}: {ex.Message}");
            return _defaultFloorMaterial;
        }
    }

    /// <summary>
    /// Gets the material for a specific wall type
    /// </summary>
    /// <param name="wallType">The wall type to get material for</param>
    /// <returns>Material for the wall type, or default wall material if not found</returns>
    public Material GetWallMaterial(WallType wallType)
    {
        try
        {
            if (_wallTypeMaterials.TryGetValue(wallType, out Material material))
            {
                return material;
            }
            
            Debug.LogWarning($"MaterialService: No material found for wall type {wallType}, using default");
            return _defaultWallMaterial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error getting wall material for {wallType}: {ex.Message}");
            return _defaultWallMaterial;
        }
    }

    /// <summary>
    /// Gets the material for doors
    /// </summary>
    /// <returns>Door material, or default door material, or default wall material as fallback</returns>
    public Material GetDoorMaterial()
    {
        try
        {
            if (_wallTypeMaterials.TryGetValue(WallType.Doorway, out Material doorMaterial))
            {
                return doorMaterial;
            }
            
            if (_defaultDoorMaterial != null)
            {
                return _defaultDoorMaterial;
            }
            
            Debug.LogWarning("MaterialService: No door material found, using default wall material");
            return _defaultWallMaterial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error getting door material: {ex.Message}");
            return _defaultWallMaterial ?? _defaultFloorMaterial;
        }
    }

    /// <summary>
    /// Clears the material cache and properly destroys created materials to prevent memory leaks
    /// </summary>
    public void CleanupMaterialCache()
    {
        try
        {
            int roomMaterialsCount = _roomTypeMaterials.Count;
            int wallMaterialsCount = _wallTypeMaterials.Count;
            
            // Destroy all created materials to prevent memory leaks
            foreach (var material in _roomTypeMaterials.Values)
            {
                if (material != null && material != _defaultFloorMaterial)
                {
                    Object.DestroyImmediate(material);
                }
            }
            
            foreach (var material in _wallTypeMaterials.Values)
            {
                if (material != null && material != _defaultWallMaterial && material != _defaultDoorMaterial)
                {
                    Object.DestroyImmediate(material);
                }
            }
            
            _roomTypeMaterials.Clear();
            _wallTypeMaterials.Clear();
            
            Debug.Log($"MaterialService: Cache cleaned up - destroyed {roomMaterialsCount + wallMaterialsCount} materials");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error cleaning up material cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the number of cached room materials
    /// </summary>
    /// <returns>Count of room materials in cache</returns>
    public int GetRoomMaterialCount()
    {
        return _roomTypeMaterials.Count;
    }

    /// <summary>
    /// Gets the number of cached wall materials
    /// </summary>
    /// <returns>Count of wall materials in cache</returns>
    public int GetWallMaterialCount()
    {
        return _wallTypeMaterials.Count;
    }

    /// <summary>
    /// Logs all currently cached materials for debugging purposes
    /// </summary>
    public void LogCachedMaterials()
    {
        try
        {
            Debug.Log("=== MaterialService Cache ===");
            Debug.Log($"Room Materials: {_roomTypeMaterials.Count}");
            foreach (var kvp in _roomTypeMaterials)
            {
                Debug.Log($"  {kvp.Key}: {(kvp.Value != null ? kvp.Value.name : "NULL")}");
            }
            
            Debug.Log($"Wall Materials: {_wallTypeMaterials.Count}");
            foreach (var kvp in _wallTypeMaterials)
            {
                Debug.Log($"  {kvp.Key}: {(kvp.Value != null ? kvp.Value.name : "NULL")}");
            }
            Debug.Log("============================");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error logging cached materials: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when the service is being destroyed - ensures proper cleanup
    /// </summary>
    ~MaterialService()
    {
        try
        {
            CleanupMaterialCache();
            Debug.Log("MaterialService: Service destroyed and cache cleaned up");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error in destructor: {ex.Message}");
        }
    }
}