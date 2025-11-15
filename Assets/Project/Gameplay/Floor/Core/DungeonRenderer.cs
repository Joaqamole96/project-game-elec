// DungeonRenderer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles rendering of dungeon geometry in both debug (Gizmo) and gameplay (Real) modes.
/// Supports mesh combining for performance optimization.
/// </summary>
public class DungeonRenderer : MonoBehaviour
{
    [Header("Rendering Mode")]
    public RenderMode Mode = RenderMode.Real;
    
    [Header("Prefabs - Real Mode")]
    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject DoorPrefab;
    public GameObject EntrancePrefab;
    public GameObject ExitPrefab;

    [Header("Fallback Prefabs - Real Mode")]
    public GameObject FallbackFloorPrefab;
    public GameObject FallbackWallPrefab;
    public GameObject FallbackDoorPrefab;
    public GameObject FallbackDoorTopPrefab;

    [Header("Environment Settings")]
    public bool EnableCeiling = true;
    public bool EnableVoid = true;
    
    [Header("Parent Transforms")]
    public Transform FloorsParent;
    public Transform WallsParent;
    public Transform DoorsParent;
    public Transform SpecialObjectsParent;
    public Transform EnvironmentParent;
    
    [Header("Mobile Optimization")]
    public bool CombineMeshes = true;
    public bool EnableFloorCollision = true;
    public bool EnableWallCollision = true;
    public bool EnableDoorCollision = false;
    
    [Header("Material Settings - Gizmo Mode")]
    public Material DefaultFloorMaterial;
    public Material DefaultWallMaterial;
    public Material DefaultDoorMaterial;
    
    // Rendering components
    private IFloorRenderer _floorRenderer;
    private IWallRenderer _wallRenderer;
    private IDoorRenderer _doorRenderer;
    private SpecialRoomRenderer _specialRenderer;
    private MaterialManager _materialManager;
    private OptimizedPrefabRenderer _optimizedRenderer;
    private BiomeManager _biomeManager;

    // Combined mesh containers
    private readonly List<GameObject> _spawnedContainers = new List<GameObject>();
    
    public enum RenderMode
    {
        Gizmo,    // Colored cubes for debugging
        Real      // Actual prefabs for gameplay
    }

    void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _materialManager = new MaterialManager(DefaultFloorMaterial, DefaultWallMaterial, DefaultDoorMaterial);
        _biomeManager = new BiomeManager();
        _optimizedRenderer = new OptimizedPrefabRenderer(_biomeManager);
        
        InitializeRenderers();
        
