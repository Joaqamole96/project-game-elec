using System;
using UnityEngine;

namespace Configs.FGA
{
    public static class TilemapConfig
    {
        public enum TileType
        {
            None,
            Floor, Wall, Corridor, Door,
            // Future Implementations
            Hazard, Boost, Pillar
        }
    }
}