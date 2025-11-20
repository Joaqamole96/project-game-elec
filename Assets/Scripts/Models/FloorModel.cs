// FloorModel.cs
using UnityEngine;

public class FloorModel
{
    public Vector2Int Position;
    public FloorType Type;

    public FloorModel(Vector2Int position, FloorType type = FloorType.Normal)
    {
        Position = position;
        Type = type;
    }
}