// ================================================== //
// Scripts/Controllers/ProjectileController.cs (UNIFIED)
// ================================================== //

using UnityEngine;

/// <summary>
/// Unified projectile controller for all projectile types
/// Handles player projectiles, enemy projectiles, and magic spells
/// </summary>
public class ProjectileController : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    public GameObject owner; // Who shot this projectile
    
    [Header("Targeting")]
    public LayerMask targetLayer;
    public bool isPlayerProjectile = true; // If true, damages enemies. If false, damages player.
    
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    
    private bool hasHit = false;
    
    void Start()
    {
        // Auto-determine projectile type based on owner
        if (owner != null)
        {
            if (owner.CompareTag("Player"))
            {
                isPlayerProjectile = true;
                targetLayer = LayerMask.GetMask("Default"); // Enemies on Default layer
            }
            else if (owner.CompareTag("Enemy"))
            {
                isPlayerProjectile = false;
                targetLayer = LayerMask.GetMask("Default"); // Player on Default layer
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Don't hit owner
        if (other.gameObject == owner) return;
        
        // Don't hit triggers
        if (other.isTrigger) return;
        
        // Player projectile hitting enemy
        if (isPlayerProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Enemy projectile hitting player
        else if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Hit wall or obstacle
        else if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }
    
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        else
        {
            // Simple fallback effect
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector3.one * 0.3f;
            
            Renderer renderer = effect.GetComponent<Renderer>();
            Material mat = new(Shader.Find("Standard"))
            {
                color = Color.yellow
            };
            renderer.material = mat;
            
            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.1f);
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}