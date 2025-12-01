// ================================================== //
// Scripts/Managers/WeaponManager.cs (FIXED)
// ================================================== //

using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Current Weapon")]
    public WeaponData currentWeaponData;
    public GameObject currentWeaponInstance;
    
    [Header("Settings")]
    public Transform weaponHolder;
    private float lastAttackTime = 0f;
    
    private PlayerController player;
    private Camera mainCamera;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        mainCamera = Camera.main;
        
        // CRITICAL FIX: Create weapon holder in front of camera view
        if (weaponHolder == null)
        {
            GameObject holder = new("WeaponHolder");
            holder.transform.SetParent(transform);
            // Position weapon in front and slightly to the right (FPS view)
            holder.transform.SetLocalPositionAndRotation(new Vector3(0.8f, 0f, 0.6f), Quaternion.identity);
            weaponHolder = holder.transform;
        }
        
        // Start with default weapon
        WeaponData starterWeapon = WeaponConfig.Instance?.GetWeaponData("Sword");
        if (starterWeapon != null)
        {
            EquipWeapon(starterWeapon);
        }
    }
    
    void LateUpdate()
    {
        // CRITICAL: Make weapon holder face camera direction
        if (weaponHolder != null && mainCamera != null)
        {
            weaponHolder.rotation = mainCamera.transform.rotation;
        }
    }
    
    public void PickupWeapon(WeaponData weaponData)
    {
        EquipWeapon(weaponData);
    }
    
    private void EquipWeapon(WeaponData weaponData)
    {
        // Destroy old weapon visual
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }
        
        currentWeaponData = weaponData;
        
        // Create visual if prefab exists
        if (weaponData.prefab != null)
        {
            currentWeaponInstance = Instantiate(weaponData.prefab, weaponHolder);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
            
            // Setup weapon script if it has one
            WeaponModel weaponScript = currentWeaponInstance.GetComponent<WeaponModel>();
            if (weaponScript != null)
            {
                weaponScript.baseDamage = weaponData.damage;
                weaponScript.attackRange = weaponData.range;
                weaponScript.attackCooldown = weaponData.attackSpeed;
                weaponScript.Equip();
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
        if (currentWeaponData == null)
        {
            Debug.LogError("Current weapon data is null.");
            return;
        }
        if (Time.time < lastAttackTime + currentWeaponData.attackSpeed)
        {
            Debug.LogError($"Too early to attack, wait {lastAttackTime + currentWeaponData.attackSpeed - Time.time} more seconds.");
            return;
        }
        
        lastAttackTime = Time.time;

        // Trigger weapon animation if exists
        if (currentWeaponInstance != null)
        {
            WeaponModel weaponScript = currentWeaponInstance.GetComponent<WeaponModel>();
            Debug.Log($"weaponScript assigned as {weaponScript.GetType()}.");
            if (weaponScript != null)
            {
                Debug.Log($"Calling {weaponScript.GetType()}.Attack()...");
                weaponScript.Attack(attackPosition, attackDirection);
                return; // Let weapon handle its own attack
            }
            else
            {
                Debug.LogError($"WeaponModel is null.");
            }
            
            Animator weaponAnimator = currentWeaponInstance.GetComponentInChildren<Animator>();
            if (weaponAnimator != null)
            {
                weaponAnimator.SetTrigger("Attack");
            }
            else
            {
                Debug.LogError($"Animator is null.");
            }
        }
        
        // Fallback: Handle attack based on weapon type
        switch (currentWeaponData.weaponType)
        {
            case WeaponType.Melee:
            case WeaponType.Charge:
                PerformMeleeAttack(attackPosition, attackDirection);
                break;
                
            case WeaponType.Ranged:
            case WeaponType.Magic:
                PerformRangedAttack(attackPosition, attackDirection);
                break;
        }
    }
    
    private void PerformMeleeAttack(Vector3 position, Vector3 direction)
    {
        Vector3 attackCenter = position + direction * (currentWeaponData.range * 0.5f);
        
        Collider[] hits = Physics.OverlapSphere(attackCenter, currentWeaponData.range, player.enemyLayer);
        
        foreach (Collider hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - position).normalized;
            float angleToTarget = Vector3.Angle(direction, dirToTarget);
            
            if (angleToTarget <= 60f)
            {
                DealDamageToEnemy(hit.gameObject);
            }
        }
        
        // Visual effect
        SpawnMeleeEffect(attackCenter);
    }
    
    private void PerformRangedAttack(Vector3 position, Vector3 direction)
    {
        // Check if weapon has projectile prefab
        WeaponModel weaponScript = currentWeaponInstance?.GetComponent<WeaponModel>();
        GameObject projectilePrefab = null;
        
        if (weaponScript is RangedWeaponModel ranged)
        {
            projectilePrefab = ranged.projectilePrefab;
        }
        else if (weaponScript is MagicWeaponModel magic)
        {
            projectilePrefab = magic.spellPrefabs != null && magic.spellPrefabs.Length > 0 
                ? magic.spellPrefabs[magic.currentSpellType] 
                : null;
        }
        
        GameObject projectile;
        
        if (projectilePrefab != null)
        {
            // Use weapon's projectile
            projectile = Instantiate(projectilePrefab, position, Quaternion.LookRotation(direction));
        }
        else
        {
            // Create fallback projectile
            projectile = CreateFallbackProjectile(position, direction);
        }
        
        // Setup projectile physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        rb.velocity = direction * currentWeaponData.projectileSpeed;
        
        // Setup projectile damage
        ProjectileController projController = projectile.GetComponent<ProjectileController>();
        if (projController == null)
        {
            projController = projectile.AddComponent<ProjectileController>();
        }
        projController.damage = currentWeaponData.damage;
        projController.owner = player.gameObject;
        projController.targetLayer = player.enemyLayer;
        
        // Auto-destroy
        Destroy(projectile, 5f);
    }
    
    private GameObject CreateFallbackProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.3f;
        projectile.name = "Projectile";
        
        // Visual
        Renderer renderer = projectile.GetComponent<Renderer>();
        Color projectileColor = currentWeaponData.weaponType == WeaponType.Magic ? Color.magenta : Color.yellow;
        Material mat = new(Shader.Find("Standard"))
        {
            color = projectileColor
        };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", projectileColor * 2f);
        renderer.material = mat;
        
        return projectile;
    }
    
    private void SpawnMeleeEffect(Vector3 position)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = new Color(1f, 1f, 1f, 0.5f)
        };
        renderer.material = mat;
        
        Destroy(effect.GetComponent<Collider>());
        Destroy(effect, 0.2f);
    }
    
    private void DealDamageToEnemy(GameObject enemy)
    {
        if (enemy.TryGetComponent<EnemyController>(out var enemyController))
        {
            int finalDamage = currentWeaponData.damage;
            
            // Apply power modifiers
            if (player.powerManager != null)
            {
                finalDamage = player.powerManager.ModifyDamageDealt(finalDamage);
            }
            
            enemyController.TakeDamage(finalDamage);
            
            // Trigger effects
            if (player.powerManager != null)
            {
                player.powerManager.OnDamageDealt(finalDamage);
            }
            
            // Show damage number
            UIManager.Instance?.ShowDamageDisplay(enemy.transform.position + Vector3.up, finalDamage, false, false);
        }
    }
}