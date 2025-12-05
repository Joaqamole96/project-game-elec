// -------------------------------------------------- //
// Scripts/Models/DoorModel.cs
// -------------------------------------------------- //

using UnityEngine;

public class DoorModel
{
    public Vector2Int Position;
    public DoorState State;

    public DoorModel(Vector2Int position)
    {
        Position = position;
        State = DoorState.Closed;
    }
}
