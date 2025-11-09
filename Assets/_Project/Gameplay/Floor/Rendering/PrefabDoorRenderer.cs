using UnityEngine;
using System.Collections.Generic;

public class PrefabDoorRenderer : IDoorRenderer
{
    private GameObject _doorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public PrefabDoorRenderer(GameObject doorPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _doorPrefab = doorPrefab;
        _materialManager = materialManager;
        _biomeManager = biomeManager;
    }

    // NEW: Set the current theme
    public void SetTheme(BiomeTheme theme)
    {
        _currentTheme = theme;
    }

    public void RenderDoors(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllDoorTiles == null) return;
        
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
            }
        }
    }

    private GameObject CreateDoorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f); // Door at floor level
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject doorPrefabToUse = prefab ?? _doorPrefab;
        
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