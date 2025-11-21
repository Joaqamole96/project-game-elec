using UnityEngine;

public readonly struct WallModel
{
    public readonly Vector2Int Position;

    public WallModel(Vector2Int position)
    {
        Position = position;
    }
}