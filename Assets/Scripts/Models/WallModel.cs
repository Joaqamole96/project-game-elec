// -------------------------------------------------- //
// Scripts/Models/WallModel.cs
// -------------------------------------------------- //

using UnityEngine;

public class WallModel
{
    public Vector2Int Position;
    public WallType Type;
    public bool IsDestructible;
    public int Health;
    public int MaxHealth;

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

public enum WallType
{
    // Cardinal directions
    North, South, East, West,
    
    // Corners
    NorthEastCorner, NorthWestCorner,
    SouthEastCorner, SouthWestCorner,
    
    // Special types
    Interior, Doorway,
    Corridor, Secret
}