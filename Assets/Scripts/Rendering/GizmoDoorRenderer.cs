// GizmoDoorRenderer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders doors in Gizmo mode using colored cubes for debugging and visualization.
/// </summary>
public class GizmoDoorRenderer : IDoorRenderer
{
    private MaterialService _materialService;

    public GizmoDoorRenderer(MaterialService materialService)
    {
        _materialService = materialService;
    }

    /// <summary>
    /// Renders all doors in the layout as individual cubes.
    /// </summary>
    public void RenderDoors(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllDoorTiles == null) return;
        
        foreach (var doorPos in layout.AllDoorTiles)
        {
            var door = CreateDoorAtPosition(doorPos);
            if (door != null)
            {
                door.transform.SetParent(parent);
                
                if (enableCollision && door != null) 
                {
                    AddCollisionToObject(door, "Door");
                }
            }
        }
    }

    private GameObject CreateDoorAtPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = new(gridPos.x + 0.5f, 0.4f, gridPos.y + 0.5f);
        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.position = worldPos;
        door.transform.localScale = new Vector3(1f, 0.8f, 1f);
        door.name = $"Door_{gridPos.x}_{gridPos.y}";
        
        // Add DoorController and configure
        var doorController = door.AddComponent<DoorController>();
        doorController.doorModel = door; // The cube itself is the model
        
        ApplyDoorMaterial(door);
        return door;
    }

    private void ApplyDoorMaterial(GameObject obj)
    {
        if (obj == null) return;
        
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = _materialService.GetDoorMaterial();
        }
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