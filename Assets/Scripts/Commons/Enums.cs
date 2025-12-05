// -------------------------------------------------- //
// Scripts/Commons/Enums.cs
// -------------------------------------------------- //

public enum DoorState { Open, Closed, Locked, Broken }

public enum ItemType 
{ 
    Consumable, 
    Equipment 
}

public enum PowerType
{
    // Movement
    SpeedBoost,      // +20% movement speed
    Dash,            // Quick dash ability
    // Combat
    AttackSpeed,     // +30% attack speed
    Damage,          // +25% damage
    CriticalHit,     // 15% chance for 2x damage
    // Defense
    MaxHealth,       // +20 max HP
    HealthRegen,     // Regenerate 1 HP per 5 seconds
    DamageReduction, // Take 15% less damage
    // Utility
    Vampire,         // Heal 10% of damage dealt
    ExtraGold,       // +50% gold from enemies
    LuckyFind        // Higher chance for rare drops
}

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