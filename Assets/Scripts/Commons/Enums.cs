// -------------------------------------------------- //
// Scripts/Commons/Enums.cs
// -------------------------------------------------- //

public enum RoomAccess 
{ 
    Open, 
    Closed, 
    Locked 
}

public enum RoomType 
{
    // Critical path
    Entrance, Exit,

    // Standard rooms
    Empty, Combat, Shop, Treasure,

    // Special rooms
    Boss, Survival, Puzzle, Secret
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

public enum WeaponType
{
    Melee,
    Charge,
    Ranged,
    Magic
}