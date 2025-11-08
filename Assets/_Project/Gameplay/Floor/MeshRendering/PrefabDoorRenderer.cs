using UnityEngine;
using System.Collections.Generic;

public class PrefabDoorRenderer : IDoorRenderer
{
    private GameObject _doorPrefab;
    private MaterialManager _materialManager;
    private BiomeManager _biomeManager;

    public PrefabDoorRenderer(GameObject doorPrefab, MaterialManager materialManager)
    {
        _doorPrefab = doorPrefab;
        _materialManager = materialManager;
        _biomeManager = new BiomeManager();
    }

    public void RenderDoors(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllDoorTiles == null) return;
        
        foreach (var doorPos in layout.AllDoorTiles)
        {
            var doorPrefab = _biomeManager.GetDoorPrefab(doorPos);
            
            var door = CreateDoorAtPosition(doorPos, doorPrefab);
            door.transform.SetParent(parent);
            
            if (enableCollision && door != null) 
            {
                AddCollisionToObject(door, "Door");
            }
        }
    }

    private GameObject CreateDoorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0.4f, gridPos.y + 0.5f);
        
        // Use the biome-specific prefab if available, otherwise use fallback
        GameObject doorPrefabToUse = prefab ?? _doorPrefab;
        
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