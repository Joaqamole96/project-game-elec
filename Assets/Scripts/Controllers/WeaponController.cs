// ================================================== //
// Scripts/Managers/WeaponController.cs (WITH ANIMATION SUPPORT)
// ================================================== //

using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class WeaponController : MonoBehaviour
{
    [Header("Current Weapon")]
    public WeaponModel currentWeaponModel;
    public GameObject currentWeaponInstance;
    
    [Header("Weapon Holder")]
    public Transform weaponHolder;
    
    private PlayerController player;
    private Camera mainCamera;
    private float lastAttackTime = 0f;
    private Animator weaponAnimator; // Reference to weapon's animator
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        mainCamera = Camera.main;
        
        // Create weapon holder in front of camera view
        if (weaponHolder == null)
        {
            GameObject holder = new("WeaponHolder");
            holder.transform.SetParent(transform);
            // Position weapon in camera view
            holder.transform.localPosition = new Vector3(1f, -0.5f, 1f);
            weaponHolder = holder.transform;
        }
        
        // Start with default weapon
        WeaponModel starterWeapon = WeaponConfig.Instance.GetRandomWeapon();
        if (starterWeapon != null)
        {
            EquipWeapon(starterWeapon);
        }
    }
    
    void LateUpdate()
    {
        // Make weapon holder always face camera direction
        if (weaponHolder != null && mainCamera != null)
        {
            // weaponHolder.rotation = mainCamera.transform.rotation;
            // Get only the y rotation from the camera
            float yRotation = mainCamera.transform.eulerAngles.y;

            // Apply only the y rotation to the weapon holder
            // Add 180 because for some reason yRotation makes the weaponHolder face backward
            weaponHolder.rotation = Quaternion.Euler(0, yRotation + 180, 0);
        }
    }
    
    public void PickupWeapon(WeaponModel weaponData)
    {
        EquipWeapon(weaponData);
    }
    
    private void EquipWeapon(WeaponModel weaponData)
    {
        // Destroy old weapon visual
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }
        
        currentWeaponModel = weaponData;
        
        // Create visual if prefab exists
        if (weaponData.prefab != null)
        {
            currentWeaponInstance = Instantiate(weaponData.prefab, weaponHolder);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
            
            weaponAnimator = currentWeaponInstance.GetComponentInChildren<Animator>(true);
        
            if (weaponAnimator == null)
            {
                Debug.LogWarning($"Weapon prefab {weaponData.weaponName} has no Animator component in children!");
            }
        }
        
        // Update player stats
        if (player != null)
        {
            player.playerDamage = weaponData.damage;
            player.attackCooldown = weaponData.attackSpeed;
            player.attackRange = weaponData.range;
        }
        
        Debug.Log($"Equipped: {weaponData.weaponName} (Damage: {weaponData.damage}, Speed: {weaponData.attackSpeed})");
    }
    
    public void Attack(Vector3 attackPosition, Vector3 attackDirection)
    {
        if (currentWeaponModel == null) return;
        if (Time.time < lastAttackTime + currentWeaponModel.attackSpeed) return;
        
        lastAttackTime = Time.time;
        
        // Trigger weapon attack animation
        weaponAnimator.SetTrigger("Attack");
        
        // Perform attack based on weapon type
        switch (currentWeaponModel.weaponType)
        {
            case WeaponType.Melee:
                PerformMeleeAttack(attackPosition, attackDirection, currentWeaponModel.range, currentWeaponModel.damage);
                break;
                
            case WeaponType.Charge:
                PerformMeleeAttack(attackPosition, attackDirection, currentWeaponModel.range * 1.5f, currentWeaponModel.damage);
                break;
                
            case WeaponType.Ranged:
                PerformRangedAttack(attackDirection, currentWeaponModel.damage, currentWeaponModel.projectileSpeed);
                break;
                
            case WeaponType.Magic:
                PerformRangedAttack(attackDirection, currentWeaponModel.damage, currentWeaponModel.projectileSpeed);
                break;
        }
    }
    
    // ==========================================
    // MELEE ATTACK (Sword, Axe)
    // ==========================================
    
    private void PerformMeleeAttack(Vector3 position, Vector3 direction, float range, int damage)
    {
        Vector3 attackCenter = position + direction * (range * 0.5f);
        
        Collider[] hits = Physics.OverlapSphere(attackCenter, range, player.enemyLayer);
        
        foreach (Collider hit in hits)
        {
            // Check if enemy is in front of player (60 degree cone)
            Vector3 dirToTarget = (hit.transform.position - position).normalized;
            float angleToTarget = Vector3.Angle(direction, dirToTarget);
            
            if (angleToTarget <= 60f)
            {
                DealDamageToEnemy(hit.gameObject, damage);
            }
        }
        
        Debug.Log($"Melee attack: range={range}, damage={damage}");
    }
    
    // ==========================================
    // RANGED ATTACK (Bow, Staff)
    // ==========================================
    
    private void PerformRangedAttack(Vector3 direction, int damage, float speed)
    {
        // Create projectile
        GameObject projectile = ResourceService.LoadProjectilePrefab();
        
        // Setup physics
        if (!projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        rb.velocity = direction * speed;
        
        // Setup damage
        if (!projectile.TryGetComponent<ProjectileController>(out var projController))
        {
            projController = projectile.AddComponent<ProjectileController>();
        }
        projController.damage = damage;
        projController.owner = player.gameObject;
        projController.isPlayerProjectile = true;
        
        // Auto-destroy
        Destroy(projectile, 5f);
        
        Debug.Log($"Ranged attack: damage={damage}, speed={speed}");
    }
    
    // ==========================================
    // DAMAGE & EFFECTS
    // ==========================================
    
    private void DealDamageToEnemy(GameObject enemy, int damage)
    {
        if (enemy.TryGetComponent<EnemyController>(out var enemyController))
        {
            int finalDamage = damage;
            
            // Apply power modifiers
            if (player.powerManager != null) finalDamage = player.powerManager.ModifyDamageDealt(finalDamage);
            
            enemyController.TakeDamage(finalDamage);
            
            // Trigger effects
            if (player.powerManager != null) player.powerManager.OnDamageDealt(finalDamage);
            
            // Show damage number
            UIManager.Instance.ShowDamageDisplay(enemy.transform.position + Vector3.up, finalDamage, false, false);
            
            Debug.Log($"Hit {enemy.name} for {finalDamage} damage!");
        }
    }
}