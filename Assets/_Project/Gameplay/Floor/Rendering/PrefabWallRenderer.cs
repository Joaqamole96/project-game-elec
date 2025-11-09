using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrefabWallRenderer : IWallRenderer
{
    private GameObject _wallPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public PrefabWallRenderer(GameObject wallPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _wallPrefab = wallPrefab;
        _materialManager = materialManager;
        _biomeManager = biomeManager;
    }

    // NEW: Set the current theme
    public void SetTheme(BiomeTheme theme)
    {
        _currentTheme = theme;
    }

    public List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent)
    {
        // In real mode with prefabs, we typically don't combine meshes
        // But we'll return empty list for interface compliance
        RenderIndividualWalls(layout, parent, false);
        return new List<GameObject>();
    }

    public void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision)
    {
        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                // Use the current theme's wall prefab
                var wallPrefab = _biomeManager.GetWallPrefab(_currentTheme);
                
                var wall = CreateWallAtPosition(wallPos, wallType, wallPrefab);
                if (wall != null)
                {
                    wall.transform.SetParent(parent);
                    
                    if (enableCollision)
                        AddCollisionToObject(wall, "Wall");
                }
            }
        }
    }

    private GameObject CreateWallAtPosition(Vector2Int gridPos, WallType wallType, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 1f, gridPos.y + 0.5f); // Walls at 1 unit height
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject wallPrefabToUse = prefab ?? _wallPrefab;
        
        if (wallPrefabToUse == null)
        {
            Debug.LogWarning("No wall prefab available!");
            return null;
        }
        
        var wall = Object.Instantiate(wallPrefabToUse, worldPos, Quaternion.identity);
        wall.name = $"Wall_{wallType}_{gridPos.x}_{gridPos.y}";
        
        // Optional: Rotate wall based on wall type for better visual alignment
        ApplyWallRotation(wall, wallType);
        
        return wall;
    }

    private void ApplyWallRotation(GameObject wall, WallType wallType)
    {
        // Adjust rotation based on wall type for better visual presentation
        Quaternion rotation = Quaternion.identity;
        
        switch (wallType)
        {
            case WallType.North:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case WallType.South:
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case WallType.East:
                rotation = Quaternion.Euler(0, 90, 0);
                break;
            case WallType.West:
                rotation = Quaternion.Euler(0, 270, 0);
                break;
            case WallType.Corridor:
                // Corridor walls might need special handling
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            // Add more cases for corners if needed
        }
        
        wall.transform.rotation = rotation;
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}