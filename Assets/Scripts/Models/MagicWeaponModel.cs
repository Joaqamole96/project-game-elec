// ================================================== //
// MAGIC WEAPON - Spell types
// ================================================== //

using UnityEngine;

public class MagicWeaponModel : WeaponModel
{
    [Header("Magic Settings")]
    public GameObject[] spellPrefabs; // 0:Fireball, 1:Lightning, 2:Ice
    public float projectileSpeed = 15f;
    public Transform firePoint;
    public int currentSpellType = 0;
    
    public override void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (!CanAttack()) return;
        
        isAttacking = true;
        
        if (animator != null)
        {
            animator.SetInteger("SpellType", currentSpellType);
            animator.SetTrigger("Attack");
        }
        
        RegisterAttack();
    }
    
    public void SetSpellType(int spellType)
    {
        currentSpellType = Mathf.Clamp(spellType, 0, spellPrefabs.Length - 1);
    }
    
    // Called by animation event
    public void OnSpawnProjectile()
    {
        if (spellPrefabs[currentSpellType] == null) return;
        
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject spell = Instantiate(spellPrefabs[currentSpellType], spawnPos, Quaternion.LookRotation(transform.forward));
        
        if (spell.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = transform.forward * projectileSpeed;
        }
        
        if (spell.TryGetComponent<ProjectileModel>(out var proj))
        {
            proj.damage = baseDamage;
            proj.targetLayer = targetLayer;
        }
        
        Destroy(spell, 5f);
    }
    
    // Called by animation event
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}