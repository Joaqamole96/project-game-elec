// ================================================== //
// Scripts/Controllers/ProjectileController.cs (FIXED)
// ================================================== //

using UnityEngine;

/// <summary>
/// Unified projectile controller for all projectile types
/// Handles player projectiles, enemy projectiles, and magic spells
/// Now with proper collision detection for walls and obstacles
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
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
    
    [Header("Lifetime")]
    public float maxLifetime = 5f;
    
    private bool hasHit = false;
    private Rigidbody rb;
    private float spawnTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        
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
        
        // Ensure proper physics setup
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        Collider col = GetComponent<Collider>();
        col.isTrigger = true; // Must be trigger for OnTriggerEnter
    }
    
    void Update()
    {
        // Destroy after max lifetime
        if (Time.time - spawnTime > maxLifetime)
        {
            DestroyProjectile();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Don't hit owner
        if (other.gameObject == owner) return;
        
        // Don't hit other triggers (like room boundaries)
        if (other.isTrigger) return;
        
        // Player projectile hitting enemy
        if (isPlayerProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect(other.ClosestPoint(transform.position));
                DestroyProjectile();
            }
        }
        // Enemy projectile hitting player
        else if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect(other.ClosestPoint(transform.position));
                DestroyProjectile();
            }
        }
        // Hit wall, obstacle, or any other non-target collider
        else if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            hasHit = true;
            SpawnHitEffect(other.ClosestPoint(transform.position));
            DestroyProjectile();
        }
    }
    
    private void SpawnHitEffect(Vector3 hitPosition)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }
        else
        {
            // Simple fallback effect
            CreateFallbackHitEffect(hitPosition);
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hitPosition);
        }
    }
    
    private void CreateFallbackHitEffect(Vector3 position)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = isPlayerProjectile ? Color.yellow : Color.red;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", mat.color * 2f);
        renderer.material = mat;
        
        Destroy(effect.GetComponent<Collider>());
        Destroy(effect, 0.2f);
    }
    
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        // Visualize projectile trajectory
        Gizmos.color = isPlayerProjectile ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        if (rb != null)
        {
            Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2f);
        }
    }
}