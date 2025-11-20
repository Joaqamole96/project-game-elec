// GameConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for core gameplay mechanics and balancing.
/// </summary>
[System.Serializable]
public class GameConfig
{
    [Header("Geometry Settings")]
    [Tooltip("Simplify geometry for better performance on mobile devices")]
    public bool SimplifyGeometry = true;
}