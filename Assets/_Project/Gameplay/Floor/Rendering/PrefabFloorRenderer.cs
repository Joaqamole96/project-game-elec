using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrefabFloorRenderer : IFloorRenderer
{
    private GameObject _floorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;
    private BiomeTheme _currentTheme;

    public PrefabFloorRenderer(GameObject floorPrefab, MaterialManager materialManager, BiomeManager biomeManager)
    {
        _floorPrefab = floorPrefab;
        _materialManager = materialManager;
        _biomeManager = biomeManager;
    }

    // NEW: Set the current theme
    public void SetTheme(BiomeTheme theme)
    {
        _currentTheme = theme;
    }

    public List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        RenderIndividualFloors(layout, rooms, parent, false);
        return new List<GameObject>();
    }

    public void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision)
    {
        foreach (var floorPos in layout.AllFloorTiles)
        {
            // Use the current theme's floor prefab
            var floorPrefab = _biomeManager.GetFloorPrefab(_currentTheme);
            
            var floor = CreateFloorAtPosition(floorPos, floorPrefab);
            if (floor != null)
            {
                floor.transform.SetParent(parent);
                
                if (enableCollision)
                    AddCollisionToObject(floor, "Floor");
            }
        }
    }

    private GameObject CreateFloorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f);
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject floorPrefabToUse = prefab ?? _floorPrefab;
        
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