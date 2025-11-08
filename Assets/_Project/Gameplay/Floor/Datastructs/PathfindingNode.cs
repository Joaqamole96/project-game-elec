using UnityEngine;

public class PathfindingNode
{
    public Vector2Int Position;
    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;
    public PathfindingNode Parent;
    
    public PathfindingNode(Vector2Int position)
    {
        Position = position; 
    }
}