// -------------------------------------------------- //
// Scripts/Renderers/LandmarkRenderer.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class LandmarkRenderer
{
    public void RenderLandmarks(List<RoomModel> rooms, Transform parent)
    {
        if (rooms == null || parent == null) throw new System.Exception("Cannot render special objects: rooms or parent is null");
        int landmarksCreated = 0;
        foreach (var room in rooms) if (room != null && IsSpecialRoomType(room.Type) && RenderRoomLandmark(room, parent)) landmarksCreated++;
        Debug.Log($"Created {landmarksCreated} special room objects");
    }

    private bool IsSpecialRoomType(RoomType roomType)
        => roomType == RoomType.Entrance || 
            roomType == RoomType.Exit || 
            roomType == RoomType.Shop || 
            roomType == RoomType.Treasure;

    private bool RenderRoomLandmark(RoomModel room, Transform parent)
    {
        GameObject prefab = ResourceService.LoadLandmarkPrefab(room.Type);
        if (prefab != null)
        {
            // FIXED: Calculate proper ground-level position
            Vector3 position = CalculateGroundPosition(room, prefab);
            
            var landmark = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            landmark.name = $"{room.Type}_{room.ID}";
            
            // Ensure proper colliders
            SetupLandmarkCollider(landmark, room.Type);
            
            Debug.Log($"LandmarkRenderer: Spawned {room.Type} at ground level: {position}");
            
            return true;
        }
        else
        {
            Debug.LogWarning($"No prefab available for {room.Type} room {room.ID}");
            return false;
        }
    }

    private Vector3 CalculateGroundPosition(RoomModel room, GameObject prefab)
    {
        // Get prefab bounds to calculate proper Y offset
        Bounds bounds = GetPrefabBounds(prefab);
        
        // Calculate Y position so bottom of prefab is at ground level
        float groundY = 0f; // Floor is at Y=0
        float prefabHalfHeight = bounds.extents.y;
        float yPosition = groundY + prefabHalfHeight;
        
        // For special cases (shop, treasure), place directly on ground
        return new Vector3(room.Center.x + 0.5f, yPosition, room.Center.y + 0.5f);
    }

    private Bounds GetPrefabBounds(GameObject prefab)
    {
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        
        // Default bounds if no renderer
        return new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f));
    }

    private void SetupLandmarkCollider(GameObject landmark, RoomType roomType)
    {
        // Ensure proper collider exists and is configured
        Collider col = landmark.GetComponent<Collider>();
        
        if (col == null)
        {
            // Add appropriate collider based on room type
            switch (roomType)
            {
                case RoomType.Shop:
                case RoomType.Treasure:
                    SphereCollider sphere = landmark.AddComponent<SphereCollider>();
                    sphere.isTrigger = true;
                    sphere.radius = 3f;
                    sphere.center = Vector3.zero;
                    Debug.Log($"Added SphereCollider to {roomType}");
                    break;
                    
                case RoomType.Exit:
                case RoomType.Entrance:
                    CapsuleCollider capsule = landmark.AddComponent<CapsuleCollider>();
                    capsule.isTrigger = true;
                    capsule.radius = 2f;
                    capsule.height = 3f;
                    capsule.center = Vector3.zero;
                    Debug.Log($"Added CapsuleCollider to {roomType}");
                    break;
            }
        }
        else
        {
            // Ensure existing collider is properly configured
            col.isTrigger = true;
            Debug.Log($"Configured existing collider for {roomType}");
        }
    }
}