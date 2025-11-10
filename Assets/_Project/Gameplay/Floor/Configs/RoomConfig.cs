using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomSizePreset
{
    public string Name;
    public Vector2Int Size;
    public float Weight = 1.0f;
    public RoomType[] AllowedTypes;
    public bool IsFixedSize = true;
    public Vector2Int MinSize;
    public Vector2Int MaxSize;
    
    public RoomSizePreset() { }
    
    public RoomSizePreset(string name, Vector2Int size, float weight, RoomType[] allowedTypes, bool isFixedSize)
    {
        Name = name;
        Size = size;
        Weight = weight;
        AllowedTypes = allowedTypes;
        IsFixedSize = isFixedSize;
    }
}

[System.Serializable]
public class RoomSizeConfig
{
    [Header("Fixed Size Rooms")]
    public RoomSizePreset EntranceSize = new RoomSizePreset("Entrance", new Vector2Int(10, 10), 1f, new[] { RoomType.Entrance }, true);
    public RoomSizePreset ExitSize = new RoomSizePreset("Exit", new Vector2Int(10, 10), 1f, new[] { RoomType.Exit }, true);
    public RoomSizePreset ShopSize = new RoomSizePreset("Shop", new Vector2Int(15, 15), 1f, new[] { RoomType.Shop }, true);
    public RoomSizePreset TreasureSize = new RoomSizePreset("Treasure", new Vector2Int(15, 15), 1f, new[] { RoomType.Treasure }, true);
    public RoomSizePreset BossSize = new RoomSizePreset("Boss", new Vector2Int(30, 30), 1f, new[] { RoomType.Boss }, true);

    [Header("Variable Size Rooms")]
    public RoomSizePreset CombatSize = new RoomSizePreset("Combat", Vector2Int.zero, 1f, new[] { RoomType.Combat }, false) 
    { 
        MinSize = new Vector2Int(15, 15), 
        MaxSize = new Vector2Int(25, 25) 
    };
    
    public RoomSizePreset EmptySize = new RoomSizePreset("Empty", Vector2Int.zero, 1f, new[] { RoomType.Empty }, false) 
    { 
        MinSize = new Vector2Int(15, 15), 
        MaxSize = new Vector2Int(25, 25) 
    };

    public RoomSizePreset GetSizeForRoomType(RoomType roomType, System.Random random)
    {
        switch (roomType)
        {
            case RoomType.Entrance: return EntranceSize;
            case RoomType.Exit: return ExitSize;
            case RoomType.Shop: return ShopSize;
            case RoomType.Treasure: return TreasureSize;
            case RoomType.Boss: return BossSize;
            case RoomType.Combat: return GenerateRandomSize(CombatSize, random);
            case RoomType.Empty: return GenerateRandomSize(EmptySize, random);
            default: return GenerateRandomSize(CombatSize, random);
        }
    }

    private RoomSizePreset GenerateRandomSize(RoomSizePreset preset, System.Random random)
    {
        if (preset.IsFixedSize) return preset;
        
        int width = random.Next(preset.MinSize.x, preset.MaxSize.x + 1);
        int height = random.Next(preset.MinSize.y, preset.MaxSize.y + 1);
        
        return new RoomSizePreset(preset.Name, new Vector2Int(width, height), preset.Weight, preset.AllowedTypes, true);
    }
}

[System.Serializable]
public class RoomConfig
{
    [Header("Room Size System")]
    public RoomSizeConfig SizePresets = new RoomSizeConfig();

    [Header("Room Placement")]
    [Range(1, 10)]
    [Tooltip("Minimum inset from partition bounds for room creation")]
    public int MinInset = 4;

    [Range(5, 15)]
    [Tooltip("Maximum inset from partition bounds for room creation")]
    public int MaxInset = 8;

    [Header("Room Count")]
    [Range(5, 50)]
    [Tooltip("Maximum number of rooms to generate per floor")]
    public int MaxRooms = 20;

    [Header("Room Padding")]
    [Range(1, 5)]
    [Tooltip("Padding around room edges for spawn positions and gameplay")]
    public int SpawnPadding = 2;

    // REMOVED: All old size-related fields (MinRoomSize, MaxRoomSize, etc.)

    public RoomConfig Clone()
    {
        return new RoomConfig
        {
            MinInset = MinInset,
            MaxInset = MaxInset,
            MaxRooms = MaxRooms,
            SpawnPadding = SpawnPadding,
            SizePresets = SizePresets
        };
    }

    public void Validate()
    {
        MinInset = Mathf.Clamp(MinInset, 1, 10);
        MaxInset = Mathf.Clamp(MaxInset, 5, 15);
        MaxRooms = Mathf.Clamp(MaxRooms, 5, 50);
        SpawnPadding = Mathf.Clamp(SpawnPadding, 1, 5);

        if (MinInset >= MaxInset)
        {
            MinInset = MaxInset - 2;
        }
    }

    // KEEP only these utility methods:
    public int GetRandomInset(System.Random random)
    {
        return random.Next(MinInset, MaxInset + 1);
    }

    // REMOVED: CalculateRoomSize, IsValidRoomSize, CanPartitionContainRoom
    // These are now handled by the new size system
}