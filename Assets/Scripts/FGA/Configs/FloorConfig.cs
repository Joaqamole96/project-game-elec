// ==================================================
// Floor Config
// -----------
// A configuration of a Floor.
// ==================================================

using UnityEngine;

namespace FGA.Configs
{
    public static class FloorConfig
    {
        public static int MIN_FLOOR_LEVEL = 1;
        public static int MAX_FLOOR_LEVEL = 15;
        public static Vector2Int FLOOR_POSITION = new Vector2Int(0, 0);
        public static int MIN_FLOOR_SIZE = 40;
        public static int MAX_FLOOR_SIZE = 400;
    }
}