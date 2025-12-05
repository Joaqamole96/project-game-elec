// ================================================== //
// Scripts/Renderers/CorridorRenderer.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CorridorRenderer
{
    private GameObject _floorPrefab;
    private GameObject _wallPrefab;
    
    public CorridorRenderer()
    {
        LoadPrefabs();
    }
    
    private void LoadPrefabs()
    {
        _floorPrefab = ResourceService.LoadFloorPrefab();
        _wallPrefab = ResourceService.LoadWallPrefab();
    }
    
    public void RenderCorridors(LevelModel layout, List<RoomModel> rooms, Transform parent, string biome)
    {
        if (layout?.Corridors == null || rooms == null)
        {
            Debug.LogWarning("CorridorRenderer: Invalid layout or rooms");
            return;
        }
        Material floorMat = ResourceService.LoadFloorMaterial(biome);
        Material wallMat = ResourceService.LoadWallMaterial(biome);
        // Get pure corridor tiles (not in any room)
        HashSet<Vector2Int> corridorTiles = GetPureCorridorTiles(layout, rooms);
        if (corridorTiles.Count == 0)
        {
            Debug.Log("CorridorRenderer: No corridor tiles to render");
            return;
        }
        // Group corridor tiles into straight segments
        List<CorridorSegment> segments = IdentifyCorridorSegments(corridorTiles);
        Debug.Log($"CorridorRenderer: Found {segments.Count} corridor segments from {corridorTiles.Count} tiles");
        // Render each segment
        foreach (var segment in segments) RenderCorridorSegment(segment, parent, floorMat, wallMat);
    }
    
    // ==========================================
    // CORRIDOR IDENTIFICATION
    // ==========================================
    
    private struct CorridorSegment
    {
        public Vector2Int start;
        public Vector2Int end;
        public bool isHorizontal;
        
        public int GetLength()
            => isHorizontal ? 
                Mathf.Abs(end.x - start.x) + 1 : 
                Mathf.Abs(end.y - start.y) + 1;
        
        public Vector3 GetCenter()
            => new(
                (start.x + end.x) / 2f + 0.5f,
                0.5f,
                (start.y + end.y) / 2f + 0.5f
            );
    }
    
    private HashSet<Vector2Int> GetPureCorridorTiles(LevelModel layout, List<RoomModel> rooms)
    {
        HashSet<Vector2Int> corridorTiles = new();
        foreach (var corridor in layout.Corridors)
        {
            if (corridor?.Tiles == null) continue;
            foreach (var tile in corridor.Tiles)
            {
                // Check if tile is in any room
                bool inRoom = rooms.Any(room => room != null && room.ContainsPosition(tile));
                if (!inRoom) corridorTiles.Add(tile);
            }
        }
        return corridorTiles;
    }
    
    private List<CorridorSegment> IdentifyCorridorSegments(HashSet<Vector2Int> tiles)
    {
        List<CorridorSegment> segments = new();
        HashSet<Vector2Int> processed = new();
        foreach (var tile in tiles)
        {
            if (processed.Contains(tile)) continue;
            // Try to extend horizontally (along X axis)
            var horizontalSegment = TryExtendSegment(tile, tiles, processed, true);
            if (horizontalSegment.GetLength() > 1)
            {
                segments.Add(horizontalSegment);
                continue;
            }
            // Try to extend vertically (along Z axis)
            var verticalSegment = TryExtendSegment(tile, tiles, processed, false);
            if (verticalSegment.GetLength() > 1)
            {
                segments.Add(verticalSegment);
                continue;
            }
            // Single isolated tile (shouldn't happen, but handle it)
            segments.Add(new CorridorSegment { 
                start = tile, 
                end = tile, 
                isHorizontal = true 
            });
            processed.Add(tile);
        }
        return segments;
    }
    
    private CorridorSegment TryExtendSegment(Vector2Int start, HashSet<Vector2Int> tiles, HashSet<Vector2Int> processed, bool horizontal)
    {
        Vector2Int current = start;
        Vector2Int direction = horizontal ? new Vector2Int(1, 0) : new Vector2Int(0, 1);
        Vector2Int end = start;
        // Mark start as processed
        processed.Add(current);
        // Extend forward
        while (true)
        {
            Vector2Int next = current + direction;
            if (!tiles.Contains(next) || processed.Contains(next)) break;
            processed.Add(next);
            current = next;
            end = next;
        }
        // Extend backward from start
        current = start;
        Vector2Int backwardDirection = -direction;
        Vector2Int actualStart = start;
        while (true)
        {
            Vector2Int next = current + backwardDirection;
            if (!tiles.Contains(next) || processed.Contains(next)) break;
            processed.Add(next);
            current = next;
            actualStart = next;
        }
        return new CorridorSegment
        {
            start = actualStart,
            end = end,
            isHorizontal = horizontal
        };
    }
    
    // ==========================================
    // CORRIDOR RENDERING
    // ==========================================
    
    private void RenderCorridorSegment(CorridorSegment segment, Transform parent, Material floorMat, Material wallMat)
    {
        GameObject container = new($"Corridor_{segment.start.x}_{segment.start.y}");
        container.transform.SetParent(parent);
        // Render stretched floor
        RenderCorridorFloor(segment, container.transform, floorMat);
        // Render walls along both sides
        RenderCorridorWalls(segment, container.transform, wallMat);
    }
    
    private void RenderCorridorFloor(CorridorSegment segment, Transform parent, Material material)
    {
        if (_floorPrefab == null) return;
        int length = segment.GetLength();
        Vector3 center = segment.GetCenter();
        GameObject floor = Object.Instantiate(_floorPrefab, center, Quaternion.identity, parent);
        floor.name = "Floor";
        // Stretch floor based on orientation
        if (segment.isHorizontal) floor.transform.localScale = new Vector3(length, 1, 2);
        else floor.transform.localScale = new Vector3(2, 1, length);
        ApplyMaterial(floor, material);
    }
    
    private void RenderCorridorWalls(CorridorSegment segment, Transform parent, Material material)
    {
        if (_wallPrefab == null) return;
        int length = segment.GetLength();
        Vector3 center = segment.GetCenter();
        if (segment.isHorizontal)
        {
            // North wall (rotation = 0, extends along X)
            Vector3 northPos = center + new Vector3(0, 5f, 1f);
            GameObject northWall = Object.Instantiate(_wallPrefab, northPos, Quaternion.identity, parent);
            northWall.name = "NorthWall";
            northWall.transform.localScale = new Vector3(length + 1, 1, 1);
            ApplyMaterial(northWall, material);
            // South wall (rotation = 0, extends along X)
            Vector3 southPos = center + new Vector3(0, 5f, -1f);
            GameObject southWall = Object.Instantiate(_wallPrefab, southPos, Quaternion.identity, parent);
            southWall.name = "SouthWall";
            southWall.transform.localScale = new Vector3(length + 1, 1, 1);
            ApplyMaterial(southWall, material);
        }
        else
        {
            // East wall (rotation = 90, extends along Z)
            Vector3 eastPos = center + new Vector3(1f, 5f, 0);
            GameObject eastWall = Object.Instantiate(_wallPrefab, eastPos, Quaternion.Euler(0, 90, 0), parent);
            eastWall.name = "EastWall";
            eastWall.transform.localScale = new Vector3(length + 1, 1, 1);
            ApplyMaterial(eastWall, material);
            // West wall (rotation = 90, extends along Z)
            Vector3 westPos = center + new Vector3(-1f, 5f, 0);
            GameObject westWall = Object.Instantiate(_wallPrefab, westPos, Quaternion.Euler(0, 90, 0), parent);
            westWall.name = "WestWall";
            westWall.transform.localScale = new Vector3(length + 1, 1, 1);
            ApplyMaterial(westWall, material);
        }
    }
    
    // ==========================================
    // UTILITY
    // ==========================================
    
    private void ApplyMaterial(GameObject obj, Material material)
    {
        if (obj == null || material == null) return;
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers) renderer.sharedMaterial = material;
    }
}