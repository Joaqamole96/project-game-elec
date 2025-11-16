using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    
    private RoomModel currentRoom;
    private LevelModel currentLevel;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void SetCurrentLevel(LevelModel level)
    {
        currentLevel = level;
    }
    
    public void UpdatePlayerRoom(Vector3 playerPosition)
    {
        if (currentLevel == null) return;
        
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(playerPosition.x),
            Mathf.FloorToInt(playerPosition.z)
        );
        
        RoomModel newRoom = currentLevel.GetRoomAtPosition(gridPos);
        
        if (newRoom != null && newRoom != currentRoom)
        {
            currentRoom = newRoom;
            Debug.Log($"Entered room: {currentRoom.Type} (ID: {currentRoom.ID})");
            
            // Here we can trigger room events (spawn enemies, etc.)
            OnRoomEntered(currentRoom);
        }
    }
    
    private void OnRoomEntered(RoomModel room)
    {
        // We'll expand this later for combat rooms
        if (room.Type == RoomType.Combat && !room.IsCleared)
        {
            Debug.Log("Combat room entered - enemies will spawn here!");
            // TODO: Spawn enemies
        }
    }
    
    public RoomModel GetCurrentRoom()
    {
        return currentRoom;
    }
}