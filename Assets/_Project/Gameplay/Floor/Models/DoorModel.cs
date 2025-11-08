using UnityEngine;

public class DoorModel
{
    public Vector2Int Position;
    public DoorType Type;
    public DoorState State;
    public KeyType RequiredKey;
    
    public DoorModel(Vector2Int position, DoorType type = DoorType.Wood)
    {
        Position = position;
        Type = type;
        State = DoorState.Closed;
        RequiredKey = KeyType.None;
    }
    
    public bool CanOpen(KeyType key = KeyType.None)
    {
        return State != DoorState.Locked || 
               (State == DoorState.Locked && key == RequiredKey);
    }
    
    public void Open()
    {
        if (State == DoorState.Locked) return;
        State = DoorState.Open;
    }
    
    public void Close() => State = DoorState.Closed;
    
    public void Lock(KeyType keyType)
    {
        State = DoorState.Locked;
        RequiredKey = keyType;
    }
    
    public void Unlock()
    {
        if (State == DoorState.Locked)
        {
            State = DoorState.Closed;
            RequiredKey = KeyType.None;
        }
    }
}

public enum DoorType
{
    Wood, Metal
}

public enum DoorState
{
    Open, Closed, Locked, Broken
}

public enum KeyType
{
    None, Key, MasterKey
}