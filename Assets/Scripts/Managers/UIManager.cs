// ================================================== //
// Scripts/Manager/UIManager.cs (FIXED)
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Prefab References")]
    public GameObject HUDUI;
    public GameObject mobileControlsUI;
    public GameObject shopUI;
    public GameObject pauseUI;
    public GameObject settingsUI;
    public GameObject gameOverUI;
    public GameObject damageUI;
    public GameObject tooltipUI;
    public GameObject crosshairUI;
    
    [Header("Active UI Instances")]
    private GameObject HUDUIInstance;
    private GameObject mobileControlsUIInstance;
    private GameObject shopUIInstance;
    private GameObject pauseUIInstance;
    private GameObject settingsModal;
    private GameObject gameOverModal;
    private GameObject tooltipInstance;
    
    [Header("Player Interface Components")]
    private Slider healthBar;
    private TextMeshProUGUI healthText;
    private MobileInputController inputManager;

    [Header("Crosshair")]
    private GameObject crosshairUIInstance;
    private CrosshairController crosshairController;
    
    private Canvas mainCanvas;
    private PlayerController player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // CRITICAL: Setup canvas FIRST before any UI instantiation
        SetupMainCanvas();
    }
    
    void Start()
    {
        player = PlayerController.Instance;
        
        // Initialize UI
        InitializePrefabs();
        InitializePlayerInterface();
        InitializeMobileControls();
        InitializeCrosshair(); // NEW
        
        // Load modal prefabs (don't instantiate yet)
        LoadModalPrefabs();
    }
    
    void Update()
    {
        if (HUDUIInstance != null && player != null)
        {
            UpdatePlayerInterface();
        }
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    // ==========================================
    // CANVAS SETUP (CRITICAL FIX)
    // ==========================================
    
    private void SetupMainCanvas()
    {
        // Get or create Canvas component
        mainCanvas = GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = gameObject.AddComponent<Canvas>();
        }
        
        // CRITICAL: Set to Screen Space - Overlay (not World Space!)
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 0;
        
        // Add CanvasScaler for resolution independence
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
        
        Debug.Log("UIManager: Canvas configured as Screen Space Overlay");
    }

    // ==========================================
    // PREFABS
    // ==========================================

    private void InitializePrefabs()
    {
        if (HUDUI == null)  HUDUI = ResourceService.LoadHUDUI();
        if (mobileControlsUI == null) mobileControlsUI = ResourceService.LoadMobileControlsUI();
        if (shopUI == null) shopUI = ResourceService.LoadShopUI();
        if (crosshairUI == null) crosshairUI = ResourceService.LoadCrosshairUI();
        if (damageUI == null) damageUI = Resources.Load<GameObject>("UI/popup_Damage");
    }
    
    // ==========================================
    // PLAYER INTERFACE (HUD)
    // ==========================================
    
    private void InitializePlayerInterface()
    {
        if (HUDUI != null)
        {
            // Instantiate HUD prefab as child of main canvas
            HUDUIInstance = Instantiate(HUDUI, mainCanvas.transform);
            HUDUIInstance.name = "PlayerInterface";
            
            // Cache component references from prefab
            CacheHUDComponents();
            
            Debug.Log("UIManager: Player interface initialized from prefab");
        }
        else
        {
            Debug.LogError("UIManager: No HUD prefab found.");
        }
    }
    
    private void CacheHUDComponents()
    {
        // Try to find components by name in the HUD hierarchy
        healthBar = FindComponentInChildren<Slider>(HUDUIInstance, "HealthBar");
        healthText = FindComponentInChildren<TextMeshProUGUI>(HUDUIInstance, "HealthText");
        
        // Log what we found
        Debug.Log($"UIManager: Cached HUD components - " +
                  $"HealthBar: {healthBar != null}, " +
                  $"HealthText: {healthText != null}, ");
    }
    
    private void UpdatePlayerInterface()
    {
        // Update health
        if (healthText != null)
        {
            healthText.text = $"HP: {player.CurrentHealth}/{player.maxHealth}";
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = player.maxHealth;
            healthBar.value = player.CurrentHealth;
        }
    }
    
    // ==========================================
    // MOBILE CONTROLS
    // ==========================================
    
    private void InitializeMobileControls()
    {
        if (mobileControlsUI == null) return;
        
        // Only show on mobile platforms
        // bool isMobile = Application.isMobilePlatform;
        bool isMobile = true;
        
        if (isMobile)
        {
            mobileControlsUIInstance = Instantiate(mobileControlsUI, mainCanvas.transform);
            mobileControlsUIInstance.name = "MobileControls";
            
            // Get MobileInputController component and initialize
            inputManager = mobileControlsUIInstance.GetComponent<MobileInputController>();
            if (inputManager == null)
            {
                inputManager = mobileControlsUIInstance.AddComponent<MobileInputController>();
            }
        }
    }
    
    // ==========================================
    // CROSSHAIR UI
    // ==========================================

    private void InitializeCrosshair()
    {
        if (crosshairUI != null)
        {
            crosshairUIInstance = Instantiate(crosshairUI, mainCanvas.transform);
            crosshairUIInstance.name = "Crosshair";
            
            // Get controller component
            crosshairController = crosshairUIInstance.GetComponent<CrosshairController>();
            if (crosshairController == null)
            {
                crosshairController = crosshairUIInstance.AddComponent<CrosshairController>();
            }
            
            Debug.Log("UIManager: Crosshair initialized");
        }
        else
        {
            Debug.LogWarning("UIManager: No crosshair UI prefab found");
        }
    }

    public void FlashCrosshair()
    {
        if (crosshairController != null)
        {
            crosshairController.Flash();
        }
    }
    
    // ==========================================
    // SHOP UI
    // ==========================================
    
    public void ShowShopDisplay(ShopController shop)
    {
        if (shopUI == null)
        {
            Debug.LogWarning("Shop modal prefab not assigned!");
            return;
        }
        
        // Instantiate if not exists
        if (shopUIInstance == null)
        {
            shopUIInstance = Instantiate(shopUI, mainCanvas.transform);
            shopUIInstance.name = "ShopUI";
            
            // Add ShopDisplay component if not present
            ShopDisplay shopDisplay = shopUIInstance.GetComponent<ShopDisplay>();
            if (shopDisplay == null)
            {
                shopDisplay = shopUIInstance.AddComponent<ShopDisplay>();
            }
            
            // Cache references
            CacheShopDisplayReferences(shopDisplay);
        }
        
        shopUIInstance.SetActive(true);
        
        // Open shop
        ShopDisplay ui = shopUIInstance.GetComponent<ShopDisplay>();
        if (ui != null)
        {
            ui.OpenShop(shop);
        }
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void CacheShopDisplayReferences(ShopDisplay shopDisplay)
    {
        // Find and assign UI components
        shopDisplay.shopPanel = FindChildByName(shopUIInstance, "ShopPanel");
        shopDisplay.itemsContainer = FindChildByName(shopUIInstance, "ItemsContainer")?.transform;
        shopDisplay.playerGoldText = FindComponentInChildren<TextMeshProUGUI>(shopUIInstance, "GoldText");
        shopDisplay.closeButton = FindComponentInChildren<Button>(shopUIInstance, "CloseButton");
        
        // Load item card prefab
        shopDisplay.itemCardPrefab = Resources.Load<GameObject>("UI/comp_ShopItem");
    }
    
    // ==========================================
    // PAUSE MODAL
    // ==========================================
    
    public void TogglePauseMenu()
    {
        if (pauseUI == null)
        {
            Debug.LogWarning("UIManager: pauseUI prefab not assigned");
            return;
        }
        
        if (pauseUIInstance == null)
        {
            pauseUIInstance = Instantiate(pauseUI, mainCanvas.transform);
            pauseUIInstance.name = "PauseModal";
            
            // CRITICAL: Setup buttons IMMEDIATELY after instantiation
            SetupPauseMenuButtons();
            
            Debug.Log("UIManager: Pause menu instantiated and buttons setup");
        }
        
        bool isActive = !pauseUIInstance.activeSelf;
        pauseUIInstance.SetActive(isActive);
        
        Time.timeScale = isActive ? 0f : 1f;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;
        
        Debug.Log($"UIManager: Pause menu toggled - Active: {isActive}");
    }
    
    private void SetupPauseMenuButtons()
    {
        if (pauseUIInstance == null)
        {
            Debug.LogError("UIManager: Cannot setup pause buttons - instance is null");
            return;
        }
        
        // Find buttons with multiple possible names
        Button resumeBtn = FindButtonInChildren(pauseUIInstance, "ResumeButton", "Resume", "ContinueButton");
        Button settingsBtn = FindButtonInChildren(pauseUIInstance, "SettingsButton", "Settings", "OptionsButton");
        Button quitBtn = FindButtonInChildren(pauseUIInstance, "ExitButton", "Quit", "MainMenuButton");
        
        // Setup Resume button
        if (resumeBtn != null)
        {
            resumeBtn.onClick.RemoveAllListeners();
            resumeBtn.onClick.AddListener(() => {
                Debug.Log("UIManager: Resume clicked");
                TogglePauseMenu();
            });
            Debug.Log("UIManager: Resume button connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Resume button not found");
        }
        
        // Setup Settings button
        if (settingsBtn != null)
        {
            settingsBtn.onClick.RemoveAllListeners();
            settingsBtn.onClick.AddListener(() => {
                Debug.Log("UIManager: Settings clicked");
                ShowSettingsModal();
            });
            Debug.Log("UIManager: Settings button connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Settings button not found");
        }
        
        // Setup Quit button
        if (quitBtn != null)
        {
            quitBtn.onClick.RemoveAllListeners();
            quitBtn.onClick.AddListener(() => {
                Debug.Log("UIManager: Quit clicked");
                QuitGame();
            });
            Debug.Log("UIManager: Quit button connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Quit button not found");
        }
    }
    
    // ==========================================
    // SETTINGS MODAL
    // ==========================================
    
    private void ShowSettingsModal()
    {
        if (settingsUI == null)
        {
            Debug.LogWarning("UIManager: settingsUI prefab not assigned");
            return;
        }
        
        if (settingsModal == null)
        {
            settingsModal = Instantiate(settingsUI, mainCanvas.transform);
            settingsModal.name = "SettingsModal";
            
            // CRITICAL: Setup controls IMMEDIATELY
            SetupSettingsControls();
            
            Debug.Log("UIManager: Settings modal instantiated and controls setup");
        }
        
        // Hide pause menu, show settings
        if (pauseUIInstance != null)
        {
            pauseUIInstance.SetActive(false);
        }
        settingsModal.SetActive(true);
        
        Debug.Log("UIManager: Settings modal shown");
    }
    
    private void SetupSettingsControls()
    {
        if (settingsModal == null)
        {
            Debug.LogError("UIManager: Cannot setup settings - modal is null");
            return;
        }
        
        // Find sliders with multiple possible names
        Slider musicSlider = FindSliderInChildren(settingsModal, "MusicSlider", "Music", "MusicVolume");
        Slider sfxSlider = FindSliderInChildren(settingsModal, "SFXSlider", "SFX", "SoundEffects");
        Slider sensitivitySlider = FindSliderInChildren(settingsModal, "SensitivitySlider", "Sensitivity", "CameraSensitivity");
        
        // Setup Music slider
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener((value) => {
                Debug.Log($"UIManager: Music volume changed to {value}");
                AudioManager audioManager = GameDirector.Instance?.audioManager;
                if (audioManager != null)
                {
                    audioManager.musicVolume = value;
                }
            });
            
            // Set initial value
            AudioManager audio = GameDirector.Instance?.audioManager;
            if (audio != null)
            {
                musicSlider.value = audio.musicVolume;
            }
            
            Debug.Log("UIManager: Music slider connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Music slider not found");
        }
        
        // Setup SFX slider
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener((value) => {
                Debug.Log($"UIManager: SFX volume changed to {value}");
                AudioManager audioManager = GameDirector.Instance?.audioManager;
                if (audioManager != null)
                {
                    audioManager.sfxVolume = value;
                }
            });
            
            AudioManager audio = GameDirector.Instance?.audioManager;
            if (audio != null)
            {
                sfxSlider.value = audio.sfxVolume;
            }
            
            Debug.Log("UIManager: SFX slider connected");
        }
        else
        {
            Debug.LogWarning("UIManager: SFX slider not found");
        }
        
        // Setup Sensitivity slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener((value) => {
                Debug.Log($"UIManager: Sensitivity changed to {value}");
                CameraController cam = Camera.main?.GetComponent<CameraController>();
                if (cam != null)
                {
                    cam.rotationSpeed = value * 5f;
                }
            });
            
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null)
            {
                sensitivitySlider.value = cam.rotationSpeed / 5f;
            }
            
            Debug.Log("UIManager: Sensitivity slider connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Sensitivity slider not found");
        }
        
        // Find and setup Back button
        Button backBtn = FindButtonInChildren(settingsModal, "BackButton", "Back", "CloseButton");
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(() => {
                Debug.Log("UIManager: Back from settings clicked");
                settingsModal.SetActive(false);
                if (pauseUIInstance != null)
                {
                    pauseUIInstance.SetActive(true);
                }
            });
            Debug.Log("UIManager: Back button connected");
        }
        else
        {
            Debug.LogWarning("UIManager: Back button not found in settings");
        }
    }
    
    // ==========================================
    // GAME OVER MODAL
    // ==========================================
    
    public void ShowGameOver(bool victory, int floorReached, int goldCollected, int enemiesKilled)
    {
        if (gameOverUI == null)
        {
            Debug.LogWarning("GameOver modal prefab not assigned, creating fallback");
            CreateFallbackGameOver(victory, floorReached, goldCollected, enemiesKilled);
            return;
        }
        
        if (gameOverModal == null)
        {
            gameOverModal = Instantiate(gameOverUI, mainCanvas.transform);
            gameOverModal.name = "GameOverModal";
        }
        
        gameOverModal.SetActive(true);
        
        // Update text
        TextMeshProUGUI titleText = FindComponentInChildren<TextMeshProUGUI>(gameOverModal, "TitleText");
        if (titleText != null)
        {
            titleText.text = victory ? "VICTORY!" : "GAME OVER";
            titleText.color = victory ? Color.yellow : Color.red;
        }
        
        TextMeshProUGUI statsText = FindComponentInChildren<TextMeshProUGUI>(gameOverModal, "StatsText");
        if (statsText != null)
        {
            statsText.text = $"Floor Reached: {floorReached}\n" +
                           $"Gold Collected: {goldCollected}\n" +
                           $"Enemies Defeated: {enemiesKilled}";
        }
        
        // Setup buttons
        Button retryBtn = FindComponentInChildren<Button>(gameOverModal, "RetryButton");
        Button mainMenuBtn = FindComponentInChildren<Button>(gameOverModal, "MainMenuButton");
        
        if (retryBtn != null) retryBtn.onClick.AddListener(RetryGame);
        if (mainMenuBtn != null) mainMenuBtn.onClick.AddListener(ReturnToMainMenu);
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void CreateFallbackGameOver(bool victory, int floorReached, int goldCollected, int enemiesKilled)
    {
        // Create simple game over screen
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = victory ? "VICTORY!" : "GAME OVER";
        title.fontSize = 72;
        title.color = victory ? Color.yellow : Color.red;
        title.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(600, 100);
        
        // Stats text
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(panel.transform, false);
        
        TextMeshProUGUI stats = statsObj.AddComponent<TextMeshProUGUI>();
        stats.text = $"Floor Reached: {floorReached}\nGold Collected: {goldCollected}\nEnemies Defeated: {enemiesKilled}";
        stats.fontSize = 36;
        stats.color = Color.white;
        stats.alignment = TextAlignmentOptions.Center;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.sizeDelta = new Vector2(600, 150);
        
        gameOverModal = panel;
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("UIManager: Created fallback game over screen");
    }
    
    // ==========================================
    // DAMAGE POPUP
    // ==========================================
    
    public void ShowDamageDisplay(Vector3 worldPosition, int damage, bool isCritical = false, bool isHeal = false)
    {
        if (damageUI == null)
        {
            Debug.LogWarning("UIManager: No damage popup prefab assigned");
            return;
        }
        
        // Spawn in world space (not as child of canvas)
        GameObject popup = Instantiate(damageUI, worldPosition, Quaternion.identity);
        
        DamageDisplay dmgNum = popup.GetComponent<DamageDisplay>();
        if (dmgNum == null)
        {
            dmgNum = popup.AddComponent<DamageDisplay>();
        }
        
        dmgNum.Initialize(damage, isCritical, isHeal);
    }
    
    // ==========================================
    // TOOLTIP
    // ==========================================
    
    public void ShowTooltip(string text, Vector2 screenPosition)
    {
        if (tooltipUI == null) return;
        
        if (tooltipInstance == null)
        {
            tooltipInstance = Instantiate(tooltipUI, mainCanvas.transform);
            tooltipInstance.name = "Tooltip";
        }
        
        tooltipInstance.SetActive(true);
        
        RectTransform rect = tooltipInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.position = screenPosition;
        }
        
        TextMeshProUGUI tooltipText = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipText != null)
        {
            tooltipText.text = text;
        }
    }
    
    public void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            tooltipInstance.SetActive(false);
        }
    }
    
    // ==========================================
    // UTILITY METHODS
    // ==========================================
    
    private void LoadModalPrefabs()
    {
        if (shopUI == null)
            shopUI = Resources.Load<GameObject>("UI/modal_Shop");
        
        if (pauseUI == null)
            pauseUI = Resources.Load<GameObject>("UI/modal_Pause");
        
        if (settingsUI == null)
            settingsUI = Resources.Load<GameObject>("UI/modal_Settings");
        
        if (gameOverUI == null)
            gameOverUI = Resources.Load<GameObject>("UI/modal_GameOver");
        
        if (tooltipUI == null)
            tooltipUI = Resources.Load<GameObject>("UI/popup_Tooltip");
    }
    
    private void SaveGame()
    {
        SaveManager saveManager = SaveManager.Instance;
        if (saveManager != null)
        {
            saveManager.SaveGame();
            Debug.Log("Game saved!");
        }
    }
    
    private void RetryGame()
    {
        Time.timeScale = 1f;
        SaveManager.Instance?.NewGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SaveManager.Instance?.SaveGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    private void QuitGame()
    {
        SaveManager.Instance?.SaveGame();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
        
        Debug.Log("UIManager: Quitting game");
    }
    
    // ==========================================
    // HELPER METHODS
    // ==========================================

    private Button FindButtonInChildren(GameObject parent, params string[] possibleNames)
    {
        if (parent == null) return null;
        
        // Try direct children first
        foreach (string name in possibleNames)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    Debug.Log($"UIManager: Found button '{name}' in direct children");
                    return btn;
                }
            }
        }
        
        // Deep search
        Button[] allButtons = parent.GetComponentsInChildren<Button>(true);
        Debug.Log($"UIManager: Found {allButtons.Length} buttons in hierarchy");
        
        foreach (Button btn in allButtons)
        {
            foreach (string name in possibleNames)
            {
                if (btn.gameObject.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug.Log($"UIManager: Found button matching '{name}': {btn.gameObject.name}");
                    return btn;
                }
            }
        }
        
        Debug.LogWarning($"UIManager: Button not found. Searched for: {string.Join(", ", possibleNames)}");
        return null;
    }

    private Slider FindSliderInChildren(GameObject parent, params string[] possibleNames)
    {
        if (parent == null) return null;
        
        // Try direct children first
        foreach (string name in possibleNames)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                Slider slider = child.GetComponent<Slider>();
                if (slider != null)
                {
                    Debug.Log($"UIManager: Found slider '{name}' in direct children");
                    return slider;
                }
            }
        }
        
        // Deep search
        Slider[] allSliders = parent.GetComponentsInChildren<Slider>(true);
        Debug.Log($"UIManager: Found {allSliders.Length} sliders in hierarchy");
        
        foreach (Slider slider in allSliders)
        {
            foreach (string name in possibleNames)
            {
                if (slider.gameObject.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug.Log($"UIManager: Found slider matching '{name}': {slider.gameObject.name}");
                    return slider;
                }
            }
        }
        
        Debug.LogWarning($"UIManager: Slider not found. Searched for: {string.Join(", ", possibleNames)}");
        return null;
    }
    
    private T FindComponentInChildren<T>(GameObject parent, string childName) where T : Component
    {
        GameObject child = FindChildByName(parent, childName);
        return child != null ? child.GetComponent<T>() : null;
    }
    
    private GameObject FindChildByName(GameObject parent, string name)
    {
        if (parent == null) return null;
        
        // Check direct children first
        foreach (Transform child in parent.transform)
        {
            if (child.name == name) return child.gameObject;
        }
        
        // Recursive search
        foreach (Transform child in parent.transform)
        {
            GameObject found = FindChildByName(child.gameObject, name);
            if (found != null) return found;
        }
        
        return null;
    }
}