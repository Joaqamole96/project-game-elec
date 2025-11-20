/// <summary>
/// Accessibility state of a room.
/// </summary>
public enum RoomAccess
{
    Open,
    Closed,
    Locked
}

/// <summary>
/// Purpose and functionality of a room.
/// </summary>
public enum RoomType
{
    // Critical path
    Entrance,
    Exit,

    // Standard rooms
    Empty,
    Combat,
    Shop,
    Treasure,

    // Special rooms
    Boss,
    Survival,
    Puzzle,
    Secret
}