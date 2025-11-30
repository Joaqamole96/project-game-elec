// -------------------------------------------------- //
// Scripts/Displays/DamageDisplay.cs (FIXED)
// -------------------------------------------------- //

using UnityEngine;
using TMPro;

public class DamageDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textComponent;
    
    [Header("Settings")]
    public float floatSpeed = 2f;
    public float lifetime = 1f;
    public float fadeSpeed = 1f;
    
    private Vector3 floatDirection = Vector3.up;
    private float spawnTime;
    private Color originalColor;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Auto-find text component if not assigned
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (textComponent == null)
        {
            Debug.LogError("DamageDisplay: No TextMeshProUGUI component found!");
            Destroy(gameObject);
            return;
        }
        
        originalColor = textComponent.color;
        spawnTime = Time.time;
        
        // Random horizontal offset
        floatDirection += new Vector3(
            Random.Range(-0.5f, 0.5f), 
            0, 
            Random.Range(-0.5f, 0.5f)
        );
    }
    
    public void Initialize(int damage, bool isCritical = false, bool isHeal = false)
    {
        // Ensure we have text component
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (textComponent != null)
        {
            textComponent.text = damage.ToString();
            
            if (isCritical)
            {
                textComponent.color = Color.yellow;
                textComponent.fontSize = 48;
                textComponent.text += "!";
            }
            else if (isHeal)
            {
                textComponent.color = Color.green;
                textComponent.text = "+" + textComponent.text;
            }
            else
            {
                textComponent.color = Color.white;
            }
            
            originalColor = textComponent.color;
        }
        
        spawnTime = Time.time;
        
        // Random horizontal offset
        floatDirection = Vector3.up + new Vector3(
            Random.Range(-0.5f, 0.5f), 
            0, 
            0
        );
    }
    
    void Update()
    {
        if (textComponent == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Float upward
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
        
        // Calculate age
        float age = Time.time - spawnTime;
        
        // Fade out
        if (age > lifetime - fadeSpeed)
        {
            float alpha = 1f - ((age - (lifetime - fadeSpeed)) / fadeSpeed);
            alpha = Mathf.Clamp01(alpha);
            textComponent.color = new Color(
                originalColor.r, 
                originalColor.g, 
                originalColor.b, 
                alpha
            );
        }
        
        // Destroy after lifetime
        if (age > lifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        // Face camera (billboard effect)
        if (mainCamera != null)
        {
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }
}