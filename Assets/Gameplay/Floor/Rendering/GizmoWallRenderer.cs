// GizmoWallRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Renders walls in Gizmo mode using colored cubes for debugging and visualization.
/// </summary>
public class GizmoWallRenderer : IWallRenderer
{
    private MaterialManager _materialManager;
    private MeshCombiner _meshCombiner;

    public GizmoWallRenderer(MaterialManager materialManager)
    {
        _materialManager = materialManager;
        _meshCombiner = new MeshCombiner();
    }

    /// <summary>
    /// Renders walls as combined meshes grouped by wall type for optimal performance.
    /// </summary>
    public List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null)
        {
            Debug.LogError("Cannot render combined walls: layout data is null");
            return new List<GameObject>();
        }

        var wallGroups = GroupWallTilesByType(layout);
        var meshContainers = new List<GameObject>();

        foreach (var wallGroup in wallGroups)
        {
            if (wallGroup.Value.Count > 0)
            {
                var combinedMesh = _meshCombiner.CreateCombinedMesh(wallGroup.Value, $"Walls_{wallGroup.Key}", parent);
                if (combinedMesh != null)
                {
                    ApplyWallMaterial(combinedMesh, wallGroup.Key);
                    meshContainers.Add(combinedMesh);
                }
            }
        }

        return meshContainers;
    }

    /// <summary>
    /// Renders walls as individual cubes for detailed debugging.
    /// </summary>
    public void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null) return;

        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                var wall = CreateWallAtPosition(wallPos, wallType);
                if (wall != null)
                {
                    wall.transform.SetParent(parent);
                    
                    if (enableCollision)
                        AddCollisionToObject(wall, "Wall");
                }
            }
        }
    }

    private Dictionary<WallType, List<Vector3>> GroupWallTilesByType(LevelModel layout)
    {
        var wallGroups = new Dictionary<WallType, List<Vector3>>();
        
        foreach (var wallPos in layout.AllWallTiles)
        {
            if (layout.WallTypes.TryGetValue(wallPos, out var wallType))
            {
                if (!wallGroups.ContainsKey(wallType))
                    wallGroups[wallType] = new List<Vector3>();
                    
                wallGroups[wallType].Add(new Vector3(wallPos.x + 0.5f, 0.5f, wallPos.y + 0.5f));
            }
        }

        return wallGroups;
    }

    private GameObject CreateWallAtPosition(Vector2Int gridPos, WallType wallType)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0.5f, gridPos.y + 0.5f);
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = worldPos;
        wall.transform.localScale = new Vector3(1f, 1f, 1f);
        wall.name = $"Wall_{wallType}_{gridPos.x}_{gridPos.y}";
        ApplyWallMaterial(wall, wallType);
        return wall;
    }

    private void ApplyWallMaterial(GameObject obj, WallType wallType)
    {
        if (obj == null) return;
        
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = _materialManager.GetWallMaterial(wallType);
        }
    }

    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }
}