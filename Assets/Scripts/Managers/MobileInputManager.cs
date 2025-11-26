// ================================================== //
// Scripts/Input/MobileInputManager.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mobile touch controls for player movement and actions
/// Automatically detects platform and shows/hides mobile controls
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("Mobile UI Elements")]
    public GameObject mobileControlsCanvas;
    public VirtualJoystick movementJoystick;
    public Button attackButton;
    public Button dashButton;
    public Button interactButton;
    
    [Header("Settings")]
    public bool forceMobileMode = false;
    
    private PlayerController player;
    private bool isMobilePlatform;
    
    void Start()
    {
        // Detect platform
        isMobilePlatform = Application.isMobilePlatform || forceMobileMode;
        
        // Show/hide mobile controls
        if (mobileControlsCanvas != null)
        {
            mobileControlsCanvas.SetActive(isMobilePlatform);
        }
        
        // Get player reference
        player = PlayerController.Instance;
        
        // Setup button listeners
        if (isMobilePlatform)
        {
            SetupMobileControls();
        }
    }
    
    void Update()
    {
        if (!isMobilePlatform || player == null) return;
        
        // Update player movement from joystick
        if (movementJoystick != null)
        {
            Vector2 input = movementJoystick.GetInputDirection();
            player.SetMovementInput(input);
        }
    }
    
    private void SetupMobileControls()
    {
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => player?.OnAttackButtonPressed());
        }
        
        if (dashButton != null)
        {
            dashButton.onClick.AddListener(() => {
                // Trigger dash if player has dash power
                if (player?.powerManager != null)
                {
                    // Dash logic handled in PowerManager
                }
            });
        }
        
        if (interactButton != null)
        {
            interactButton.onClick.AddListener(() => {
                // Trigger interact (open chests, talk to NPCs, etc.)
                Debug.Log("Interact button pressed");
            });
        }
        
        Debug.Log("Mobile controls initialized");
    }
}

// ================================================== //
// Scripts/Input/VirtualJoystick.cs
// ================================================== //

/// <summary>
/// Virtual joystick for mobile touch input
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Components")]
    public RectTransform background;
    public RectTransform handle;
    
    [Header("Settings")]
    public float handleRange = 50f;
    public float deadZone = 0.1f;
    
    private Vector2 inputDirection;
    private Vector2 joystickCenter;
    
    void Start()
    {
        joystickCenter = background.position;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - joystickCenter;
        
        // Clamp to handle range
        float magnitude = direction.magnitude;
        if (magnitude > handleRange)
        {
            direction = direction.normalized * handleRange;
        }
        
        // Update handle position
        handle.anchoredPosition = direction;
        
        // Calculate normalized input
        inputDirection = direction / handleRange;
        
        // Apply dead zone
        if (inputDirection.magnitude < deadZone)
        {
            inputDirection = Vector2.zero;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset joystick
        handle.anchoredPosition = Vector2.zero;
        inputDirection = Vector2.zero;
    }
    
    public Vector2 GetInputDirection()
    {
        return inputDirection;
    }
}