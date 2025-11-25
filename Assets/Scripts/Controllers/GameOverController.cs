// ================================================== //
// Scripts/UI/GameOverUI.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Game over screen
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;
    public Button retryButton;
    public Button mainMenuButton;
    
    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }
    
    public void ShowGameOver(bool victory, int floorReached, int goldCollected, int enemiesKilled)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (titleText != null)
        {
            titleText.text = victory ? "VICTORY!" : "GAME OVER";
        }
        
        if (statsText != null)
        {
            statsText.text = $"Floor Reached: {floorReached}\n" +
                           $"Gold Collected: {goldCollected}\n" +
                           $"Enemies Defeated: {enemiesKilled}";
        }
    }
    
    private void OnRetry()
    {
        Time.timeScale = 1f;
        
        // Don't load save - restart fresh
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.NewGame();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}