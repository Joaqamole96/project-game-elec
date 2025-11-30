// -------------------------------------------------- //
// Scripts/Services/MaterialService.cs (MINIMAL)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Minimal material service for cleanup operations
/// </summary>
public class MaterialService
{
    private readonly Dictionary<RoomType, Material> _roomTypeMaterials = new();
    private readonly Dictionary<WallType, Material> _wallTypeMaterials = new();

    /// <summary>
    /// Clears the material cache and properly destroys created materials
    /// </summary>
    public void CleanupMaterialCache()
    {
        try
        {
            int totalCount = _roomTypeMaterials.Count + _wallTypeMaterials.Count;
            
            foreach (var material in _roomTypeMaterials.Values)
            {
                if (material != null)
                {
                    Object.DestroyImmediate(material);
                }
            }
            
            foreach (var material in _wallTypeMaterials.Values)
            {
                if (material != null)
                {
                    Object.DestroyImmediate(material);
                }
            }
            
            _roomTypeMaterials.Clear();
            _wallTypeMaterials.Clear();
            
            Debug.Log($"MaterialService: Cache cleaned - destroyed {totalCount} materials");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MaterialService: Error cleaning up cache: {ex.Message}");
        }
    }

    ~MaterialService()
    {
        CleanupMaterialCache();
    }
}