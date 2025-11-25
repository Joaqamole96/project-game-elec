// ================================================== //
// Scripts/Enemies/EnemyProjectileModel.cs
// ================================================== //

using UnityEngine;

public class EnemyProjectileModel : MonoBehaviour
{
    public int damage = 8;
    private bool hasHit = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamage(damage);
                hasHit = true;
                Destroy(gameObject);
            }
        }
        else if (!other.isTrigger && !other.CompareTag("Enemy"))
        {
            // Hit wall
            Destroy(gameObject);
        }
    }
}