// ==================================================
// Room Config
// -----------
// A configuration of a Room.
// ==================================================

using System.Collections.Generic;

namespace FGA.Configs
{
    public static class RoomConfig
    {
        public enum RoomType
        {
            // Endpoints
            Entrance, Exit,
            // Throughpoints
            Boss, Survival, Pursuit, Puzzle,
            // Standard
            Combat, Treasure, Shop,
            // Base
            Empty
        }

        public enum RoomFace
        {
            North,
            South,
            West,
            East
        }

        public static int MIN_ROOM_SIZE = 20;
    }
}