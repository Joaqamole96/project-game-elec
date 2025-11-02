// TileMapBaker.cs
using System.Collections.Generic;
using UnityEngine;
using FGA.Models;

namespace FGA.Presentation
{
    public static class TileMapBaker
    {
        // returns TileType[,] sized [width,height]
        public static TileType[,] Bake(FloorModel model, bool fillOuterWalls = true)
        {
            var w = model.width; var h = model.height;
            var tiles = new TileType[w, h];
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) tiles[x, y] = TileType.Void;

            // mark rooms
            foreach (var r in model.rooms)
            {
                for (int x = r.rect.xMin; x < r.rect.xMax; x++)
                    for (int y = r.rect.yMin; y < r.rect.yMax; y++)
                        if (InBounds(x, y, w, h)) tiles[x, y] = TileType.Floor;
            }

            // mark corridors
            foreach (var c in model.corridors)
                foreach (var t in c.tiles) if (InBounds(t.x, t.y, w, h)) tiles[t.x, t.y] = TileType.Floor;

            // doors / keys / locked
            foreach (var d in model.doorTiles) if (InBounds(d.x, d.y, w, h)) tiles[d.x, d.y] = TileType.Door;
            foreach (var k in model.keyTiles) if (InBounds(k.x, k.y, w, h)) tiles[k.x, k.y] = TileType.Key;
            foreach (var ld in model.lockedDoorTiles) if (InBounds(ld.x, ld.y, w, h)) tiles[ld.x, ld.y] = TileType.LockedDoor;

            // walls adjacent to floor
            int[] dx = { 1, -1, 0, 0 }; int[] dy = { 0, 0, 1, -1 };
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (tiles[x, y] != TileType.Void) continue;
                    bool adj = false;
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + dx[i], ny = y + dy[i];
                        if (!InBounds(nx, ny, w, h)) continue;
                        var t = tiles[nx, ny];
                        if (t == TileType.Floor || t == TileType.Door || t == TileType.Key || t == TileType.LockedDoor) { adj = true; break; }
                    }
                    if (adj) tiles[x, y] = TileType.Wall;
                }

            // optional fill remaining voids with wall
            if (fillOuterWalls)
                for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) if (tiles[x, y] == TileType.Void) tiles[x, y] = TileType.Wall;

            // copy back into model tiles for convenience
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) model.SetTile(x, y, tiles[x, y]);

            return tiles;
        }

        static bool InBounds(int x, int y, int w, int h) => x >= 0 && y >= 0 && x < w && y < h;
    }
}