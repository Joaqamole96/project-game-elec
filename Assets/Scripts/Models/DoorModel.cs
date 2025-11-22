// DoorModel.cs
using UnityEngine;

/// <summary>
/// Represents a door between rooms or corridors with state and interaction properties.
/// </summary>
public class DoorModel
{
    /// <summary>Grid position of the door.</summary>
    public Vector2Int Position { get; private set; }
    
    /// <summary>Type of door (affects appearance and properties).</summary>
    public DoorType Type { get; private set; }
    
    /// <summary>Current state of the door.</summary>
    public DoorState State { get; private set; }
    
    /// <summary>Key required to open this door (if locked).</summary>
    public KeyType RequiredKey { get; private set; }
    
    /// <summary>World position of the door for gameplay.</summary>
    public Vector3 WorldPosition => new(Position.x + 0.5f, 0f, Position.y + 0.5f);

    public DoorModel(Vector2Int position, DoorType type = DoorType.Wood)
    {
        Position = position;
        Type = type;
        State = DoorState.Closed;
        RequiredKey = KeyType.None;
    }
    
    /// <summary>
    /// Checks if the door can be opened with the given key.
    /// </summary>
    public bool CanOpen(KeyType key = KeyType.None)
    {
        return State != DoorState.Locked || (State == DoorState.Locked && key == RequiredKey);
    }
    
    /// <summary>
    /// Attempts to open the door. Returns true if successful.
    /// </summary>
    public bool TryOpen(KeyType key = KeyType.None)
    {
        if (!CanOpen(key)) return false;
        
        State = DoorState.Open;
        return true;
    }
    
    /// <summary>
    /// Closes the door if it's not locked.
    /// </summary>
    public void Close()
    {
        if (State != DoorState.Locked)
            State = DoorState.Closed;
    }
    
    /// <summary>
    /// Locks the door with the specified key type.
    /// </summary>
    public void Lock(KeyType keyType)
    {
        State = DoorState.Locked;
        RequiredKey = keyType;
    }
    
    /// <summary>
    /// Unlocks the door if the correct key is provided.
    /// </summary>
    public bool TryUnlock(KeyType key)
    {
        if (State == DoorState.Locked && key == RequiredKey)
        {
            State = DoorState.Closed;
            RequiredKey = KeyType.None;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Forces the door open regardless of lock state (for debugging/cheats).
    /// </summary>
    public void ForceOpen()
    {
        State = DoorState.Open;
    }
    
    /// <summary>
    /// Checks if the door is currently blocking pathfinding.
    /// </summary>
    public bool IsBlocking => State == DoorState.Closed || State == DoorState.Locked;
}

/// <summary>
/// Types of doors with different properties and appearances.
/// </summary>
public enum DoorType
{
    Wood,
    Metal,
    Secret,
    Boss
}

/// <summary>
/// Possible states for a door.
/// </summary>
public enum DoorState
{
    Open,
    Closed,
    Locked,
    Broken
}

/// <summary>
/// Types of keys that can unlock doors.
/// </summary>
public enum KeyType
{
    None,
    Key,
    MasterKey,
    BossKey
}