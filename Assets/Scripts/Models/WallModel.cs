// WallModel.cs
using UnityEngine;

/// <summary>
/// Represents wall segments that need gameplay interaction.
/// Created sparingly for destructible or interactive walls only.
/// </summary>
public class WallModel
{
    /// <summary>Grid position of the wall.</summary>
    public Vector2Int Position { get; private set; }
    
    /// <summary>Type of wall for rendering and behavior.</summary>
    public WallType Type { get; private set; }
    
    /// <summary>Whether this wall can be destroyed.</summary>
    public bool IsDestructible { get; private set; }
    
    /// <summary>Current health of the wall (if destructible).</summary>
    public int Health { get; private set; }
    
    /// <summary>Maximum health of the wall (if destructible).</summary>
    public int MaxHealth { get; private set; }
    
    /// <summary>World position for gameplay.</summary>
    public Vector3 WorldPosition => new(Position.x + 0.5f, 0.5f, Position.y + 0.5f);
    
    /// <summary>Whether the wall has been destroyed.</summary>
    public bool IsDestroyed => IsDestructible && Health <= 0;

    public WallModel(Vector2Int position, WallType type, bool isDestructible = false)
    {
        Position = position;
        Type = type;
        IsDestructible = isDestructible;
        
        if (isDestructible)
        {
            MaxHealth = CalculateMaxHealth(type);
            Health = MaxHealth;
        }
    }

    /// <summary>
    /// Applies damage to the wall if it's destructible.
    /// </summary>
    public void Damage(int damage)
    {
        if (!IsDestructible || IsDestroyed) return;
        
        Health = Mathf.Max(0, Health - damage);
    }

    /// <summary>
    /// Repairs the wall by the specified amount.
    /// </summary>
    public void Repair(int amount)
    {
        if (!IsDestructible) return;
        
        Health = Mathf.Min(MaxHealth, Health + amount);
    }

    /// <summary>
    /// Completely repairs the wall.
    /// </summary>
    public void FullRepair()
    {
        if (!IsDestructible) return;
        
        Health = MaxHealth;
    }

    /// <summary>
    /// Makes the wall indestructible.
    /// </summary>
    public void MakeIndestructible()
    {
        IsDestructible = false;
        Health = 0;
        MaxHealth = 0;
    }

    /// <summary>
    /// Gets the health percentage (0-1) for UI displays.
    /// </summary>
    public float GetHealthPercentage()
    {
        if (!IsDestructible) return 1f;
        return (float)Health / MaxHealth;
    }

    private int CalculateMaxHealth(WallType wallType)
    {
        return wallType switch
        {
            WallType.North or WallType.South or WallType.East or WallType.West => 100,
            WallType.Corridor => 50,
            WallType.Interior => 75,
            _ => 100
        };
    }
}

/// <summary>
/// Types of walls with different positions and properties.
/// </summary>
public enum WallType
{
    // Cardinal directions
    North,
    South,
    East,
    West,
    
    // Corners
    NorthEastCorner,
    NorthWestCorner,
    SouthEastCorner,
    SouthWestCorner,
    
    // Special types
    Interior,
    Doorway,
    Corridor,
    Secret
}