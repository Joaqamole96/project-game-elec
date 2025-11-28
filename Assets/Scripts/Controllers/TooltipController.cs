// TooltipManager.cs (singleton)
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }
    
    public GameObject tooltipPrefab;
    public Canvas canvas;
    
    private GameObject currentTooltip;
    private TextMeshProUGUI tooltipText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowTooltip(string text, Vector2 screenPosition)
    {
        if (currentTooltip == null && tooltipPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPrefab, canvas.transform);
            tooltipText = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (tooltipText != null)
        {
            tooltipText.text = text;
        }
        
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(true);
            currentTooltip.transform.position = screenPosition;
        }
    }
    
    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }
    }
}
// ```

// ---

// ## 📦 ProBuilder Layout Prefabs

// Since you want to use ProBuilder instead of primitives, here's the setup:

// ### **Floor Tile (1x1x1)**
// - ProBuilder Cube
// - Scale: (1, 1, 1)
// - UV unwrapping: Box projected
// - Material slots: 1 (top surface)
// - Collider: Box Collider (non-trigger)

// ### **Wall Segment (1x8x1)**
// - ProBuilder Cube
// - Scale: (1, 8, 1)
// - Position: (0, 4, 0) - centered at height
// - UV unwrapping: Box projected
// - Material slots: 1
// - Collider: Box Collider

// ### **Doorway (3-unit span)**
// ```
// DoorwayPrefab
// ├── LeftCorner (ProBuilder - 0.5x8x1)
// ├── DoorFrame (ProBuilder - 2x8x0.2)
// │   └── DoorController
// └── RightCorner (ProBuilder - 0.5x8x1)