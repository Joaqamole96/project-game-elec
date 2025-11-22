// FloorModel.cs
using UnityEngine;

/// <summary>
/// Represents an individual walkable floor tile with gameplay properties.
/// Created sparingly for tiles that need special gameplay functionality.
/// </summary>
public class FloorModel
{
    /// <summary>Grid position of the floor tile.</summary>
    public Vector2Int Position { get; private set; }
    
    /// <summary>Type of room this floor belongs to.</summary>
    public RoomType RoomType { get; private set; }
    
    /// <summary>Material/visual type of the floor.</summary>
    public MaterialType Material { get; private set; }
    
    /// <summary>World position for gameplay objects.</summary>
    public Vector3 WorldPosition => new(Position.x + 0.5f, 0f, Position.y + 0.5f);
    
    /// <summary>Whether this floor tile has any special gameplay properties.</summary>
    public bool HasSpecialProperties => Material != MaterialType.Default;

    public FloorModel(Vector2Int position, RoomType roomType = RoomType.Combat)
    {
        Position = position;
        RoomType = roomType;
        Material = MaterialType.Default;
    }

    /// <summary>
    /// Sets the material type for this floor tile.
    /// </summary>
    public void SetMaterial(MaterialType material)
    {
        Material = material;
    }

    /// <summary>
    /// Checks if this floor tile is traversable by the player.
    /// </summary>
    public bool IsTraversable => Material != MaterialType.Impassable;

    /// <summary>
    /// Gets the movement cost for pathfinding over this tile.
    /// </summary>
    public float GetMovementCost()
    {
        return Material switch
        {
            MaterialType.Default => 1.0f,
            MaterialType.Cracked => 1.2f,
            MaterialType.Mossy => 1.5f,
            MaterialType.Bloody => 1.3f,
            MaterialType.Ice => 0.8f,
            _ => 1.0f
        };
    }
}

/// <summary>
/// Types of floor materials with different gameplay properties.
/// </summary>
public enum MaterialType
{
    Default,
    Cracked,
    Mossy,
    Bloody,
    Ice,
    Impassable
}