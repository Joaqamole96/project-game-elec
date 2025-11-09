// IRendererInterfaces.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface for floor rendering implementations.
/// </summary>
public interface IFloorRenderer
{
    /// <summary>Renders floors as combined meshes grouped by room type.</summary>
    List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent);
    
    /// <summary>Renders floors as individual objects.</summary>
    void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision);
}

/// <summary>
/// Interface for wall rendering implementations.
/// </summary>
public interface IWallRenderer
{
    /// <summary>Renders walls as combined meshes grouped by wall type.</summary>
    List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent);
    
    /// <summary>Renders walls as individual objects.</summary>
    void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision);
}

/// <summary>
/// Interface for door rendering implementations.
/// </summary>
public interface IDoorRenderer
{
    /// <summary>Renders all doors in the layout.</summary>
    void RenderDoors(LevelModel layout, Transform parent, bool enableCollision);
}