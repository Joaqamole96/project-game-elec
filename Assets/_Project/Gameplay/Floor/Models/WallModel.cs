using UnityEngine;

/// <summary>Represents wall segments that need gameplay interaction - use sparingly</summary>
public class WallModel
{
    public Vector2Int Position;
    public WallType Type;
    public bool IsDestructible;
    
    // ONLY store these if needed for gameplay
    public int Health;
    public int MaxHealth;

    public WallModel(Vector2Int position, WallType type, bool isDestructible = false)
    {
        Position = position;
        Type = type;
        IsDestructible = isDestructible;
        
        if (isDestructible)
        {
            MaxHealth = 100;
            Health = MaxHealth;
        }
    }
    
    // Only include if you need destructive walls
    public void Damage(int damage)
    {
        if (!IsDestructible) return;
        Health = Mathf.Max(0, Health - damage);
    }
    
    public bool IsDestroyed => IsDestructible && Health <= 0;
}

public enum WallType
{
    North, South, East, West,
    NorthEastCorner, NorthWestCorner, SouthEastCorner, SouthWestCorner,
    Interior, Doorway, Corridor
}