// ================================================== //
// Scripts/UI/MainMenuDirector.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls main menu UI and navigation
/// </summary>
public class MainMenuDirector : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenu;
    public GameObject settings;
    
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider cameraSensitivitySlider;
    public Button backFromSettingsButton;
    
    void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
        CheckSaveData();
    }
    
    private void InitializeMenu()
    {
        // Show main menu, hide others
        if (mainMenu != null) mainMenu.SetActive(true);
        if (settings != null) settings.SetActive(false);
        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Load settings
        LoadSettings();
    }
    
    private void SetupButtonListeners()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (backFromSettingsButton != null) backFromSettingsButton.onClick.AddListener(OnBackFromSettings);
        
        // Settings sliders
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (cameraSensitivitySlider != null) cameraSensitivitySlider.onValueChanged.AddListener(OnCameraSensitivityChanged);
    }
    
    private void CheckSaveData()
    {
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        if (continueButton != null) continueButton.interactable = hasSave;
    }
    
    // ------------------------- //
    // BUTTON HANDLERS
    // ------------------------- //
    
    private void OnNewGame()
    {
        Debug.Log("MainMenu: Starting new game...");
        
        if (SaveManager.Instance != null) SaveManager.Instance.NewGame();
        
        // Load game scene
        SceneManager.LoadScene("GameScene");
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
        
        if (mainMenu != null) mainMenu.SetActive(false);
        if (settings != null) settings.SetActive(true);
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
        if (settings != null) settings.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);
        
        SaveSettings();
    }
    
    // ------------------------- //
    // SETTINGS
    // ------------------------- //
    
    private void LoadSettings()
    {
        if (SaveManager.Instance != null)
        {
            var (music, sfx, sensitivity) = SaveManager.Instance.LoadSettings();
            
            if (musicVolumeSlider != null) musicVolumeSlider.value = music;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
            if (cameraSensitivitySlider != null) cameraSensitivitySlider.value = sensitivity;
            
            ApplySettings(music, sfx, sensitivity);
        }
    }
    
    private void SaveSettings()
    {
        if (SaveManager.Instance != null)
        {
            float music = musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f;
            float sfx = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
            float sensitivity = cameraSensitivitySlider != null ? cameraSensitivitySlider.value : 0.5f;
            
            SaveManager.Instance.SaveSettings(music, sfx, sensitivity);
        }
    }
    
    private void ApplySettings(float music, float sfx, float sensitivity)
    {
        AudioManager audioManager = GameDirector.Instance?.audioManager;
        if (audioManager != null)
        {
            audioManager.musicVolume = music;
            audioManager.sfxVolume = sfx;
        }
        
        // Apply camera sensitivity to game settings
        // if (GameDirector.Instance != null)
        // {
        //     GameDirector.Instance.cameraSensitivity = sensitivity;
        // }
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
    
    private void OnCameraSensitivityChanged(float value)
    {
        // if (GameDirector.Instance != null)
        // {
        //     GameDirector.Instance.cameraSensitivity = value;
        // }
    }
}