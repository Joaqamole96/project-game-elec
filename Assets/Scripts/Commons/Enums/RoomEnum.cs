// -------------------- //
// Script/Commons/Enums/RoomEnum.cs
// -------------------- //

public enum RoomAccess { Open, Closed, Locked }

public enum RoomType
{
    // Critical path
    Entrance, Exit,
    // Standard rooms
    Empty, Combat, Shop, Treasure,
    // Special rooms
    Boss, Survival, Puzzle, Secret
}