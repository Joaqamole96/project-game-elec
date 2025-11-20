// DoorModel.cs
using UnityEngine;

public class DoorModel
{
    public Vector2Int Position;
    public DoorState State;
    public DoorKey Key;

    public DoorModel(Vector2Int position, DoorState state = DoorState.Closed, DoorKey key = DoorKey.None)
    {
        Position = position;
        State = state;
        Key = key;
    }
}