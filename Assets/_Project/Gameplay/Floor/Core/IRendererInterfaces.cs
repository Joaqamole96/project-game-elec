// Core/IRendererInterfaces.cs
using UnityEngine;
using System.Collections.Generic;

public interface IFloorRenderer
{
    List<GameObject> RenderCombinedFloorsByRoomType(LevelModel layout, List<RoomModel> rooms, Transform parent);
    void RenderIndividualFloors(LevelModel layout, List<RoomModel> rooms, Transform parent, bool enableCollision);
}

public interface IWallRenderer
{
    List<GameObject> RenderCombinedWallsByType(LevelModel layout, Transform parent);
    void RenderIndividualWalls(LevelModel layout, Transform parent, bool enableCollision);
}

public interface IDoorRenderer
{
    void RenderDoors(LevelModel layout, Transform parent, bool enableCollision);
}