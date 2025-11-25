// ================================================== //
// Scripts/UI/HUDController.cs - HUD Controller
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game HUD display (health, gold, floor, etc.)
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI powerCountText;
    
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    
    private PlayerController player;
    private bool isPaused = false;
    
    void Start()
    {
        player = PlayerController.Instance;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        SetupPauseMenu();
    }
    
    void Update()
    {
        UpdateHUD();
        HandlePauseInput();
    }
    
    private void UpdateHUD()
    {
        if (player == null) return;
        
        // Health bar
        if (healthBar != null && player != null)
        {
            healthBar.maxValue = player.maxHealth;
            healthBar.value = player.CurrentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{player.CurrentHealth}/{player.maxHealth}";
        }
        
        // Gold
        if (goldText != null && player.inventory != null)
        {
            goldText.text = $"Gold: {player.inventory.gold}";
        }
        
        // Floor
        if (floorText != null)
        {
            LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
            if (layoutManager != null && layoutManager.LevelConfig != null)
            {
                floorText.text = $"Floor: {layoutManager.LevelConfig.FloorLevel}";
            }
        }
        
        // Power count
        if (powerCountText != null && player.powerManager != null)
        {
            powerCountText.text = $"Powers: {player.powerManager.activePowers.Count}/{player.powerManager.maxPowers}";
        }
    }
    
    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        
        Time.timeScale = isPaused ? 0f : 1f;
        
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
    
    private void SetupPauseMenu()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResume);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }
    
    private void OnResume()
    {
        TogglePause();
    }
    
    private void OnSettings()
    {
        // TODO: Show settings panel
        Debug.Log("HUDController: Settings (not implemented)");
    }
    
    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        
        // Save before returning to menu
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}