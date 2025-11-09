// PrefabDoorRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders doors in Real mode using actual prefabs for gameplay.
/// Supports biome themes and proper door placement.
/// </summary>
public class PrefabDoorRenderer : IDoorRenderer
{
    private GameObject _fallbackDoorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public PrefabDoorRenderer(GameObject doorPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _fallbackDoorPrefab = doorPrefab;
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
    /// Renders all doors as individual prefab instances.
    /// </summary>
    public void RenderDoors(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllDoorTiles == null)
        {
            Debug.LogError("Cannot render prefab doors: layout or door tiles is null");
            return;
        }
        
        int doorsCreated = 0;
        foreach (var doorPos in layout.AllDoorTiles)
        {
            // Use the current theme's door prefab
            var doorPrefab = _biomeManager.GetDoorPrefab(_currentTheme);
            var door = CreateDoorAtPosition(doorPos, doorPrefab);
            
            if (door != null)
            {
                door.transform.SetParent(parent);
                
                if (enableCollision) 
                {
                    AddCollisionToObject(door, "Door");
                }
                
                doorsCreated++;
            }
        }

        Debug.Log($"Created {doorsCreated} prefab doors");
    }

    private GameObject CreateDoorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f); // Door at floor level
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject doorPrefabToUse = prefab ?? _fallbackDoorPrefab;
        
        if (doorPrefabToUse == null)
        {
            Debug.LogWarning("No door prefab available!");
            return null;
        }
        
        var door = Object.Instantiate(doorPrefabToUse, worldPos, Quaternion.identity);
        door.name = $"Door_{gridPos.x}_{gridPos.y}";
        
        return door;
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        if (objectType == "Door")
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
    }
}