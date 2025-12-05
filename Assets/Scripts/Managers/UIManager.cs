// ================================================== //
// Scripts/Manager/UIManager.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Prefab References")]
    public GameObject playerInterfacePrefab;    // interface_Player
    public GameObject mobileControlsPrefab;     // interface_MobileControls
    public GameObject shopModalPrefab;          // modal_Shop
    public GameObject pauseModalPrefab;         // modal_Pause
    public GameObject settingsModalPrefab;      // modal_Settings
    public GameObject gameOverModalPrefab;      // modal_GameOver
    public GameObject damagePopupPrefab;        // popup_Damage
    public GameObject tooltipPrefab;            // popup_Tooltip
    
    [Header("Active UI Instances")]
    private GameObject playerInterface;
    private GameObject mobileControls;
    private GameObject shopModal;
    private GameObject pauseModal;
    private GameObject settingsModal;
    private GameObject gameOverModal;
    private GameObject tooltipInstance;
    
    [Header("Player Interface Components")]
    private Slider healthBar;
    private TextMeshProUGUI healthText;
    private Slider manaBar;
    private TextMeshProUGUI manaText;
    
    private Canvas mainCanvas;
    private PlayerController player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get or create main canvas
        mainCanvas = GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = gameObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    void Start()
    {
        player = PlayerController.Instance;
        
        // Initialize UI
        InitializePrefabs();
        InitializePlayerInterface();
        InitializeMobileControls();
        
        // Load modal prefabs (don't instantiate yet)
        LoadModalPrefabs();
    }
    
    void Update()
    {
        if (playerInterface != null && player != null)
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
    // PREFABS
    // ==========================================

    private void InitializePrefabs()
    {
        playerInterfacePrefab = ResourceService.LoadHUDPrefab();
        // mobileControls;
        // shopModal;
        // pauseModal;
        // settingsModal;
        // gameOverModal;
        // tooltipInstance;
    }
    
    // ==========================================
    // PLAYER INTERFACE (HUD)
    // ==========================================
    
    private void InitializePlayerInterface()
    {
        if (playerInterfacePrefab == null)
        {
            Debug.LogWarning("Player interface prefab not assigned!");
            return;
        }
        
        playerInterface = Instantiate(playerInterfacePrefab, transform);
        playerInterface.name = "PlayerInterface";
        
        // Cache component references
        healthBar = FindComponentInChildren<Slider>(playerInterface, "HealthBar");
        healthText = FindComponentInChildren<TextMeshProUGUI>(playerInterface, "HealthText");
        manaBar = FindComponentInChildren<Slider>(playerInterface, "ManaBar");
        manaText = FindComponentInChildren<TextMeshProUGUI>(playerInterface, "ManaText");

        playerInterface.transform.SetParent(mainCanvas.transform);
    }
    
    private void UpdatePlayerInterface()
    {
        // Update health
        if (healthBar != null)
        {
            healthBar.maxValue = player.maxHealth;
            healthBar.value = player.CurrentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{player.CurrentHealth}/{player.maxHealth}";
        }
        
        // Update mana (if implemented)
        // TODO: Add mana system to player
    }
    
    // ==========================================
    // MOBILE CONTROLS
    // ==========================================
    
    private void InitializeMobileControls()
    {
        if (mobileControlsPrefab == null) return;
        
        // Only show on mobile platforms
        bool isMobile = Application.isMobilePlatform;
        
        if (isMobile)
        {
            mobileControls = Instantiate(mobileControlsPrefab, transform);
            mobileControls.name = "MobileControls";
            
            // Get MobileInputManager component and initialize
            MobileInputManager inputManager = mobileControls.GetComponent<MobileInputManager>();
            if (inputManager == null)
            {
                inputManager = mobileControls.AddComponent<MobileInputManager>();
            }
        }
    }
    
    // ==========================================
    // SHOP MODAL
    // ==========================================
    
    public void ShowShopDisplay(ShopController shop)
    {
        if (shopModalPrefab == null)
        {
            Debug.LogWarning("Shop modal prefab not assigned!");
            return;
        }
        
        // Instantiate if not exists
        if (shopModal == null)
        {
            shopModal = Instantiate(shopModalPrefab, transform);
            shopModal.name = "ShopModal";
            
            // Add ShopDisplay component if not present
            ShopDisplay shopDisplay = shopModal.GetComponent<ShopDisplay>();
            if (shopDisplay == null)
            {
                shopDisplay = shopModal.AddComponent<ShopDisplay>();
            }
            
            // Cache references
            CacheShopDisplayReferences(shopDisplay);
        }
        
        shopModal.SetActive(true);
        
        // Open shop
        ShopDisplay ui = shopModal.GetComponent<ShopDisplay>();
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
        shopDisplay.shopPanel = FindChildByName(shopModal, "ShopPanel");
        shopDisplay.itemsContainer = FindChildByName(shopModal, "ItemsContainer")?.transform;
        shopDisplay.playerGoldText = FindComponentInChildren<TextMeshProUGUI>(shopModal, "GoldText");
        shopDisplay.closeButton = FindComponentInChildren<Button>(shopModal, "CloseButton");
        
        // Load item card prefab
        shopDisplay.itemCardPrefab = Resources.Load<GameObject>("UI/comp_ShopItem");
    }
    
    // ==========================================
    // PAUSE MODAL
    // ==========================================
    
    public void TogglePauseMenu()
    {
        if (pauseModalPrefab == null) return;
        
        if (pauseModal == null)
        {
            pauseModal = Instantiate(pauseModalPrefab, transform);
            pauseModal.name = "PauseModal";
            
            // Setup button listeners
            SetupPauseMenuButtons();
        }
        
        bool isActive = !pauseModal.activeSelf;
        pauseModal.SetActive(isActive);
        
        // Pause/unpause game
        Time.timeScale = isActive ? 0f : 1f;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;
    }
    
    private void SetupPauseMenuButtons()
    {
        Button resumeBtn = FindComponentInChildren<Button>(pauseModal, "ResumeButton");
        Button settingsBtn = FindComponentInChildren<Button>(pauseModal, "SettingsButton");
        Button quitBtn = FindComponentInChildren<Button>(pauseModal, "ExitButton");
        
        if (resumeBtn != null) resumeBtn.onClick.AddListener(TogglePauseMenu);
        if (settingsBtn != null) settingsBtn.onClick.AddListener(ShowSettingsModal);
        if (quitBtn != null) quitBtn.onClick.AddListener(QuitGame);
    }
    
    // ==========================================
    // SETTINGS MODAL
    // ==========================================
    
    private void ShowSettingsModal()
    {
        if (settingsModalPrefab == null) return;
        
        if (settingsModal == null)
        {
            settingsModal = Instantiate(settingsModalPrefab, transform);
            settingsModal.name = "SettingsModal";
            
            SetupSettingsControls();
        }
        
        // Hide pause menu, show settings
        if (pauseModal != null) pauseModal.SetActive(false);
        settingsModal.SetActive(true);
    }
    
    private void SetupSettingsControls()
    {
        // Music slider
        Slider musicSlider = FindComponentInChildren<Slider>(settingsModal, "MusicSlider");
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener((value) => {
                AudioManager audioManager = GameDirector.Instance?.audioManager;
                if (audioManager != null) audioManager.musicVolume = value;
            });
        }
        
        // SFX slider
        Slider sfxSlider = FindComponentInChildren<Slider>(settingsModal, "SFXSlider");
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener((value) => {
                AudioManager audioManager = GameDirector.Instance?.audioManager;
                if (audioManager != null) audioManager.sfxVolume = value;
            });
        }
        
        // Camera sensitivity
        Slider sensitivitySlider = FindComponentInChildren<Slider>(settingsModal, "SensitivitySlider");
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener((value) => {
                CameraController cam = Camera.main?.GetComponent<CameraController>();
                if (cam != null) cam.rotationSpeed = value * 5f;
            });
        }
        
        // Back button
        Button backBtn = FindComponentInChildren<Button>(settingsModal, "BackButton");
        if (backBtn != null)
        {
            backBtn.onClick.AddListener(() => {
                settingsModal.SetActive(false);
                if (pauseModal != null) pauseModal.SetActive(true);
            });
        }
    }
    
    // ==========================================
    // GAME OVER MODAL
    // ==========================================
    
    public void ShowGameOver(bool victory, int floorReached, int goldCollected, int enemiesKilled)
    {
        if (gameOverModalPrefab == null) return;
        
        if (gameOverModal == null)
        {
            gameOverModal = Instantiate(gameOverModalPrefab, transform);
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
    
    // ==========================================
    // DAMAGE POPUP
    // ==========================================
    
    public void ShowDamageDisplay(Vector3 worldPosition, int damage, bool isCritical = false, bool isHeal = false)
    {
        if (damagePopupPrefab == null) return;
        
        GameObject popup = Instantiate(damagePopupPrefab, worldPosition, Quaternion.identity);
        
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
        if (tooltipPrefab == null) return;
        
        if (tooltipInstance == null)
        {
            tooltipInstance = Instantiate(tooltipPrefab, transform);
            tooltipInstance.name = "Tooltip";
        }
        
        tooltipInstance.SetActive(true);
        tooltipInstance.transform.position = screenPosition;
        
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
        if (shopModalPrefab == null)
            shopModalPrefab = Resources.Load<GameObject>("UI/modal_Shop");
        
        if (pauseModalPrefab == null)
            pauseModalPrefab = Resources.Load<GameObject>("UI/modal_Pause");
        
        if (settingsModalPrefab == null)
            settingsModalPrefab = Resources.Load<GameObject>("UI/modal_Settings");
        
        if (gameOverModalPrefab == null)
            gameOverModalPrefab = Resources.Load<GameObject>("UI/modal_GameOver");
        
        if (damagePopupPrefab == null)
            damagePopupPrefab = Resources.Load<GameObject>("UI/popup_Damage");
        
        if (tooltipPrefab == null)
            tooltipPrefab = Resources.Load<GameObject>("UI/popup_Tooltip");
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
    }
    
    // ==========================================
    // HELPER METHODS
    // ==========================================
    
    private T FindComponentInChildren<T>(GameObject parent, string childName) where T : Component
    {
        GameObject child = FindChildByName(parent, childName);
        return child != null ? child.GetComponent<T>() : null;
    }
    
    private GameObject FindChildByName(GameObject parent, string name)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent.transform)
        {
            if (child.name == name) return child.gameObject;
            
            GameObject found = FindChildByName(child.gameObject, name);
            if (found != null) return found;
        }
        
        return null;
    }
}