// TooltipController.cs (singleton)
using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }
    
    public GameObject tooltipPrefab;
    public Canvas canvas;
    
    private GameObject currentTooltip;
    private TextMeshProUGUI tooltipText;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void ShowTooltip(string text, Vector2 screenPosition)
    {
        if (currentTooltip == null && tooltipPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPrefab, canvas.transform);
            tooltipText = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (tooltipText != null) tooltipText.text = text;
        
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(true);
            currentTooltip.transform.position = screenPosition;
        }
    }
    
    public void HideTooltip()
    {
        if (currentTooltip != null) currentTooltip.SetActive(false);
    }
}