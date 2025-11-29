// ================================================== //
// Scripts/Services/SaveService.cs
// ================================================== //

using UnityEngine;

/// <summary>
/// Service for managing game save operations at key gameplay moments
/// Bridges between game events and the SaveManager singleton
/// Attach to GameDirector or relevant manager objects
/// </summary>

// NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
/*
public class SaveService : MonoBehaviour
{
    private SaveManager saveManager;
    
    void Start()
    {
        try
        {
            saveManager = SaveManager.Instance;
            if (saveManager == null) Debug.LogWarning("SaveService: SaveManager instance not found - save functionality disabled");
            else Debug.Log("SaveService: Successfully initialized with SaveManager");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveService: Error during initialization: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Called when player dies - intentionally does not save to allow reloading last save
    /// </summary>
    public void OnPlayerDied()
    {
        try
        {
            Debug.Log("SaveService: Player died - intentionally not saving to allow reload from last save");
            // Note: This allows players to reload their last save point
            // rather than saving the failed state
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveService: Error in OnPlayerDied: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Called when player completes a floor - triggers floor completion save logic
    /// </summary>
    public void OnFloorCleared()
    {
        try
        {
            if (saveManager != null)
            {
                saveManager.OnFloorCompleted();
                Debug.Log("SaveService: Floor cleared - progress saved");
            }
            else
            {
                Debug.LogWarning("SaveService: Cannot save floor completion - SaveManager not available");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveService: Error saving floor completion: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Called when boss is defeated - triggers immediate game save
    /// </summary>
    public void OnBossDefeated()
    {
        try
        {
            if (saveManager != null)
            {
                saveManager.SaveGame();
                Debug.Log("SaveService: Boss defeated - game saved");
            }
            else
            {
                Debug.LogWarning("SaveService: Cannot save boss defeat - SaveManager not available");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveService: Error saving boss defeat: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Manual save trigger for other game events
    /// </summary>
    public void ManualSave()
    {
        try
        {
            if (saveManager != null)
            {
                saveManager.SaveGame();
                Debug.Log("SaveService: Manual save completed");
            }
            else
            {
                Debug.LogWarning("SaveService: Cannot perform manual save - SaveManager not available");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveService: Error during manual save: {ex.Message}");
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("SaveService: Service destroyed");
    }
}
*/