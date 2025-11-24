// -------------------------------------------------- //
// Scripts/Managers/CombatManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages combat state for rooms - tracks enemies, locks doors, handles rewards.
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("Current Room State")]
    public RoomModel currentRoom;
    public bool isInCombat = false;
    public int enemiesRemaining = 0;
    
    [Header("Door References")]
    private List<DoorController> roomDoors = new();
    
    [Header("Rewards")]
    public int goldRewardMin = 10;
    public int goldRewardMax = 50;
    public float healthDropChance = 0.3f;
    
    private LevelModel levelLayout;
    
    public static CombatManager Instance { get; private set; }
    
    // ------------------------- //
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Get level layout reference
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null)
        {
            levelLayout = layoutManager.CurrentLayout;
        }
    }
    
    // ------------------------- //
    // ROOM ENTRY/EXIT
    // ------------------------- //
    
    public void OnPlayerEnterRoom(RoomModel room)
    {
        if (room == null || room == currentRoom) return;
        
        currentRoom = room;
        
        Debug.Log($"CombatManager: Player entered {room.Type} room (ID: {room.ID})");
        
        // Handle combat rooms
        if ((room.Type == RoomType.Combat || room.Type == RoomType.Boss) && !room.IsCleared)
        {
            StartCombatInRoom(room);
        }
    }
    
    private void StartCombatInRoom(RoomModel room)
    {
        Debug.Log($"CombatManager: Starting combat in room {room.ID}");
        
        isInCombat = true;
        
        // Count enemies in room
        enemiesRemaining = CountEnemiesInRoom(room);
        
        if (enemiesRemaining > 0)
        {
            // Lock all doors leading to this room
            LockRoomDoors(room);
            
            Debug.Log($"CombatManager: {enemiesRemaining} enemies in room, doors locked");
        }
        else
        {
            // No enemies, mark as cleared
            OnRoomCleared(room);
        }
    }
    
    // ------------------------- //
    // ENEMY TRACKING
    // ------------------------- //
    
    private int CountEnemiesInRoom(RoomModel room)
    {
        EntityManager entityManager = GameDirector.Instance?.entityManager;
        if (entityManager == null) return 0;
        
        int count = 0;
        Vector2Int center = room.Center;
        
        foreach (GameObject enemy in entityManager.allEnemies)
        {
            if (enemy == null) continue;
            
            Vector2Int enemyGridPos = new(
                Mathf.FloorToInt(enemy.transform.position.x),
                Mathf.FloorToInt(enemy.transform.position.z)
            );
            
            if (room.ContainsPosition(enemyGridPos))
            {
                count++;
            }
        }
        
        return count;
    }
    
    public void OnEnemyDied(GameObject enemy)
    {
        if (!isInCombat || currentRoom == null) return;
        
        // Check if enemy was in current room
        Vector2Int enemyGridPos = new(
            Mathf.FloorToInt(enemy.transform.position.x),
            Mathf.FloorToInt(enemy.transform.position.z)
        );
        
        if (currentRoom.ContainsPosition(enemyGridPos))
        {
            enemiesRemaining--;
            Debug.Log($"CombatManager: Enemy died, {enemiesRemaining} remaining");
            
            if (enemiesRemaining <= 0)
            {
                OnRoomCleared(currentRoom);
            }
        }
    }
    
    // ------------------------- //
    // ROOM CLEARING
    // ------------------------- //
    
    private void OnRoomCleared(RoomModel room)
    {
        Debug.Log($"CombatManager: Room {room.ID} cleared!");
        
        room.MarkAsCleared();
        isInCombat = false;
        
        // Unlock doors
        UnlockRoomDoors(room);
        
        // Spawn rewards
        SpawnRoomRewards(room);
    }
    
    private void SpawnRoomRewards(RoomModel room)
    {
        // Spawn gold
        int goldAmount = Random.Range(goldRewardMin, goldRewardMax + 1);
        Vector2Int centerTile = room.Center;
        Vector3 spawnPos = new(centerTile.x + 0.5f, 1f, centerTile.y + 0.5f);
        
        // TODO: Spawn actual gold prefab
        Debug.Log($"CombatManager: Spawned {goldAmount} gold at {spawnPos}");
        
        // Chance to spawn health potion
        if (Random.value < healthDropChance)
        {
            GameObject healthPotionPrefab = ResourceService.LoadHealthPotionPrefab();
            if (healthPotionPrefab != null)
            {
                Vector3 offset = new(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                Instantiate(healthPotionPrefab, spawnPos + offset, Quaternion.identity);
                Debug.Log("CombatManager: Spawned health potion");
            }
        }
    }
    
    // ------------------------- //
    // DOOR MANAGEMENT
    // ------------------------- //
    
    private void LockRoomDoors(RoomModel room)
    {
        roomDoors.Clear();
        
        if (levelLayout == null || levelLayout.AllDoorTiles == null) return;
        
        // Find all door tiles adjacent to this room
        foreach (Vector2Int doorPos in levelLayout.AllDoorTiles)
        {
            if (IsDoorAdjacentToRoom(doorPos, room))
            {
                DoorController door = FindDoorAtPosition(doorPos);
                if (door != null)
                {
                    door.LockDoor();
                    roomDoors.Add(door);
                }
            }
        }
        
        Debug.Log($"CombatManager: Locked {roomDoors.Count} doors");
    }
    
    private void UnlockRoomDoors(RoomModel room)
    {
        foreach (DoorController door in roomDoors)
        {
            if (door != null)
            {
                door.UnlockDoor();
            }
        }
        
        roomDoors.Clear();
        Debug.Log("CombatManager: Unlocked all doors");
    }
    
    private bool IsDoorAdjacentToRoom(Vector2Int doorPos, RoomModel room)
    {
        // Check 4 cardinal directions from door
        Vector2Int[] neighbors = new Vector2Int[]
        {
            doorPos + Vector2Int.up,
            doorPos + Vector2Int.down,
            doorPos + Vector2Int.left,
            doorPos + Vector2Int.right
        };
        
        foreach (Vector2Int neighbor in neighbors)
        {
            if (room.ContainsPosition(neighbor))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private DoorController FindDoorAtPosition(Vector2Int position)
    {
        DoorController[] allDoors = FindObjectsOfType<DoorController>();
        
        foreach (DoorController door in allDoors)
        {
            Vector2Int doorGridPos = new(
                Mathf.FloorToInt(door.transform.position.x),
                Mathf.FloorToInt(door.transform.position.z)
            );
            
            if (doorGridPos == position)
            {
                return door;
            }
        }
        
        return null;
    }
    
    // ------------------------- //
    // PUBLIC API
    // ------------------------- //
    
    public bool IsRoomCleared(RoomModel room)
    {
        return room != null && room.IsCleared;
    }
    
    public void ForceCompleteRoom()
    {
        if (currentRoom != null && isInCombat)
        {
            OnRoomCleared(currentRoom);
        }
    }
}