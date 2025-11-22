using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main orchestrator for dungeon rendering using the new consolidated rendering systems.
/// Handles layout, props, decor, and environment rendering.
/// </summary>
public class DungeonRenderer : MonoBehaviour
{
    [Header("Rendering Settings")]
    public bool EnableCollision = true;
    public bool EnableCeiling = true;
    public bool EnableVoid = true;
    
    [Header("Parent Transforms")]
    public Transform LayoutParent;
    public Transform PropsParent;
    public Transform DecorParent;
    public Transform EnvironmentParent;
    
    // Consolidated rendering systems
    private LayoutRenderer _layoutRenderer;
    private PropRenderer _propRenderer;
    private DecorRenderer _decorRenderer;
    private BiomeManager _biomeManager;

    private bool _isInitialized = false;

    /// <summary>
    /// Initializes the rendering systems with required dependencies.
    /// </summary>
    public void Initialize(BiomeManager biomeManager)
    {
        if (biomeManager == null)
        {
            Debug.LogError("DungeonRenderer.Initialize(): BiomeManager cannot be null");
            return;
        }

        _biomeManager = biomeManager;
        
        // Initialize consolidated rendering systems
        _layoutRenderer = new LayoutRenderer(_biomeManager);
        _propRenderer = new PropRenderer(_biomeManager);
        _decorRenderer = new DecorRenderer(_biomeManager);
        
        CreateParentContainers();
        _isInitialized = true;
        
        Debug.Log("DungeonRenderer: Rendering systems initialized successfully");
    }

    /// <summary>
    /// Renders the complete dungeon using the consolidated rendering systems.
    /// </summary>
    public void RenderDungeon(LevelModel layout, List<RoomModel> rooms, BiomeModel biome)
    {
        if (!_isInitialized)
        {
            Debug.LogError("DungeonRenderer not initialized. Call Initialize() first.");
            return;
        }

        if (layout == null)
        {
            Debug.LogError("DungeonRenderer.RenderDungeon(): Layout cannot be null");
            return;
        }

        if (biome == null)
        {
            Debug.LogError("DungeonRenderer.RenderDungeon(): Biome cannot be null");
            return;
        }

        Debug.Log($"Starting dungeon rendering for biome: {biome.Name}");

        try
        {
            // Render all components using consolidated systems
            RenderLayout(layout, biome);
            RenderProps(layout, rooms, biome);
            RenderDecor(layout, rooms, biome);
            RenderEnvironment(layout, biome);
            
            Debug.Log("Dungeon rendering completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Dungeon rendering failed: {e.Message}");
        }
    }

    /// <summary>
    /// Renders the structural layout (floors, walls, doors, ceiling, void).
    /// </summary>
    private void RenderLayout(LevelModel layout, BiomeModel biome)
    {
        if (_layoutRenderer == null)
        {
            Debug.LogError("LayoutRenderer is not initialized");
            return;
        }

        Debug.Log("Rendering layout...");
        _layoutRenderer.RenderCompleteLayout(layout, biome, LayoutParent);
    }

    /// <summary>
    /// Renders functional props (entrance, exit, shop, treasure, etc.).
    /// </summary>
    private void RenderProps(LevelModel layout, List<RoomModel> rooms, BiomeModel biome)
    {
        if (_propRenderer == null)
        {
            Debug.LogError("PropRenderer is not initialized");
            return;
        }

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No rooms provided for prop rendering");
            return;
        }

        Debug.Log("Rendering props...");
        _propRenderer.RenderAllProps(layout, rooms, biome, PropsParent);
    }

    /// <summary>
    /// Renders decorative objects (trees, rocks, etc.).
    /// </summary>
    private void RenderDecor(LevelModel layout, List<RoomModel> rooms, BiomeModel biome)
    {
        if (_decorRenderer == null)
        {
            Debug.LogError("DecorRenderer is not initialized");
            return;
        }

        Debug.Log("Rendering decor...");
        _decorRenderer.RenderAllDecor(layout, rooms, biome, DecorParent);
    }

    /// <summary>
    /// Renders environment elements (ceiling, void plane).
    /// </summary>
    private void RenderEnvironment(LevelModel layout, BiomeModel biome)
    {
        if (!EnableCeiling && !EnableVoid) return;

        Debug.Log("Rendering environment...");
        
        if (EnableCeiling)
        {
            _layoutRenderer.RenderCeiling(layout, biome, EnvironmentParent);
        }
        
        if (EnableVoid)
        {
            _layoutRenderer.RenderVoidPlane(layout, biome, EnvironmentParent);
        }
    }

    /// <summary>
    /// Clears all rendered dungeon geometry.
    /// </summary>
    public void ClearRendering()
    {
        Debug.Log("Clearing dungeon rendering...");
        
        ClearChildObjects(LayoutParent);
        ClearChildObjects(PropsParent);
        ClearChildObjects(DecorParent);
        ClearChildObjects(EnvironmentParent);
        
        Debug.Log("Dungeon rendering cleared");
    }

    #region Utility Methods

    private void CreateParentContainers()
    {
        LayoutParent = CreateParentIfNull(LayoutParent, "Layout");
        PropsParent = CreateParentIfNull(PropsParent, "Props");
        DecorParent = CreateParentIfNull(DecorParent, "Decor");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
        
        // Set all parents as children of this transform
        LayoutParent.SetParent(transform);
        PropsParent.SetParent(transform);
        DecorParent.SetParent(transform);
        EnvironmentParent.SetParent(transform);
    }

    private Transform CreateParentIfNull(Transform parent, string name)
    {
        if (parent != null) return parent;
        
        var newParent = new GameObject(name).transform;
        newParent.SetParent(transform);
        newParent.localPosition = Vector3.zero;
        return newParent;
    }

    private void ClearChildObjects(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
    }

    /// <summary>
    /// Gets the total number of rendered objects across all categories.
    /// </summary>
    public int GetTotalRenderedObjects()
    {
        int total = 0;
        total += LayoutParent?.childCount ?? 0;
        total += PropsParent?.childCount ?? 0;
        total += DecorParent?.childCount ?? 0;
        total += EnvironmentParent?.childCount ?? 0;
        return total;
    }

    /// <summary>
    /// Logs rendering statistics for debugging.
    /// </summary>
    public void LogRenderingStats()
    {
        Debug.Log($"Rendering Stats - Layout: {LayoutParent?.childCount ?? 0}, " +
                 $"Props: {PropsParent?.childCount ?? 0}, " +
                 $"Decor: {DecorParent?.childCount ?? 0}, " +
                 $"Environment: {EnvironmentParent?.childCount ?? 0}");
    }

    #endregion
}