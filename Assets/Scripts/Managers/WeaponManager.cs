// ================================================== //
// Scripts/Managers/WeaponManager.cs (WITH ANIMATION SUPPORT)
// ================================================== //

using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Current Weapon")]
    public WeaponData currentWeaponData;
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
        WeaponData starterWeapon = WeaponConfig.Instance?.GetRandomWeapon();
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
            
            weaponAnimator = currentWeaponInstance.GetComponentInChildren<Animator>(true);
        
            if (weaponAnimator == null)
            {
                Debug.LogWarning($"Weapon prefab {weaponData.weaponName} has no Animator component in children!");
            }
            else
            {
                Debug.LogWarning($"Found Animator on {weaponAnimator.gameObject.name}");
                
                // Optional: Log animator parameters for debugging
                foreach (AnimatorControllerParameter param in weaponAnimator.parameters)
                {
                    Debug.LogWarning($"Animator Parameter: {param.name} (Type: {param.type})");
                }
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
        if (currentWeaponData == null) return;
        if (Time.time < lastAttackTime + currentWeaponData.attackSpeed) return;
        
        lastAttackTime = Time.time;
        
        // Trigger weapon attack animation
        TriggerAttackAnimation();
        
        // Simple weapon swing animation (optional visual feedback - keep as backup)
        if (currentWeaponInstance != null && weaponAnimator == null)
        {
            Debug.Log("Stupid ass swing incoming..");
            StartCoroutine(SimpleWeaponSwing());
        }
        
        // Perform attack based on weapon type
        switch (currentWeaponData.weaponType)
        {
            case WeaponType.Melee:
                PerformMeleeAttack(attackPosition, attackDirection, currentWeaponData.range, currentWeaponData.damage);
                break;
                
            case WeaponType.Charge:
                PerformMeleeAttack(attackPosition, attackDirection, currentWeaponData.range * 1.5f, currentWeaponData.damage);
                break;
                
            case WeaponType.Ranged:
                PerformRangedAttack(attackPosition, attackDirection, currentWeaponData.damage, currentWeaponData.projectileSpeed);
                break;
                
            case WeaponType.Magic:
                PerformRangedAttack(attackPosition, attackDirection, currentWeaponData.damage, currentWeaponData.projectileSpeed);
                break;
        }
    }
    
    // ==========================================
    // ANIMATION METHODS
    // ==========================================
    
    private void TriggerAttackAnimation()
    {
        if (weaponAnimator != null && weaponAnimator.isActiveAndEnabled)
        {
            // Trigger the "Attack" parameter in the animator
            // This assumes you have a trigger parameter named "Attack" in your animator
            Debug.LogWarning("Triggering attack...");
            weaponAnimator.SetTrigger("Attack");
            Debug.LogWarning("Triggered attack...");
            
            // Alternative: If using boolean parameter
            // weaponAnimator.SetBool("IsAttacking", true);
            // StartCoroutine(ResetAttackAnimation());
            
            Debug.Log("Attack animation triggered!");
        }
        else if (weaponAnimator == null && currentWeaponInstance != null)
        {
            // Try to find animator again (in case it was added after instantiation)
            weaponAnimator = currentWeaponInstance.GetComponentInChildren<Animator>();
            if (weaponAnimator != null)
            {
                weaponAnimator.SetTrigger("Attack");
            }
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
        
        // Visual feedback
        TriggerAttackAnimation();
        
        // SpawnMeleeEffect(attackCenter);
        
        Debug.Log($"Melee attack: range={range}, damage={damage}");
    }
    
    // ==========================================
    // RANGED ATTACK (Bow, Staff)
    // ==========================================
    
    private void PerformRangedAttack(Vector3 position, Vector3 direction, int damage, float speed)
    {
        // Create projectile
        GameObject projectile = CreateProjectile(position, direction);
        
        // Setup physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        rb.velocity = direction * speed;
        
        // Setup damage
        ProjectileController projController = projectile.GetComponent<ProjectileController>();
        if (projController == null)
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
            
            Debug.Log($"Hit {enemy.name} for {finalDamage} damage!");
        }
    }
    
    private GameObject CreateProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.3f;
        projectile.name = "Projectile";
        
        // Visual based on weapon type
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
    
    private System.Collections.IEnumerator SimpleWeaponSwing()
    {
        if (currentWeaponInstance == null) yield break;
        
        // Simple forward swing animation (used only if no animator is present)
        Vector3 originalPos = currentWeaponInstance.transform.localPosition;
        Vector3 swingPos = originalPos + new Vector3(0.2f, -0.1f, 0.3f);
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        // Swing forward
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentWeaponInstance.transform.localPosition = Vector3.Lerp(originalPos, swingPos, elapsed / duration);
            yield return null;
        }
        
        // Return to original position
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentWeaponInstance.transform.localPosition = Vector3.Lerp(swingPos, originalPos, elapsed / duration);
            yield return null;
        }
        
        currentWeaponInstance.transform.localPosition = originalPos;
    }
}