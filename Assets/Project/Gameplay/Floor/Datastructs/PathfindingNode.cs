// PathfindingNode.cs
using UnityEngine;

/// <summary>
/// Represents a node in the A* pathfinding algorithm with position and cost data.
/// </summary>
public class PathfindingNode
{
    public Vector2Int Position { get; }
    public float GCost { get; set; }  // Cost from start node
    public float HCost { get; set; }  // Heuristic cost to target
    public float FCost => GCost + HCost;  // Total cost
    public PathfindingNode Parent { get; set; }

    public PathfindingNode(Vector2Int position)
    {
        Position = position;
    }

    /// <summary>
    /// Resets the node costs and parent for reuse.
    /// </summary>
    public void Reset()
    {
        GCost = 0;
        HCost = 0;
        Parent = null;
    }

    /// <summary>
    /// Calculates heuristic distance to target position using Manhattan distance.
    /// </summary>
    public float CalculateHeuristic(Vector2Int target)
    {
        return Mathf.Abs(Position.x - target.x) + Mathf.Abs(Position.y - target.y);
    }
}