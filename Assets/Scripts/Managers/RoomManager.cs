// -------------------------------------------------- //
// Scripts/Manaers/RoomManager.cs
// -------------------------------------------------- //

using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    public RoomModel CurrentRoom { get; private set; }
    private LevelModel currentLevel;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    
    public void UpdatePlayerRoom(Vector3 playerPosition)
    {
        if (currentLevel == null) return;
        
        Vector2Int gridPos = new(
            Mathf.FloorToInt(playerPosition.x),
            Mathf.FloorToInt(playerPosition.z)
        );
        
        RoomModel newRoom = currentLevel.GetRoomAtPosition(gridPos);
        
        if (newRoom != null && newRoom != CurrentRoom)
        {
            CurrentRoom = newRoom;
            OnRoomEntered(CurrentRoom);
        }
    }
    
    private void OnRoomEntered(RoomModel room)
    {
        Debug.Log($"Entered {room.Type} room (ID: {room.ID})");
        
        if (room.Type == RoomType.Combat && !room.IsCleared) StartCombatInRoom();
    }
    
    private void StartCombatInRoom()
    {
        Debug.Log("Combat started!");
        // Future: Close doors, spawn enemies if not already present
    }
}