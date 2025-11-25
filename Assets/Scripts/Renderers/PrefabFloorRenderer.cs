// -------------------------------------------------- //
// Scripts/Renderers/PrefabFloorRenderer.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class PrefabFloorRenderer
{
    private GameObject _fallbackFloorPrefab;
    private BiomeManager _biomeManager;

    public PrefabFloorRenderer(GameObject floorPrefab, BiomeManager biomeManager)
    {
        _fallbackFloorPrefab = floorPrefab;
        _biomeManager = biomeManager;
    }

    public List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent)
    {
        RenderIndividualFloors(layout, rooms, parent, false);
        return new List<GameObject>();
    }

    public void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision)
    {
        if (layout?.AllFloorTiles == null) 
            throw new System.Exception("Cannot render prefab floors: layout or floor tiles is null");

        // Get current biome as STRING
        string currentBiome = _biomeManager.CurrentBiome;

        int floorsCreated = 0;
        foreach (var floorPos in layout.AllFloorTiles)
        {
            var floorPrefab = _biomeManager.GetFloorPrefab(currentBiome); // FIXED: Pass string
            var floor = CreateFloorAtPosition(floorPos, floorPrefab);
            
            if (floor != null)
            {
                floor.transform.SetParent(parent);
                if (enableCollision) AddCollisionToObject(floor);
                floorsCreated++;
            }
        }

        Debug.Log($"Created {floorsCreated} prefab floors");
    }

    private GameObject CreateFloorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f);
        
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

    private void AddCollisionToObject(GameObject obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null) 
            obj.AddComponent<BoxCollider>();
    }
}