// ================================================== //
// Scripts/UI/MainMenuController.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls main menu UI and navigation
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button backFromSettingsButton;
    
    [Header("Credits")]
    public Button backFromCreditsButton;
    
    void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
        CheckSaveData();
    }
    
    private void InitializeMenu()
    {
        // Show main menu, hide others
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Load settings
        LoadSettings();
    }
    
    private void SetupButtonListeners()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCredits);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
        
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(OnBackFromSettings);
        
        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(OnBackFromCredits);
        
        // Settings sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }
    
    private void CheckSaveData()
    {
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        
        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
        }
    }
    
    // ------------------------- //
    // BUTTON HANDLERS
    // ------------------------- //
    
    private void OnNewGame()
    {
        Debug.Log("MainMenu: Starting new game...");
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.NewGame();
        }
        
        // Load game scene
        SceneManager.LoadScene("GameScene"); // Change to your game scene name
    }
    
    private void OnContinue()
    {
        Debug.Log("MainMenu: Continuing game...");
        
        // Load game scene
        SceneManager.LoadScene("GameScene");
        
        // SaveManager will auto-load in game scene
    }
    
    private void OnSettings()
    {
        Debug.Log("MainMenu: Opening settings...");
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    private void OnCredits()
    {
        Debug.Log("MainMenu: Opening credits...");
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
    
    private void OnQuit()
    {
        Debug.Log("MainMenu: Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void OnBackFromSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        SaveSettings();
    }
    
    private void OnBackFromCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    
    // ------------------------- //
    // SETTINGS
    // ------------------------- //
    
    private void LoadSettings()
    {
        if (SaveManager.Instance != null)
        {
            var (master, music, sfx) = SaveManager.Instance.LoadSettings();
            
            if (masterVolumeSlider != null) masterVolumeSlider.value = master;
            if (musicVolumeSlider != null) musicVolumeSlider.value = music;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
            
            ApplySettings(master, music, sfx);
        }
    }
    
    private void SaveSettings()
    {
        if (SaveManager.Instance != null)
        {
            float master = masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
            float music = musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f;
            float sfx = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
            
            SaveManager.Instance.SaveSettings(master, music, sfx);
        }
    }
    
    private void ApplySettings(float master, float music, float sfx)
    {
        AudioManager audioManager = GameDirector.Instance?.audioManager;
        if (audioManager != null)
        {
            audioManager.masterVolume = master;
            audioManager.musicVolume = music;
            audioManager.sfxVolume = sfx;
        }
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        AudioManager audioManager = GameDirector.Instance?.audioManager;
        if (audioManager != null)
        {
            audioManager.masterVolume = value;
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager audioManager = GameDirector.Instance?.audioManager;
        if (audioManager != null)
        {
            audioManager.musicVolume = value;
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager audioManager = GameDirector.Instance?.audioManager;
        if (audioManager != null)
        {
            audioManager.sfxVolume = value;
        }
    }
}





// ================================================== //
// Integration: Update PlayerController.cs Die() method
// ================================================== //

/*
In PlayerController.cs, update the Die() method:


*/