// -------------------------------------------------- //
// Scripts/Generators/NavMeshGenerator.cs
// -------------------------------------------------- //

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
using Unity.AI.Navigation;
#endif

/// <summary>
/// Generates NavMesh at runtime for procedurally generated dungeons.
/// Supports both Editor and Runtime baking.
/// </summary>
public class NavMeshGenerator : MonoBehaviour
{
    [Header("NavMesh Settings")]
    public float agentRadius = 0.5f;
    public float agentHeight = 2f;
    public float maxSlope = 45f;
    public float stepHeight = 0.4f;
    
    [Header("Build Settings")]
    public int agentTypeID = 0; // Humanoid agent type
    public bool bakeOnGeneration = true;
    public LayerMask walkableLayers = ~0; // All layers by default
    
    private NavMeshSurface navMeshSurface;
    private List<GameObject> temporaryColliders = new();
    
    void Awake()
    {
        EnsureNavMeshSurfaceExists();
    }
    
    /// <summary>
    /// Ensures NavMeshSurface component exists, creates if missing.
    /// </summary>
    private void EnsureNavMeshSurfaceExists()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        
        if (navMeshSurface == null)
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            Debug.Log("Created NavMeshSurface component");
        }
        
        ConfigureNavMeshSurface();
    }
    
    private void ConfigureNavMeshSurface()
    {
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.layerMask = walkableLayers;
        
        // Set build settings
        var buildSettings = navMeshSurface.GetBuildSettings();
        buildSettings.agentRadius = agentRadius;
        buildSettings.agentHeight = agentHeight;
        buildSettings.agentSlope = maxSlope;
        buildSettings.agentClimb = stepHeight;
        buildSettings.agentTypeID = agentTypeID;
    }
    
    /// <summary>
    /// Bakes NavMesh for the entire dungeon layout.
    /// Call this after dungeon generation completes.
    /// </summary>
    public void BakeNavMesh(LevelModel layout, Transform floorsParent, Transform wallsParent)
    {
        if (layout == null)
        {
            Debug.LogError("Cannot bake NavMesh: layout is null");
            return;
        }
        
        Debug.Log("Starting NavMesh baking...");
        
        // Clear any existing NavMesh
        ClearNavMesh();
        
        // Ensure all geometry has colliders
        EnsureCollidersExist(floorsParent, wallsParent, layout);
        
        // Set baking bounds
        SetBakingBounds(layout);
        
        // Bake the NavMesh
        navMeshSurface.BuildNavMesh();
        
        Debug.Log($"NavMesh baked successfully! Area: {layout.OverallBounds.size.x}x{layout.OverallBounds.size.y}");
    }
    
    private void EnsureCollidersExist(Transform floorsParent, Transform wallsParent, LevelModel layout)
    {
        // Clean up any previous temporary colliders
        CleanupTemporaryColliders();
        
        // Ensure floor colliders exist (for walkable surface)
        if (floorsParent != null)
        {
            EnsureFloorColliders(floorsParent);
        }
        else
        {
            // Create temporary floor plane if no floors exist
            CreateTemporaryFloorColliders(layout);
        }
        
        // Ensure wall colliders exist (for obstacles)
        if (wallsParent != null)
        {
            EnsureWallColliders(wallsParent);
        }
    }
    
    private void EnsureFloorColliders(Transform floorsParent)
    {
        int collidersAdded = 0;
        
        foreach (Transform child in floorsParent)
        {
            // Check all floor children
            if (child.GetComponent<MeshFilter>() != null)
            {
                
                if (!child.TryGetComponent<MeshCollider>(out var existingCollider))
                {
                    var meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = false;
                    meshCollider.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
                    collidersAdded++;
                }
                else
                {
                    // Ensure mesh is assigned
                    if (existingCollider.sharedMesh == null)
                    {
                        existingCollider.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
                    }
                }
            }
        }
        
        if (collidersAdded > 0)
        {
            Debug.Log($"Added {collidersAdded} floor colliders for NavMesh");
        }
    }
    
    private void EnsureWallColliders(Transform wallsParent)
    {
        int collidersAdded = 0;
        
        foreach (Transform child in wallsParent)
        {
            if (child.GetComponent<Collider>() == null)
            {
                var boxCollider = child.gameObject.AddComponent<BoxCollider>();
                collidersAdded++;
            }
        }
        
        if (collidersAdded > 0)
        {
            Debug.Log($"Added {collidersAdded} wall colliders for NavMesh");
        }
    }
    
    private void CreateTemporaryFloorColliders(LevelModel layout)
    {
        Debug.Log("Creating temporary floor colliders for NavMesh baking");

        // Create one large box collider for the entire floor
        GameObject tempFloor = new("TempNavMeshFloor")
        {
            layer = LayerMask.NameToLayer("Default")
        };

        var bounds = layout.OverallBounds;
        tempFloor.transform.position = new Vector3(bounds.center.x, 0f, bounds.center.y);
        
        var boxCollider = tempFloor.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(bounds.size.x, 0.1f, bounds.size.y);
        boxCollider.center = Vector3.zero;
        
        temporaryColliders.Add(tempFloor);
    }
    
    private void SetBakingBounds(LevelModel layout)
    {
        var bounds = layout.OverallBounds;
        
        // Create bounding volume that covers ENTIRE dungeon
        Vector3 center = new(
            bounds.center.x, 
            1f,  // Center at floor height
            bounds.center.y
        );
        
        Vector3 size = new(
            bounds.size.x + 20f,  // Add padding
            10f,  // Height for agents
            bounds.size.y + 20f   // Add padding
        );
        
        navMeshSurface.center = center;
        navMeshSurface.size = size;
        
        Debug.Log($"NavMesh baking bounds: Center={center}, Size={size}, Dungeon Bounds={bounds}");
    }
    
    /// <summary>
    /// Clears the existing NavMesh data.
    /// </summary>
    public void ClearNavMesh()
    {
        if (navMeshSurface != null && navMeshSurface.navMeshData != null)
        {
            navMeshSurface.RemoveData();
            Debug.Log("Cleared existing NavMesh data");
        }
        
        CleanupTemporaryColliders();
    }
    
    private void CleanupTemporaryColliders()
    {
        foreach (var tempCollider in temporaryColliders)
        {
            if (tempCollider != null)
            {
                Destroy(tempCollider);
            }
        }
        
        temporaryColliders.Clear();
    }
    
    /// <summary>
    /// Updates NavMesh in a specific area (useful for dynamic changes).
    /// </summary>
    public void UpdateNavMeshArea(Bounds bounds)
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            Debug.Log("Updated NavMesh in area");
        }
    }
    
    /// <summary>
    /// Checks if a position is on the NavMesh.
    /// </summary>
    public bool IsPositionOnNavMesh(Vector3 position, float maxDistance = 2f)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas);
    }
    
    /// <summary>
    /// Gets the nearest valid NavMesh position to the given point.
    /// </summary>
    public Vector3 GetNearestNavMeshPosition(Vector3 position, float maxDistance = 5f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        Debug.LogWarning($"No NavMesh found near position {position}");
        return position;
    }
    
    void OnDestroy()
    {
        CleanupTemporaryColliders();
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (navMeshSurface != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(navMeshSurface.center, navMeshSurface.size);
        }
    }
    #endif
}