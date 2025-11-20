// WallModel.cs
using UnityEngine;

public class WallModel
{
    public Vector2Int Position { get; private set; }

    public WallModel(Vector2Int position)
    {
        Position = position;
    }
}