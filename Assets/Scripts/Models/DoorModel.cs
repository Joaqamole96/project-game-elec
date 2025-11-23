// -------------------------------------------------- //
// Scripts/Models/DoorModel.cs
// -------------------------------------------------- //

using UnityEngine;

public class DoorModel
{
    public Vector2Int Position;
    public DoorState State;
    public KeyType RequiredKey;

    public DoorModel(Vector2Int position)
    {
        Position = position;
        State = DoorState.Closed;
        RequiredKey = KeyType.None;
    }
}

public enum DoorState { Open, Closed, Locked, Broken }

public enum KeyType { None, Key, }