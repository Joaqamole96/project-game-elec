/// <summary> The possible states for a door. </summary>
public enum DoorState
{
    Open,
    Closed,
    Locked,
    Broken
}

/// <summary> The key type that a door needs to be unlocked. </summary>
public enum DoorKey
{
    None,
    Key,
    MasterKey,
    BossKey
}