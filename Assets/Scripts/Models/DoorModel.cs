using UnityEngine;

public readonly struct DoorModel
{
    public readonly Vector2Int Position;
    public readonly DoorState State;
    public readonly DoorKey Key;

    public DoorModel(Vector2Int position, DoorState state = DoorState.Closed, DoorKey key = DoorKey.None)
    {
        Position = position;
        State = state;
        Key = key;
    }
}