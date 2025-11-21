using UnityEngine;

public readonly struct FloorModel
{
    public readonly Vector2Int Position;
    public readonly FloorType Type;

    public FloorModel(Vector2Int position, FloorType type)
    {
        Position = position;
        Type = type;
    }
}