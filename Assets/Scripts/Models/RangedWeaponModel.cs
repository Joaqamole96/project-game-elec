// ================================================== //
// RANGED WEAPON - Draw and release
// ================================================== //

using UnityEngine;

public class RangedWeaponModel : WeaponModel
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public Transform firePoint;
    
    private bool isDrawing = false;
    private float drawStartTime = 0f;
    private float drawAmount = 0f;
    private float drawTime = 0.5f;
    
    void Update()
    {
        if (isDrawing)
        {
            drawAmount += Time.deltaTime * 2f; // 0.5s to full draw
            
            if (animator != null)
            {
                animator.SetFloat("DrawAmount", drawAmount);
            }
        }
    }
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack) return;
        
        // Start drawing
        isDrawing = true;
        drawStartTime = Time.time;
        
        if (animator != null)
        {
            animator.SetBool("IsDrawing", true);
            animator.SetTrigger("Attack");
        }
    }
    
    public void ReleaseAttack()
    {
        if (!isDrawing) return;
        
        isDrawing = false;
        
        if (animator != null)
        {
            animator.SetBool("IsDrawing", false);
        }
        
        RegisterAttack();
    }
    
    // Called by animation event
    public void OnSpawnProjectile()
    {
        if (projectilePrefab == null) return;
        
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(transform.forward));
        
        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = transform.forward * projectileSpeed * (0.5f + drawAmount * 0.5f);
        }
        
        if (projectile.TryGetComponent<ProjectileModel>(out var proj))
        {
            proj.damage = Mathf.RoundToInt(baseDamage * (0.5f + drawAmount * 0.5f));
            proj.targetLayer = targetLayer;
        }
        
        Destroy(projectile, 5f);
        drawAmount = 0f;
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}