        _specialRenderer = new SpecialRoomRenderer(EntrancePrefab, ExitPrefab, _biomeManager);
    }

    private void InitializeRenderers()
    {
        if (Mode == RenderMode.Gizmo)
        {
            _floorRenderer = new GizmoFloorRenderer(_materialManager);
            _wallRenderer = new GizmoWallRenderer(_materialManager);
            _doorRenderer = new GizmoDoorRenderer(_materialManager);
        }
        else
        {
            _floorRenderer = new PrefabFloorRenderer(FallbackFloorPrefab, _materialManager, _biomeManager);
            _wallRenderer = new PrefabWallRenderer(FallbackWallPrefab, _materialManager, _biomeManager);
            _doorRenderer = new PrefabDoorRenderer(FallbackDoorPrefab, _materialManager, _biomeManager);
        }
    }

    private void EnsureComponentsInitialized()
    {
        if (_materialManager == null)
            InitializeComponents();
    }

    /// <summary>
    /// Renders the complete dungeon using the specified layout and room data.
    /// </summary>
    public void RenderDungeon(LevelModel layout, List<RoomModel> rooms, int floorLevel, int seed)
    {
        EnsureComponentsInitialized();
        ClearRendering();
        CreateParentContainers();

        if (Mode == RenderMode.Gizmo)
        {
            RenderGizmoMode(layout, rooms);
        }
        else
        {
            RenderRealMode(layout, floorLevel, seed);
        }
        
        RenderSpecialObjects(layout, rooms);
        LogRenderingResults();
    }

    private void RenderGizmoMode(LevelModel layout, List<RoomModel> rooms)
    {
        _materialManager.InitializeMaterialCache();
        RenderFloors(layout, rooms);
        RenderWalls(layout);
        RenderDoors(layout);
    }

    private void RenderRealMode(LevelModel layout, int floorLevel, int seed)
    {
        _optimizedRenderer.SetThemeForFloor(floorLevel, seed);
        
        // Queue all geometry for combining
        _optimizedRenderer.RenderFloorsOptimized(layout, FloorsParent);
        _optimizedRenderer.RenderWallsOptimized(layout, WallsParent);
        _optimizedRenderer.RenderDoorsOptimized(layout, DoorsParent);
        
        // Build all combined meshes
        _optimizedRenderer.FinalizeRendering(FloorsParent);
        
        // Environment elements
        RenderEnvironment(layout);
    }

    private void RenderEnvironment(LevelModel layout)
    {
        if (EnableCeiling) _optimizedRenderer.RenderCeilingOptimized(layout, EnvironmentParent);
        if (EnableVoid) _optimizedRenderer.RenderVoidPlane(layout, EnvironmentParent);
    }

    public void ClearRendering()
    {
        EnsureComponentsInitialized();
        
        ClearSpawnedContainers();
        ClearAllChildObjects();
        CleanupMaterials();
    }

    #region Rendering Orchestration
    private void RenderFloors(LevelModel layout, List<RoomModel> rooms)
    {
        if (layout?.AllFloorTiles == null || rooms == null) 
        {
            Debug.LogError("Cannot render floors: layout or rooms is null");
            return;
        }
        
        Debug.Log($"Starting floor rendering: {layout.AllFloorTiles.Count} floor tiles, {rooms.Count} rooms");
        
        if (CombineMeshes && Mode == RenderMode.Gizmo)
        {
            RenderCombinedFloors(layout, rooms);
        }
        else
        {
            RenderIndividualFloors(layout, rooms);
        }
        
        LogFloorRenderingResults();
    }

    private void RenderCombinedFloors(LevelModel layout, List<RoomModel> rooms)
    {
        Debug.Log("Using combined mesh rendering for floors");
        var floorMeshes = _floorRenderer.RenderCombinedFloorsByRoomType(layout, rooms, FloorsParent);
        _spawnedContainers.AddRange(floorMeshes);
        
        if (EnableFloorCollision)
        {
            foreach (var mesh in floorMeshes)
                AddCollisionToObject(mesh, "Floor");
        }
        
        Debug.Log($"Combined mesh rendering complete: {floorMeshes.Count} mesh objects created");
    }

    private void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms)
    {
        Debug.Log("Using individual floor rendering");
        _floorRenderer.RenderIndividualFloors(layout, rooms, FloorsParent, EnableFloorCollision);
    }

    private void RenderWalls(LevelModel layout)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null) return;
        
        if (CombineMeshes && Mode == RenderMode.Gizmo)
        {
            var wallMeshes = _wallRenderer.RenderCombinedWallsByType(layout, WallsParent);
            _spawnedContainers.AddRange(wallMeshes);
            
            if (EnableWallCollision)
            {
                foreach (var mesh in wallMeshes)
                    AddCollisionToObject(mesh, "Wall");
            }
        }
        else
        {
            _wallRenderer.RenderIndividualWalls(layout, WallsParent, EnableWallCollision);
        }
    }

    private void RenderDoors(LevelModel layout)
    {
        if (layout?.AllDoorTiles == null) return;
        _doorRenderer.RenderDoors(layout, DoorsParent, EnableDoorCollision);
    }

    private void RenderSpecialObjects(LevelModel layout, List<RoomModel> rooms)
    {
        if (Mode == RenderMode.Real)
        {
            _specialRenderer.RenderSpecialObjects(layout, rooms, SpecialObjectsParent);
        }
    }
    #endregion

    #region Utility Methods
    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;

        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
        }

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

    private void CreateParentContainers()
    {
        FloorsParent = CreateParentIfNull(FloorsParent, "Floors");
        WallsParent = CreateParentIfNull(WallsParent, "Walls");
        DoorsParent = CreateParentIfNull(DoorsParent, "Doors");
        SpecialObjectsParent = CreateParentIfNull(SpecialObjectsParent, "SpecialObjects");
        EnvironmentParent = CreateParentIfNull(EnvironmentParent, "Environment");
    }

    private Transform CreateParentIfNull(Transform parent, string name)
    {
        return parent ?? CreateParent(name);
    }

    private Transform CreateParent(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        return go.transform;
    }

    private void ClearSpawnedContainers()
    {
        foreach (var container in _spawnedContainers)
        {
            if (container != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(container);
                #else
                Destroy(container);
                #endif
            }
        }
        _spawnedContainers.Clear();
    }

    private void ClearAllChildObjects()
    {
        ClearChildObjects(FloorsParent);
        ClearChildObjects(WallsParent);
        ClearChildObjects(DoorsParent);
        ClearChildObjects(SpecialObjectsParent);
        ClearChildObjects(EnvironmentParent);
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

    private void CleanupMaterials()
    {
        _materialManager?.CleanupMaterialCache();
    }

    private void LogFloorRenderingResults()
    {
        int renderedFloors = FloorsParent?.childCount ?? 0;
        Debug.Log($"Floor rendering complete: {renderedFloors} floor objects in scene");
    }

    private void LogRenderingResults()
    {
        Debug.Log($"Rendering complete - Floors: {FloorsParent.childCount}, Walls: {WallsParent.childCount}, Doors: {DoorsParent.childCount}");
    }
    #endregion
}