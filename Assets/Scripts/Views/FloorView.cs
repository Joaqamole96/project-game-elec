// ================================= //
// FloorView.cs
// 
// Handles the visual rendering of the generated floor 
// using either Prefabs or Gizmos.
// 
// ** FIX: Updated to use a unified Tile Map for optimal performance and 
// ** elimination of overlapping GameObjects, resolving the memory issue.
// ================================= //

using UnityEngine;
using System.Collections.Generic;
using static FloorController; // Allows access to TileType enum

// Enum to select the rendering method
public enum FloorDisplayMode { PrefabMode, GizmoMode }

// -------- 2. Floor View -------- //

public class FloorView : MonoBehaviour
{
    // ----- Configuration ----- //
    [Header("Display Configuration")]
    [Tooltip("Selects whether to instantiate prefabs or draw debugging Gizmos.")]
    public FloorDisplayMode displayMode = FloorDisplayMode.GizmoMode;

    // ----- Dependencies (Prefab Mode Only) ----- //
    
    [Header("Prefab Dependencies (Prefab Mode)")]
    [Tooltip("Prefab for wall tiles.")]
    public GameObject WallTilePrefab; 
    
    [Tooltip("Prefab for floor tiles (used for rooms and paths).")]
    public GameObject FloorTilePrefab;

    [Tooltip("Parent transform for instantiated tiles to keep the hierarchy clean.")]
    public Transform TileParent;

    // ----- Internal State ----- //

    // Stores the model/map so OnDrawGizmos can access it outside the main thread flow
    private FloorModel modelToRender;
    private TileType[,] mapToRender; // NEW: Unified map for fast rendering
    private const float Y_LEVEL = 0f; // The fixed Y-coordinate for the flat floor plane.

    // ----- Main Methods ----- //

    // --- Render Floor --- //
    // Public method to be called by the Controller to initiate rendering.
    // MODIFIED: New tileMap parameter is critical for performance fix.
    public void RenderFloor(FloorModel model, TileType[,] tileMap) 
    {
        // Store both the model (for Gizmo partition debug) and the map (for efficient rendering)
        modelToRender = model;
        mapToRender = tileMap;

        Debug.Log($"View: Starting rendering in {displayMode}...");

        if (model.RootPartition == null)
        {
            Debug.LogError("Cannot render: RootPartition is null. Generate floor data first.");
            return;
        }

        if (displayMode == FloorDisplayMode.PrefabMode)
        {
            RenderFloorInPrefabMode(tileMap); // Pass the map directly
        }
        else // GizmoMode
        {
            // No action needed here; OnDrawGizmos will be called automatically by Unity
        }
    }
    
    // --- OnDrawGizmos --- //
    // Called in the editor to draw gizmos when the object is selected.
    private void OnDrawGizmos()
    {
        if (displayMode == FloorDisplayMode.GizmoMode && modelToRender != null && mapToRender != null)
        {
            // Draw the root bounds once
            Gizmos.color = Color.black;
            DrawRectGizmo(modelToRender.RootPartition.Area, true);

            // Draw all generated rooms and paths using the unified map
            DrawFloorInGizmoMode(modelToRender, mapToRender);
        }
    }

    // ----- Prefab Rendering Methods (Optimized) ----- //
    
    // --- Prefab Mode Entry Point ---
    // This function is the core performance fix.
    private void RenderFloorInPrefabMode(TileType[,] map) 
    {
        // 1. Clear any previous tiles
        ClearFloor();

        // 2. Instantiate Tiles based ONLY on the unified map.
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int tileCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileType type = map[x, y];
                GameObject prefab = null;

                if (type == TileType.Floor)
                {
                    prefab = FloorTilePrefab;
                }
                else if (type == TileType.Wall)
                {
                    prefab = WallTilePrefab;
                }
                
                if (prefab != null)
                {
                    InstantiateTile(prefab, x, y);
                    tileCount++;
                }
            }
        }
        
        Debug.Log($"View: Successfully rendered {tileCount} unique tiles in Prefab Mode. (Memory/Performance Optimized)");
    }
    
    // --- Clear Floor (Prefab Helper) ---
    private void ClearFloor()
    {
        if (TileParent != null)
        {
            // Note: This relies on TileParent being a distinct root for the floor tiles.
            for (int i = TileParent.childCount - 1; i >= 0; i--)
            {
                // DestroyImmediate is used in Editor context (like [ContextMenu]) for immediate cleanup
                DestroyImmediate(TileParent.GetChild(i).gameObject); 
            }
        }
    }

    // --- Instantiate Tile (Prefab Helper) ---
    private void InstantiateTile(GameObject prefab, int x, int y)
    {
        // Map grid Y to world Z, and set world Y (height) to Y_LEVEL (0)
        Instantiate(prefab, new Vector3(x + 0.5f, Y_LEVEL, y + 0.5f), Quaternion.identity, TileParent); 
    }
    
    // ----- Gizmo Rendering Methods (Optimized) ----- //

    // --- Gizmo Mode Entry Point ---
    private void DrawFloorInGizmoMode(FloorModel model, TileType[,] map) 
    {
        // 1. Draw Partitions (for debugging BSP bounds)
        foreach (var partition in model.Partitions)
        {
            DrawPartitionBounds(partition); // Draw only the bounds for debug
        }

        // 2. Draw Tiles based on the map (Floors and Walls)
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileType type = map[x, y];
                
                if (type == TileType.Floor)
                {
                    DrawTileGizmo(new Vector2Int(x, y), Color.green); // Floor is green
                }
                else if (type == TileType.Wall)
                {
                    DrawTileGizmo(new Vector2Int(x, y), Color.red); // Wall is red
                }
            }
        }
    }
    
    // --- Draw Rect Gizmo Helper ---
    private void DrawRectGizmo(RectInt rect, bool filled = false)
    {
        // Map grid Y to world Z, and set world Y (height) to Y_LEVEL (0)
        Vector3 center = new Vector3(rect.x + rect.width * 0.5f, Y_LEVEL, rect.y + rect.height * 0.5f);
        // Use a small height (Y) for the Gizmo so it's visible but flat
        Vector3 size = new Vector3(rect.width, 0.1f, rect.height);

        if (filled)
        {
            Gizmos.DrawCube(center, size);
        }
        else
        {
            Gizmos.DrawWireCube(center, size);
        }
    }

    // --- Draw Tile Gizmo Helper ---
    private void DrawTileGizmo(Vector2Int tileCoords, Color color)
    {
        Gizmos.color = color;
        // Center the cube on the tile coordinate
        Vector3 tileCenter = new Vector3(tileCoords.x + 0.5f, Y_LEVEL, tileCoords.y + 0.5f);
        // Use 0.9 size to fill the whole tile
        Gizmos.DrawCube(tileCenter, new Vector3(0.9f, 0.1f, 0.9f)); 
    }

    // --- Draw Partition Bounds Only ---
    private void DrawPartitionBounds(Partition partition)
    {
        // Draw the partition bounds (for debugging BSP)
        Gizmos.color = Color.Lerp(Color.blue, Color.cyan, (float)partition.Depth / 8f);
        DrawRectGizmo(partition.Area, false); 

        // Draw the room center
        if (partition.Room.HasValue)
        {
            RectInt room = partition.Room.Value;
            Gizmos.color = Color.white;
            // Draw slightly above the floor for visibility
            Gizmos.DrawSphere(new Vector3(room.center.x, Y_LEVEL + 0.1f, room.center.y), 0.2f);
        }
    }
}
