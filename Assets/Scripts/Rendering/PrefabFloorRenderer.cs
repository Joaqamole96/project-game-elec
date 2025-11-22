// PrefabFloorRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders floors in Real mode using actual prefabs for gameplay.
/// Supports biome themes and individual prefab instantiation.
/// </summary>
public class PrefabFloorRenderer : IFloorRenderer
{
    private GameObject _fallbackFloorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public PrefabFloorRenderer(GameObject floorPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _fallbackFloorPrefab = floorPrefab;
        _materialManager = materialManager;
        _biomeManager = biomeManager;
    }

    /// <summary>
    /// Sets the current biome theme for prefab selection.
    /// </summary>
    public void SetTheme(BiomeTheme theme)
    {
        _currentTheme = theme;
    }

    /// <summary>
    /// Renders floors as combined meshes (not typically used in Real mode with prefabs).
    /// </summary>
    public List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        // In real mode with prefabs, we typically don't combine meshes
        // But we'll return empty list for interface compliance
        RenderIndividualFloors(layout, rooms, parent, false);
        return new List<GameObject>();
    }

    /// <summary>
    /// Renders floors as individual prefab instances.
    /// </summary>
    public void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision)
    {
        if (layout?.AllFloorTiles == null)
        {
            Debug.LogError("Cannot render prefab floors: layout or floor tiles is null");
            return;
        }

        int floorsCreated = 0;
        foreach (var floorPos in layout.AllFloorTiles)
        {
            var floorPrefab = _biomeManager.GetFloorPrefab(_currentTheme);
            var floor = CreateFloorAtPosition(floorPos, floorPrefab);
            
            if (floor != null)
            {
                floor.transform.SetParent(parent);
                
                if (enableCollision)
                    AddCollisionToObject(floor, "Floor");
                
                floorsCreated++;
            }
        }

        Debug.Log($"Created {floorsCreated} prefab floors");
    }

    private GameObject CreateFloorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f);
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject floorPrefabToUse = prefab ?? _fallbackFloorPrefab;
        
        if (floorPrefabToUse == null)
        {
            Debug.LogWarning("No floor prefab available!");
            return null;
        }
        
        var floor = Object.Instantiate(floorPrefabToUse, worldPos, Quaternion.identity);
        floor.name = $"Floor_{gridPos.x}_{gridPos.y}";
        
        return floor;
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}