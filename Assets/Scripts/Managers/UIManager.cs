// -------------------------------------------------- //
// Scripts/Managers/UIManager.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Manages all UI elements (menus, HUD, screens, dialogs).
/// Placeholder for future implementation.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas mainCanvas;
    public GameObject mainMenuPrefab;
    public GameObject hudPrefab;
    public GameObject pauseMenuPrefab;
    public GameObject gameOverScreenPrefab;
    
    [Header("Current UI")]
    public GameObject currentHUD;
    public GameObject currentMenu;
    
    void Awake()
    {
        Debug.Log("UIManager: Initialized (placeholder)");
    }
    
    public void ShowMainMenu()
    {
        Debug.Log("UIManager: ShowMainMenu() - Not yet implemented");
        // TODO: Implement
    }
    
    public void ShowHUD()
    {
        Debug.Log("UIManager: ShowHUD() - Not yet implemented");
        // TODO: Implement
    }
    
    public void ShowPauseMenu()
    {
        Debug.Log("UIManager: ShowPauseMenu() - Not yet implemented");
        // TODO: Implement
    }
    
    public void ShowGameOverScreen()
    {
        Debug.Log("UIManager: ShowGameOverScreen() - Not yet implemented");
        // TODO: Implement
    }
    
    public void UpdateHealthBar(int current, int max)
    {
        // Debug.Log($"UIManager: Health {current}/{max}");
        // TODO: Implement
    }
}

