// DamageDisplay.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class DamageDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 2f;
    public float lifetime = 1f;
    public float fadeSpeed = 1f;
    
    private Vector3 floatDirection = Vector3.up;
    private float spawnTime;
    private Color originalColor;
    
    public void Initialize(int damage, bool isCritical = false, bool isHeal = false)
    {
        if (text != null)
        {
            text.text = damage.ToString();
            
            if (isCritical)
            {
                text.color = Color.yellow;
                text.fontSize = 64;
                text.text += "!";
            }
            else if (isHeal)
            {
                text.color = Color.green;
                text.text = "+" + text.text;
            }
            else
            {
                text.color = Color.white;
            }
            
            originalColor = text.color;
        }
        
        spawnTime = Time.time;
        
        // Random horizontal offset
        floatDirection += new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
    }
    
    void Update()
    {
        // Float upward
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
        
        // Fade out
        float age = Time.time - spawnTime;
        if (age > lifetime - fadeSpeed)
        {
            float alpha = 1f - ((age - (lifetime - fadeSpeed)) / fadeSpeed);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        
        // Destroy after lifetime
        if (age > lifetime)
        {
            Destroy(gameObject);
        }
        
        // Face camera
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward);
    }
}