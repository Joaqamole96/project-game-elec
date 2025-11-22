// PrefabWallRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders walls in Real mode using actual prefabs for gameplay.
/// Supports biome themes and proper wall orientation.
/// </summary>
public class PrefabWallRenderer : IWallRenderer
{
    private GameObject _fallbackWallPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeModel _currentTheme;

    public PrefabWallRenderer(GameObject wallPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _fallbackWallPrefab = wallPrefab;
        _materialManager = materialManager;
        _biomeManager = biomeManager;
    }

    /// <summary>
    /// Sets the current biome theme for prefab selection.
    /// </summary>
    public void SetTheme(BiomeModel theme)
    {
        _currentTheme = theme;
    }

    /// <summary>
    /// Renders walls as combined meshes (not typically used in Real mode with prefabs).
    /// </summary>
    public List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent)
    {
        // In real mode with prefabs, we typically don't combine meshes
        // But we'll return empty list for interface compliance
        RenderIndividualWalls(layout, parent, false);
        return new List<GameObject>();
    }

    /// <summary>
    /// Renders walls as individual prefab instances with proper orientation.
    /// </summary>
    public void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null)
        {
            Debug.LogError("Cannot render prefab walls: layout data is null");
            return;
        }

        int wallsCreated = 0;
        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                var wallPrefab = _biomeManager.GetWallPrefab(_currentTheme);
                var wall = CreateWallAtPosition(wallPos, wallType, wallPrefab);
                
                if (wall != null)
                {
                    wall.transform.SetParent(parent);
                    
                    if (enableCollision)
                        AddCollisionToObject(wall, "Wall");
                    
                    wallsCreated++;
                }
            }
        }

        Debug.Log($"Created {wallsCreated} prefab walls");
    }

    private GameObject CreateWallAtPosition(Vector2Int gridPos, WallType wallType, GameObject prefab)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 1f, gridPos.y + 0.5f); // Walls at 1 unit height
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject wallPrefabToUse = prefab ?? _fallbackWallPrefab;
        
        if (wallPrefabToUse == null)
        {
            Debug.LogWarning("No wall prefab available!");
            return null;
        }
        
        var wall = Object.Instantiate(wallPrefabToUse, worldPos, Quaternion.identity);
        wall.name = $"Wall_{wallType}_{gridPos.x}_{gridPos.y}";
        
        // Apply wall rotation for better visual alignment
        ApplyWallRotation(wall, wallType);
        
        return wall;
    }

    private void ApplyWallRotation(GameObject wall, WallType wallType)
    {
        if (wall == null) return;

        // Adjust rotation based on wall type for better visual presentation
        Quaternion rotation = GetWallRotation(wallType);
        wall.transform.rotation = rotation;
    }

    private Quaternion GetWallRotation(WallType wallType)
    {
        return wallType switch
        {
            WallType.North => Quaternion.Euler(0, 0, 0),
            WallType.South => Quaternion.Euler(0, 180, 0),
            WallType.East => Quaternion.Euler(0, 90, 0),
            WallType.West => Quaternion.Euler(0, 270, 0),
            WallType.Corridor => Quaternion.Euler(0, 0, 0), // Corridor walls might need special handling
            _ => Quaternion.identity
        };
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}