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
    /// Clears the material cache and properly destroys created materials to prevent memory leaks
    /// </summary>
    public void CleanupMaterialCache()
    {
        try
        {
            int roomMaterialsCount = _roomTypeMaterials.Count;
            int wallMaterialsCount = _wallTypeMaterials.Count;
            // Destroy all created materials to prevent memory leaks
            foreach (var material in _roomTypeMaterials.Values) if (material != null && material != _defaultFloorMaterial) Object.DestroyImmediate(material);
            foreach (var material in _wallTypeMaterials.Values) if (material != null && material != _defaultWallMaterial && material != _defaultDoorMaterial) Object.DestroyImmediate(material);
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