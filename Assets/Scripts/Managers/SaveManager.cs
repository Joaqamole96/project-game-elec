// ================================================== //
// Scripts/Systems/SaveManager.cs - Complete Save/Load
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles all game save/load operations using PlayerPrefs
/// Manages player progress, inventory, powers, and settings
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [Header("Save Settings")]
    public bool autoSave = true;
    public float autoSaveInterval = 60f; // Auto-save every 60 seconds
    
    private float autoSaveTimer = 0f;
    
    // Save keys
    private const string SAVE_VERSION = "v1.0";
    private const string KEY_SAVE_EXISTS = "SaveExists";
    private const string KEY_CURRENT_FLOOR = "CurrentFloor";
    private const string KEY_PLAYER_HEALTH = "PlayerHealth";
    private const string KEY_PLAYER_MAX_HEALTH = "PlayerMaxHealth";
    private const string KEY_PLAYER_GOLD = "PlayerGold";
    private const string KEY_PLAYER_POWERS = "PlayerPowers";
    private const string KEY_CURRENT_WEAPON = "CurrentWeapon";
    private const string KEY_SETTINGS_MASTER_VOLUME = "MasterVolume";
    private const string KEY_SETTINGS_MUSIC_VOLUME = "MusicVolume";
    private const string KEY_SETTINGS_SFX_VOLUME = "SFXVolume";
    
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
        }
    }
    
    void Update()
    {
        if (autoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame();
            }
        }
    }
    
    // ------------------------- //
    // SAVE GAME
    // ------------------------- //
    
    public void SaveGame()
    {
        Debug.Log("SaveManager: Saving game...");
        
        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogWarning("SaveManager: Cannot save - player not found");
            return;
        }
        
        // Mark that save exists
        PlayerPrefs.SetInt(KEY_SAVE_EXISTS, 1);
        
        // Save player stats
        SavePlayerData(player);
        
        // Save inventory
        SaveInventoryData(player.inventory);
        
        // Save powers
        SavePowerData(player.powerManager);
        
        // Save current weapon
        SaveWeaponData(player.weaponManager);
        
        // Save floor progress
        SaveFloorProgress();
        
        // Commit to disk
        PlayerPrefs.Save();
        
        Debug.Log($"SaveManager: Game saved successfully (Floor {GetCurrentFloor()})");
    }
    
    private void SavePlayerData(PlayerController player)
    {
        PlayerPrefs.SetInt(KEY_PLAYER_HEALTH, player.CurrentHealth);
        PlayerPrefs.SetInt(KEY_PLAYER_MAX_HEALTH, player.maxHealth);
    }
    
    private void SaveInventoryData(InventoryManager inventory)
    {
        if (inventory == null) return;
        
        PlayerPrefs.SetInt(KEY_PLAYER_GOLD, inventory.gold);
        
        // Save keys (as comma-separated string)
        string keysString = string.Join(",", inventory.keys);
        PlayerPrefs.SetString("PlayerKeys", keysString);
    }
    
    private void SavePowerData(PowerManager powerManager)
    {
        if (powerManager == null || powerManager.activePowers == null) return;
        
        // Save powers as comma-separated list of enum values
        List<string> powerStrings = new List<string>();
        foreach (var power in powerManager.activePowers)
        {
            powerStrings.Add(((int)power.type).ToString());
        }
        
        string powersString = string.Join(",", powerStrings);
        PlayerPrefs.SetString(KEY_PLAYER_POWERS, powersString);
        
        Debug.Log($"SaveManager: Saved {powerManager.activePowers.Count} powers");
    }
    
    private void SaveWeaponData(WeaponManager weaponManager)
    {
        if (weaponManager == null || weaponManager.currentWeaponData == null) return;
        
        PlayerPrefs.SetString(KEY_CURRENT_WEAPON, weaponManager.currentWeaponData.weaponName);
    }
    
    private void SaveFloorProgress()
    {
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null && layoutManager.LevelConfig != null)
        {
            PlayerPrefs.SetInt(KEY_CURRENT_FLOOR, layoutManager.LevelConfig.FloorLevel);
        }
    }
    
    // ------------------------- //
    // LOAD GAME
    // ------------------------- //
    
    public bool HasSaveData()
    {
        return PlayerPrefs.GetInt(KEY_SAVE_EXISTS, 0) == 1;
    }
    
    public void LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.LogWarning("SaveManager: No save data found");
            return;
        }
        
        Debug.Log("SaveManager: Loading game...");
        
        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogWarning("SaveManager: Cannot load - player not found");
            return;
        }
        
        // Load player stats
        LoadPlayerData(player);
        
        // Load inventory
        LoadInventoryData(player.inventory);
        
        // Load powers
        LoadPowerData(player.powerManager);
        
        // Load weapon
        LoadWeaponData(player.weaponManager);
        
        // Load floor progress
        LoadFloorProgress();
        
        Debug.Log($"SaveManager: Game loaded successfully (Floor {GetCurrentFloor()})");
    }
    
    private void LoadPlayerData(PlayerController player)
    {
        int savedHealth = PlayerPrefs.GetInt(KEY_PLAYER_HEALTH, player.maxHealth);
        int savedMaxHealth = PlayerPrefs.GetInt(KEY_PLAYER_MAX_HEALTH, player.maxHealth);
        
        player.maxHealth = savedMaxHealth;
        // Heal to saved health
        player.Heal(savedHealth - player.CurrentHealth);
        
        Debug.Log($"SaveManager: Loaded player health {player.CurrentHealth}/{player.maxHealth}");
    }
    
    private void LoadInventoryData(InventoryManager inventory)
    {
        if (inventory == null) return;
        
        inventory.gold = PlayerPrefs.GetInt(KEY_PLAYER_GOLD, 0);
        
        // Load keys
        string keysString = PlayerPrefs.GetString("PlayerKeys", "");
        inventory.keys.Clear();
        
        if (!string.IsNullOrEmpty(keysString))
        {
            string[] keyStrings = keysString.Split(',');
            foreach (string keyStr in keyStrings)
            {
                if (System.Enum.TryParse(keyStr, out KeyType keyType))
                {
                    inventory.keys.Add(keyType);
                }
            }
        }
        
        Debug.Log($"SaveManager: Loaded {inventory.gold} gold, {inventory.keys.Count} keys");
    }
    
    private void LoadPowerData(PowerManager powerManager)
    {
        if (powerManager == null) return;
        
        string powersString = PlayerPrefs.GetString(KEY_PLAYER_POWERS, "");
        powerManager.activePowers.Clear();
        
        if (!string.IsNullOrEmpty(powersString))
        {
            string[] powerStrings = powersString.Split(',');
            foreach (string powerStr in powerStrings)
            {
                if (int.TryParse(powerStr, out int powerInt))
                {
                    PowerType powerType = (PowerType)powerInt;
                    powerManager.AddPower(powerType);
                }
            }
        }
        
        Debug.Log($"SaveManager: Loaded {powerManager.activePowers.Count} powers");
    }
    
    private void LoadWeaponData(WeaponManager weaponManager)
    {
        if (weaponManager == null) return;
        
        string weaponName = PlayerPrefs.GetString(KEY_CURRENT_WEAPON, "Sword");
        
        // Find weapon in database
        if (WeaponConfig.Instance != null)
        {
            // Try to find by name (you may need to add a GetWeaponByName method)
            WeaponData weaponData = WeaponConfig.Instance.GetWeaponData(weaponName);
            if (weaponData != null)
            {
                weaponManager.PickupWeapon(weaponData);
                Debug.Log($"SaveManager: Loaded weapon {weaponName}");
            }
        }
    }
    
    private void LoadFloorProgress()
    {
        int savedFloor = PlayerPrefs.GetInt(KEY_CURRENT_FLOOR, 1);
        
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null && layoutManager.LevelConfig != null)
        {
            layoutManager.LevelConfig.FloorLevel = savedFloor;
            Debug.Log($"SaveManager: Loaded floor {savedFloor}");
        }
    }
    
    // ------------------------- //
    // SETTINGS
    // ------------------------- //
    
    public void SaveSettings(float masterVolume, float musicVolume, float sfxVolume)
    {
        PlayerPrefs.SetFloat(KEY_SETTINGS_MASTER_VOLUME, masterVolume);
        PlayerPrefs.SetFloat(KEY_SETTINGS_MUSIC_VOLUME, musicVolume);
        PlayerPrefs.SetFloat(KEY_SETTINGS_SFX_VOLUME, sfxVolume);
        PlayerPrefs.Save();
        
        Debug.Log("SaveManager: Settings saved");
    }
    
    public (float master, float music, float sfx) LoadSettings()
    {
        float master = PlayerPrefs.GetFloat(KEY_SETTINGS_MASTER_VOLUME, 1f);
        float music = PlayerPrefs.GetFloat(KEY_SETTINGS_MUSIC_VOLUME, 0.7f);
        float sfx = PlayerPrefs.GetFloat(KEY_SETTINGS_SFX_VOLUME, 1f);
        
        return (master, music, sfx);
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    public void DeleteSave()
    {
        Debug.Log("SaveManager: Deleting save data...");
        
        PlayerPrefs.DeleteKey(KEY_SAVE_EXISTS);
        PlayerPrefs.DeleteKey(KEY_CURRENT_FLOOR);
        PlayerPrefs.DeleteKey(KEY_PLAYER_HEALTH);
        PlayerPrefs.DeleteKey(KEY_PLAYER_MAX_HEALTH);
        PlayerPrefs.DeleteKey(KEY_PLAYER_GOLD);
        PlayerPrefs.DeleteKey(KEY_PLAYER_POWERS);
        PlayerPrefs.DeleteKey(KEY_CURRENT_WEAPON);
        PlayerPrefs.DeleteKey("PlayerKeys");
        
        PlayerPrefs.Save();
        
        Debug.Log("SaveManager: Save data deleted");
    }
    
    public void NewGame()
    {
        DeleteSave();
        
        // Reset player to defaults
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            player.maxHealth = 100;
            player.Heal(100);
            
            if (player.inventory != null)
            {
                player.inventory.ClearInventory();
            }
            
            if (player.powerManager != null)
            {
                player.powerManager.ClearAllPowers();
            }
        }
        
        // Reset floor to 1
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null && layoutManager.LevelConfig != null)
        {
            layoutManager.LevelConfig.FloorLevel = 1;
        }
        
        Debug.Log("SaveManager: New game started");
    }
    
    public int GetCurrentFloor()
    {
        return PlayerPrefs.GetInt(KEY_CURRENT_FLOOR, 1);
    }
    
    [ContextMenu("Print Save Data")]
    public void PrintSaveData()
    {
        Debug.Log("=== SAVE DATA ===");
        Debug.Log($"Save Exists: {HasSaveData()}");
        Debug.Log($"Current Floor: {GetCurrentFloor()}");
        Debug.Log($"Player Health: {PlayerPrefs.GetInt(KEY_PLAYER_HEALTH, 0)}");
        Debug.Log($"Player Gold: {PlayerPrefs.GetInt(KEY_PLAYER_GOLD, 0)}");
        Debug.Log($"Powers: {PlayerPrefs.GetString(KEY_PLAYER_POWERS, "None")}");
        Debug.Log($"Current Weapon: {PlayerPrefs.GetString(KEY_CURRENT_WEAPON, "None")}");
        Debug.Log("=================");
    }
    
    // ------------------------- //
    // AUTO-SAVE ON IMPORTANT EVENTS
    // ------------------------- //
    
    public void OnFloorCompleted()
    {
        SaveGame();
        Debug.Log("SaveManager: Auto-saved on floor completion");
    }
    
    public void OnPowerAcquired()
    {
        SaveGame();
        Debug.Log("SaveManager: Auto-saved on power acquisition");
    }
    
    public void OnWeaponAcquired()
    {
        SaveGame();
        Debug.Log("SaveManager: Auto-saved on weapon acquisition");
    }
}



// ================================================== //
// Enhanced GameDirector with SaveManager (ADD TO EXISTING)
// ================================================== //

/*
Add to GameDirector.cs:

[Header("Manager References (Auto-Created)")]
public SaveManager saveManager; // ADD THIS

In InitializeGameSystems(), add:
InitializeSaveManager();
yield return new WaitForSeconds(initializationDelay);

Add this method:
private void InitializeSaveManager()
{
    Debug.Log("GameDirector: Initializing SaveManager...");
    
    if (saveManager != null)
    {
        Debug.Log("GameDirector: SaveManager already exists");
        return;
    }
    
    GameObject saveObj = new("SaveManager");
    saveObj.transform.SetParent(systemsContainer.transform);
    saveManager = saveObj.AddComponent<SaveManager>();
    
    if (saveManager != null)
    {
        Debug.Log("GameDirector: SaveManager initialized");
        
        // Load game if save exists
        if (saveManager.HasSaveData())
        {
            Debug.Log("GameDirector: Save data found, loading...");
            // Don't load immediately, let player choose in main menu
        }
    }
    else
    {
        Debug.LogError("GameDirector: Failed to initialize SaveManager!");
    }
}
*/