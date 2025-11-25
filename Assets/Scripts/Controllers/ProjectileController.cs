// ================================================== //
// Scripts/Weapons/ProjectileController.cs
// ================================================== //

using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public int damage = 10;
    public GameObject owner;
    public LayerMask enemyLayer;
    
    private bool hasHit = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Don't hit owner
        if (other.gameObject == owner) return;
        
        // Check if enemy
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damage);
                hasHit = true;
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Hit wall
        else if (!other.isTrigger)
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }
    
    private void SpawnHitEffect()
    {
        // Simple hit particle
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.3f;
        
        Destroy(effect.GetComponent<Collider>());
        Destroy(effect, 0.1f);
    }
}