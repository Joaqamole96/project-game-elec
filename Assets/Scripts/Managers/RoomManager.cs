// -------------------------------------------------- //
// Scripts/Managers/RoomManager.cs (ENHANCED)
// -------------------------------------------------- //

using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    public RoomModel CurrentRoom { get; private set; }
    private LevelModel currentLevel;
    private CombatManager combatManager;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        combatManager = FindObjectOfType<CombatManager>();
        if (combatManager == null)
        {
            combatManager = gameObject.AddComponent<CombatManager>();
        }
    }
    
    public void UpdatePlayerRoom(Vector3 playerPosition)
    {
        if (currentLevel == null)
        {
            LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
            if (layoutManager != null)
            {
                currentLevel = layoutManager.CurrentLayout;
            }
        }
        
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
        
        // Notify combat manager
        if (combatManager != null)
        {
            combatManager.OnPlayerEnterRoom(room);
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }
}