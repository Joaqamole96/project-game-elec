// -------------------------------------------------- //
// Scripts/Models/ProjectileModel.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// ProjectileModel fired by ranged weapons
/// </summary>
public class ProjectileModel : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayer;
    public GameObject hitEffectPrefab;
    
    private bool hasHit = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Check if hit valid target
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Hit wall or obstacle
        else if (!other.isTrigger)
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
    }
}