using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonRenderer : MonoBehaviour
{
    [Header("Prefabs (Optional - will use cubes if empty)")]
    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject DoorPrefab;
    
    [Header("Parent Transforms (Optional)")]
    public Transform FloorsParent;
    public Transform WallsParent;
    public Transform DoorsParent;
    
    // Runtime collections
    private List<GameObject> _spawnedFloors = new List<GameObject>();
    private List<GameObject> _spawnedWalls = new List<GameObject>();
    private List<GameObject> _spawnedDoors = new List<GameObject>();
    
    public void RenderDungeon(DungeonLayout layout, List<RoomAssignment> roomAssignments)
    {
        ClearRendering();
        CreateParentContainers();
        
        RenderFloors(layout);
        RenderWalls(layout);
        RenderDoors(layout);
        ApplyRoomVisuals(roomAssignments);
    }
    
    public void ClearRendering()
    {
        foreach (var obj in _spawnedFloors) DestroyImmediate(obj);
        foreach (var obj in _spawnedWalls) DestroyImmediate(obj);
        foreach (var obj in _spawnedDoors) DestroyImmediate(obj);
        
        _spawnedFloors.Clear();
        _spawnedWalls.Clear();
        _spawnedDoors.Clear();
    }

    private void RenderFloors(DungeonLayout layout)
    {
        foreach (var floorPos in layout.AllFloorTiles)
        {
            CreateFloorAtPosition(floorPos);
        }
    }

    private void RenderWalls(DungeonLayout layout)
    {
        foreach (var wallPos in layout.AllWallTiles)
        {
            CreateWallAtPosition(wallPos);
        }
    }

    private void RenderDoors(DungeonLayout layout)
    {
        foreach (var doorPos in layout.AllDoorTiles)
        {
            CreateDoorAtPosition(doorPos);
        }
    }

    private void ApplyRoomVisuals(List<RoomAssignment> roomAssignments)
    {
        foreach (var assignment in roomAssignments)
        {
            ApplyRoomVisual(assignment);
        }
    }

    private void ApplyRoomVisual(RoomAssignment assignment)
    {
        Color roomColor = GetColorForRoomType(assignment.Type);
        
        foreach (var floorPos in assignment.Room.GetFloorTiles())
        {
            var floorObj = _spawnedFloors.Find(f => 
                Vector3.Distance(f.transform.position, new Vector3(floorPos.x + 0.5f, 0, floorPos.y + 0.5f)) < 0.1f);
            
            if (floorObj != null)
            {
                var renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = roomColor;
                }
            }
        }
    }

    private Color GetColorForRoomType(RoomType type)
    {
        return type switch
        {
            RoomType.Entrance => Color.green,
            RoomType.Exit => Color.red,
            RoomType.Empty => Color.gray,
            RoomType.Combat => Color.white,
            RoomType.Shop => Color.blue,
            RoomType.Treasure => Color.yellow,
            RoomType.Boss => Color.magenta,
            _ => Color.white
        };
    }

    private void CreateFloorAtPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0, gridPos.y + 0.5f);
        GameObject floor;
        
        if (FloorPrefab != null)
        {
            floor = Instantiate(FloorPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = worldPos;
            floor.transform.localScale = new Vector3(1f, 0.1f, 1f);
            floor.name = $"Floor_{gridPos.x}_{gridPos.y}";
            var renderer = floor.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = Color.gray;
        }
        
        floor.transform.SetParent(FloorsParent);
        _spawnedFloors.Add(floor);
    }

    private void CreateWallAtPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0.5f, gridPos.y + 0.5f);
        GameObject wall;
        
        if (WallPrefab != null)
        {
            wall = Instantiate(WallPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = worldPos;
            wall.transform.localScale = new Vector3(1f, 1f, 1f);
            wall.name = $"Wall_{gridPos.x}_{gridPos.y}";
            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = Color.red;
        }
        
        wall.transform.SetParent(WallsParent);
        _spawnedWalls.Add(wall);
    }

    private void CreateDoorAtPosition(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 0.4f, gridPos.y + 0.5f);
        GameObject door;
        
        if (DoorPrefab != null)
        {
            door = Instantiate(DoorPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.transform.position = worldPos;
            door.transform.localScale = new Vector3(1f, 0.8f, 1f);
            door.name = $"Door_{gridPos.x}_{gridPos.y}";
            var renderer = door.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = Color.yellow;
        }
        
        door.transform.SetParent(DoorsParent);
        _spawnedDoors.Add(door);
    }

    private void CreateParentContainers()
    {
        if (FloorsParent == null)
        {
            GameObject floorsGo = new GameObject("Floors");
            FloorsParent = floorsGo.transform;
            FloorsParent.SetParent(transform);
        }
        
        if (WallsParent == null)
        {
            GameObject wallsGo = new GameObject("Walls");
            WallsParent = wallsGo.transform;
            WallsParent.SetParent(transform);
        }
        
        if (DoorsParent == null)
        {
            GameObject doorsGo = new GameObject("Doors");
            DoorsParent = doorsGo.transform;
            DoorsParent.SetParent(transform);
        }
    }
}