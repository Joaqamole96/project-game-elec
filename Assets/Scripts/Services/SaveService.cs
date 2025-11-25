// ================================================== //
// Scripts/Systems/SaveService.cs - Helper
// ================================================== //

using UnityEngine;

/// <summary>
/// Helper class to trigger saves at appropriate times
/// Attach to GameDirector or relevant managers
/// </summary>
public class SaveService : MonoBehaviour
{
    private SaveManager saveManager;
    
    void Start()
    {
        saveManager = SaveManager.Instance;
        
        if (saveManager == null)
        {
            Debug.LogWarning("SaveService: SaveManager not found!");
        }
    }
    
    // Call these from appropriate places in your code
    
    public void OnPlayerDied()
    {
        // Don't save on death - let player reload last save
        Debug.Log("SaveService: Player died - not saving");
    }
    
    public void OnFloorCleared()
    {
        if (saveManager != null)
        {
            saveManager.OnFloorCompleted();
        }
    }
    
    public void OnBossDefeated()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame();
        }
    }
}