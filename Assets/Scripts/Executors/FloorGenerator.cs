// ================================= //
// FloorGenerator.cs
// 
// Orchestrates the floor generation process.
// This is the MonoBehaviour entry point.
// 
// ** FIX: Updated to handle the new FloorGenerationResult and pass the 
// ** TileMap to the FloorView.
// ================================= //

using UnityEngine;
using static FloorController; // Allows direct access to TileType and FloorGenerationResult

public class FloorGenerator : MonoBehaviour
{
    // ----- Parameters ----- //

    [Header("Generation Parameters")]
    [Tooltip("Size of the square floor grid (e.g., 5 means 5x5 tiles).")]
    public int floorSize = 50;
    
    [Tooltip("Maximum depth for the BSP tree. Higher depth means smaller, more fragmented rooms.")]
    public int maxDepth = 5;

    [Tooltip("A random seed for reproducible generation. Set to 0 for a truly random map.")]
    public int seed = 0; 

    // ----- Dependencies ----- //

    [Header("View Dependencies")]
    [Tooltip("Reference to the FloorView component in the scene.")]
    public FloorView floorView; 

    // ----- Internal State ----- //

    private FloorModel currentFloor;
    // Store the generated map for potential later use/debugging
    private TileType[,] currentTileMap; 

    // Called when the script starts
    void Start()
    {
        if (floorView == null)
        {
            Debug.LogError("FloorView reference is missing. Please assign the FloorView component in the Inspector.");
            return;
        }

        GenerateNewFloor();
    }

    // Public method to be called (e.g., from a button or another script)
    [ContextMenu("Generate New Floor")]
    public void GenerateNewFloor()
    {
        // 1. Generate Data (Controller)
        Debug.Log($"[Generator] Starting floor generation ({floorSize}x{floorSize}) with max depth {maxDepth}, Seed: {seed}...");
        
        // Call the updated static GenerateFloor method, receiving the Model and the Map
        FloorGenerationResult result = FloorController.GenerateFloor(floorSize, maxDepth, seed);
        currentFloor = result.Model;
        currentTileMap = result.TileMap;

        // 2. Render Data (View) - Pass the unified map for efficient rendering
        // The FloorView's RenderFloor method signature has been updated.
        floorView.RenderFloor(currentFloor, currentTileMap);
        Debug.Log("[Generator] Floor generation and rendering sequence complete.");
    }
}
