// -------------------------------------------------- //
// Scripts/Renderers/PrefabWallRenderer.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class PrefabWallRenderer
{
    private GameObject _fallbackWallPrefab;
    private BiomeManager _biomeManager;
    private BiomeModel _currentBiome;

    public PrefabWallRenderer(GameObject wallPrefab, BiomeManager biomeManager)
    {
        _fallbackWallPrefab = wallPrefab;
        _biomeManager = biomeManager;
    }

    public List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent)
    {
        RenderIndividualWalls(layout, parent, false);
        return new List<GameObject>();
    }

    public void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null) throw new("Cannot render prefab walls: layout data is null");

        int wallsCreated = 0;
        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                var wallPrefab = _biomeManager.GetWallPrefab(_currentBiome);
                var wall = CreateWallAtPosition(wallPos, wallType, wallPrefab);
                
                if (wall != null)
                {
                    wall.transform.SetParent(parent);
                    if (enableCollision) AddCollisionToObject(wall);
                    wallsCreated++;
                }
            }
        }

        Debug.Log($"Created {wallsCreated} prefab walls");
    }

    private GameObject CreateWallAtPosition(Vector2Int gridPos, WallType wallType, GameObject prefab)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 1f, gridPos.y + 0.5f);
        
        GameObject wallPrefabToUse = prefab != null ? prefab : _fallbackWallPrefab;
        
        if (wallPrefabToUse == null)
        {
            Debug.LogWarning("No wall prefab available!");
            return null;
        }
        
        var wall = Object.Instantiate(wallPrefabToUse, worldPos, Quaternion.identity);
        wall.name = $"Wall_{wallType}_{gridPos.x}_{gridPos.y}";
        
        ApplyWallRotation(wall, wallType);
        
        return wall;
    }

    private void ApplyWallRotation(GameObject wall, WallType wallType)
    {
        if (wall == null) return;

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

    private void AddCollisionToObject(GameObject obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null) obj.AddComponent<BoxCollider>();
    }
}