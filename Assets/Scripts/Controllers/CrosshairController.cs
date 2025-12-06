// ================================================== //
// CrosshairController - FIXED WITH DEBUG
// ================================================== //

using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image centerDot;
    public Image topLine;
    public Image bottomLine;
    public Image leftLine;
    public Image rightLine;
    
    [Header("Settings")]
    public float normalSpread = 10f;
    public float shootSpread = 20f;
    public float spreadTransitionSpeed = 10f;
    
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color enemyTargetColor = Color.red;
    public Color shootColor = Color.yellow;
    
    [Header("Debug")]
    public bool forceVisible = false;
    
    private float currentSpread;
    private PlayerController player;
    private WeaponController weaponController;
    private Camera mainCamera;
    
    void Start()
    {
        Debug.Log("CrosshairController: Start called");
        
        player = PlayerController.Instance;
        mainCamera = Camera.main;
        
        if (player != null)
        {
            weaponController = player.weaponManager;
            Debug.Log($"CrosshairController: Found player and weapon manager");
        }
        else
        {
            Debug.LogWarning("CrosshairController: Player not found at Start");
        }
        
        currentSpread = normalSpread;
        
        // Auto-find elements if not assigned
        if (centerDot == null || topLine == null || bottomLine == null || leftLine == null || rightLine == null)
        {
            Debug.Log("CrosshairController: Auto-finding UI elements...");
            AutoFindElements();
        }
        
        // Initial update
        UpdateCrosshairVisibility();
        
        Debug.Log($"CrosshairController: Initialized - CenterDot: {centerDot != null}, TopLine: {topLine != null}");
    }
    
    void Update()
    {
        if (player == null)
        {
            player = PlayerController.Instance;
        }
        
        if (weaponController == null && player != null)
        {
            weaponController = player.weaponManager;
        }
        
        if (forceVisible)
        {
            SetCrosshairVisible(true);
        }
        else
        {
            UpdateCrosshairVisibility();
        }
        
        UpdateCrosshairSpread();
        UpdateCrosshairColor();
    }
    
    private void AutoFindElements()
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        
        Debug.Log($"CrosshairController: Found {images.Length} images in children");
        
        foreach (Image img in images)
        {
            string name = img.gameObject.name.ToLower();
            
            if (name.Contains("center") || name.Contains("dot"))
            {
                centerDot = img;
                Debug.Log($"CrosshairController: Found centerDot: {img.gameObject.name}");
            }
            else if (name.Contains("top") || name.Contains("up"))
            {
                topLine = img;
                Debug.Log($"CrosshairController: Found topLine: {img.gameObject.name}");
            }
            else if (name.Contains("bottom") || name.Contains("down"))
            {
                bottomLine = img;
                Debug.Log($"CrosshairController: Found bottomLine: {img.gameObject.name}");
            }
            else if (name.Contains("left"))
            {
                leftLine = img;
                Debug.Log($"CrosshairController: Found leftLine: {img.gameObject.name}");
            }
            else if (name.Contains("right"))
            {
                rightLine = img;
                Debug.Log($"CrosshairController: Found rightLine: {img.gameObject.name}");
            }
        }
        
        // If still null, assign first 5 images found
        if (centerDot == null && images.Length > 0)
        {
            centerDot = images[0];
            Debug.Log("CrosshairController: Auto-assigned centerDot");
        }
        if (topLine == null && images.Length > 1)
        {
            topLine = images[1];
            Debug.Log("CrosshairController: Auto-assigned topLine");
        }
        if (bottomLine == null && images.Length > 2)
        {
            bottomLine = images[2];
            Debug.Log("CrosshairController: Auto-assigned bottomLine");
        }
        if (leftLine == null && images.Length > 3)
        {
            leftLine = images[3];
            Debug.Log("CrosshairController: Auto-assigned leftLine");
        }
        if (rightLine == null && images.Length > 4)
        {
            rightLine = images[4];
            Debug.Log("CrosshairController: Auto-assigned rightLine");
        }
    }
    
    // ------------------------- //
    // VISIBILITY
    // ------------------------- //
    
    private void UpdateCrosshairVisibility()
    {
        if (weaponController?.currentWeaponModel == null)
        {
            SetCrosshairVisible(false);
            return;
        }
        
        // Show crosshair for ranged weapons only
        bool isRangedWeapon = weaponController.currentWeaponModel.weaponType == WeaponType.Ranged ||
                             weaponController.currentWeaponModel.weaponType == WeaponType.Magic;
        
        SetCrosshairVisible(isRangedWeapon);
    }
    
    private void SetCrosshairVisible(bool visible)
    {
        if (centerDot != null) centerDot.enabled = visible;
        if (topLine != null) topLine.enabled = visible;
        if (bottomLine != null) bottomLine.enabled = visible;
        if (leftLine != null) leftLine.enabled = visible;
        if (rightLine != null) rightLine.enabled = visible;
    }
    
    // ------------------------- //
    // SPREAD ANIMATION
    // ------------------------- //
    
    private void UpdateCrosshairSpread()
    {
        float targetSpread = normalSpread;
        
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            targetSpread = shootSpread;
        }
        
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * spreadTransitionSpeed);
        
        if (topLine != null)
        {
            RectTransform rect = topLine.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, currentSpread);
        }
        
        if (bottomLine != null)
        {
            RectTransform rect = bottomLine.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -currentSpread);
        }
        
        if (leftLine != null)
        {
            RectTransform rect = leftLine.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-currentSpread, 0);
        }
        
        if (rightLine != null)
        {
            RectTransform rect = rightLine.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(currentSpread, 0);
        }
    }
    
    // ------------------------- //
    // COLOR FEEDBACK
    // ------------------------- //
    
    private void UpdateCrosshairColor()
    {
        Color targetColor = normalColor;
        
        if (IsAimingAtEnemy())
        {
            targetColor = enemyTargetColor;
        }
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            targetColor = shootColor;
        }
        
        SetCrosshairColor(targetColor);
    }
    
    private bool IsAimingAtEnemy()
    {
        if (mainCamera == null) return false;
        
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.CompareTag("Enemy");
        }
        
        return false;
    }
    
    private void SetCrosshairColor(Color color)
    {
        if (centerDot != null) centerDot.color = color;
        if (topLine != null) topLine.color = color;
        if (bottomLine != null) bottomLine.color = color;
        if (leftLine != null) leftLine.color = color;
        if (rightLine != null) rightLine.color = color;
    }
    
    // ------------------------- //
    // PUBLIC API
    // ------------------------- //
    
    public void Flash()
    {
        SetCrosshairColor(shootColor);
        Invoke(nameof(ResetColor), 0.1f);
    }
    
    private void ResetColor()
    {
        SetCrosshairColor(normalColor);
    }
    
    [ContextMenu("Test Visibility")]
    public void TestVisibility()
    {
        forceVisible = !forceVisible;
        Debug.Log($"CrosshairController: Force visible = {forceVisible}");
    }
}