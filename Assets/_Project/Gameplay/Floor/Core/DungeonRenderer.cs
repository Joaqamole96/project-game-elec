using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    
    [Header("Parent Transforms")]
    public Transform FloorsParent;
    public Transform WallsParent;
    public Transform DoorsParent;
    public Transform SpecialObjectsParent;
    
    [Header("Mobile Optimization")]
    public bool CombineMeshes = true;
    public bool EnableFloorCollision = true;
    public bool EnableWallCollision = true;
    public bool EnableDoorCollision = false;
    
    [Header("Material Settings - Gizmo Mode")]
    public Material DefaultFloorMaterial;
    public Material DefaultWallMaterial;
    public Material DefaultDoorMaterial;
    
    // FIX: Use interfaces instead of concrete types
    private IFloorRenderer _floorRenderer;
    private IWallRenderer _wallRenderer;
    private IDoorRenderer _doorRenderer;
    private SpecialRoomRenderer _specialRenderer;
    private MaterialManager _materialManager;
    
    // Combined mesh containers
    private List<GameObject> _spawnedContainers = new();

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
        
        // FIX: Initialize appropriate renderer based on mode using interfaces
        if (Mode == RenderMode.Gizmo)
        {
            _floorRenderer = new GizmoFloorRenderer(_materialManager);
            _wallRenderer = new GizmoWallRenderer(_materialManager);
            _doorRenderer = new GizmoDoorRenderer(_materialManager);
        }
        else
        {
            _floorRenderer = new PrefabFloorRenderer(FloorPrefab, _materialManager);
            _wallRenderer = new PrefabWallRenderer(WallPrefab, _materialManager);
            _doorRenderer = new PrefabDoorRenderer(DoorPrefab, _materialManager);
        }
        
        _specialRenderer = new SpecialRoomRenderer(EntrancePrefab, ExitPrefab);
    }

    private void EnsureComponentsInitialized()
    {
        if (_materialManager == null)
        {
            InitializeComponents();
        }
    }

    public void RenderDungeon(LevelModel layout, List<RoomModel> rooms)
    {
        EnsureComponentsInitialized();
        ClearRendering();
        CreateParentContainers();
        
        if (Mode == RenderMode.Gizmo)
        {
            _materialManager.InitializeMaterialCache();
        }
        
        // Render floors, walls, and doors
        RenderFloors(layout, rooms);
        RenderWalls(layout);
        RenderDoors(layout);
        
        // Render special room objects (entrance/exit)
        RenderSpecialObjects(layout, rooms);
        
        Debug.Log($"Rendered dungeon in {Mode} mode with {_spawnedContainers.Count} combined mesh objects");
    }

    public void ClearRendering()
    {
        EnsureComponentsInitialized();
        
        foreach (var container in _spawnedContainers)
        {
            if (container != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(container);
                else
                    Destroy(container);
                #else
                Destroy(container);
                #endif
            }
        }
        _spawnedContainers.Clear();

        ClearAllChildObjects(FloorsParent);
        ClearAllChildObjects(WallsParent);
        ClearAllChildObjects(DoorsParent);
        ClearAllChildObjects(SpecialObjectsParent);

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
        
        if (CombineMeshes && Mode == RenderMode.Gizmo) // Only combine in Gizmo mode
        {
            Debug.Log("Using combined mesh rendering for floors");
            var floorMeshes = _floorRenderer.RenderCombinedFloorsByRoomType(layout, rooms, FloorsParent);
            _spawnedContainers.AddRange(floorMeshes);
            
            Debug.Log($"Combined mesh rendering complete: {floorMeshes.Count} mesh objects created");
            
            if (EnableFloorCollision)
            {
                foreach (var mesh in floorMeshes)
                    AddCollisionToObject(mesh, "Floor");
            }
        }
        else
        {
            Debug.Log("Using individual floor rendering");
            _floorRenderer.RenderIndividualFloors(layout, rooms, FloorsParent, EnableFloorCollision);
        }
        
        // Verify floors were actually created
        int renderedFloors = FloorsParent?.childCount ?? 0;
        Debug.Log($"Floor rendering complete: {renderedFloors} floor objects in scene");
    }

    private void RenderWalls(LevelModel layout)
    {
        if (layout?.AllWallTiles == null || layout.WallTypes == null) return;
        
        if (CombineMeshes && Mode == RenderMode.Gizmo) // Only combine in Gizmo mode
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
        if (Mode == RenderMode.Real) // Only render special objects in Real mode
        {
            _specialRenderer.RenderSpecialObjects(layout, rooms, SpecialObjectsParent);
        }
    }
    #endregion

    #region Utility Methods
    private void AddCollisionToObject(GameObject obj, string objectType)
    {
        if (obj == null) return;

        var existingCollider = obj.GetComponent<Collider>();
        if (existingCollider == null)
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
        if (FloorsParent == null) FloorsParent = CreateParent("Floors");
        if (WallsParent == null) WallsParent = CreateParent("Walls");
        if (DoorsParent == null) DoorsParent = CreateParent("Doors");
        if (SpecialObjectsParent == null) SpecialObjectsParent = CreateParent("SpecialObjects");
    }

    private Transform CreateParent(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        return go.transform;
    }

    private void ClearAllChildObjects(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
    }

    private void CleanupMaterials()
    {
        // Simple cleanup - just clear the material cache
        // The actual Material objects will be garbage collected
        _materialManager?.CleanupMaterialCache();
    }
    #endregion
}