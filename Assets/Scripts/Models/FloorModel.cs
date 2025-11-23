// -------------------------------------------------- //
// Scripts/Models/FloorModel.cs
// -------------------------------------------------- //

using UnityEngine;

public class FloorModel
{
    public Vector2Int Position;
    public RoomType RoomType;
    // public MaterialType Material;

    public FloorModel(Vector2Int position, RoomType roomType = RoomType.Combat)
    {
        Position = position;
        RoomType = roomType;
        // Material = MaterialType.Default;
    }
}

// public enum MaterialType
// {
//     Default,
//     Cracked,
//     Mossy,
//     Bloody,
//     Ice,
//     Impassable
// }