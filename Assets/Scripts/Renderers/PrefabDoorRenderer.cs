// -------------------------------------------------- //
// Scripts/Renderers/PrefabDoorRenderer.cs
// -------------------------------------------------- //

using UnityEngine;

public class PrefabDoorRenderer : IDoorRenderer
{
    private GameObject _fallbackDoorPrefab;
    private BiomeManager _biomeManager;
    private BiomeModel _currentBiome;

    public PrefabDoorRenderer(GameObject doorPrefab, BiomeManager biomeManager)
    {
        _fallbackDoorPrefab = doorPrefab;
        _biomeManager = biomeManager;
    }

    public void RenderDoors(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllDoorTiles == null) throw new("Cannot render prefab doors: layout or door tiles is null");
        
        int doorsCreated = 0;
        foreach (var doorPos in layout.AllDoorTiles)
        {
            var doorPrefab = _biomeManager.GetDoorPrefab(_currentBiome);
            var door = CreateDoorAtPosition(doorPos, doorPrefab);
            
            if (door != null)
            {
                door.transform.SetParent(parent);
                if (enableCollision) AddCollisionToObject(door, "Door");
                doorsCreated++;
            }
        }

        Debug.Log($"Created {doorsCreated} prefab doors");
    }

    private GameObject CreateDoorAtPosition(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f);
        
        GameObject doorPrefabToUse = prefab != null ? prefab : _fallbackDoorPrefab;
        
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
        if (obj.GetComponent<Collider>() == null) obj.AddComponent<BoxCollider>();
        if (objectType == "Door")
        {
            if (!obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
    }
}