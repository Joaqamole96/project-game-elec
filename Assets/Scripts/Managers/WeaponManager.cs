// ================================================== //
// Scripts/Managers/WeaponManager.cs (ENHANCED)
// ================================================== //

using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Current Weapon")]
    public WeaponData currentWeaponData;
    public GameObject currentWeaponInstance;
    
    [Header("Settings")]
    public Transform weaponHolder; // Child transform where weapon visual appears
    private float lastAttackTime = 0f;
    
    private PlayerController player;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        
        // Create weapon holder if doesn't exist
        if (weaponHolder == null)
        {
            GameObject holder = new GameObject("WeaponHolder");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = new Vector3(0.5f, 1f, 0.5f);
            weaponHolder = holder.transform;
        }
        
        // Start with default sword
        WeaponData starterWeapon = WeaponConfig.Instance?.GetWeaponData("Sword");
        if (starterWeapon != null)
        {
            EquipWeapon(starterWeapon);
        }
    }
    
    void Update()
    {
        // Weapon switching with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentWeaponData != null)
        {
            // Could switch to saved weapon slot 1
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
        
        switch (currentWeaponData.weaponType)
        {
            case WeaponType.Melee:
                PerformMeleeAttack(attackPosition, attackDirection);
                break;
                
            case WeaponType.Ranged:
                PerformRangedAttack(attackPosition, attackDirection);
                break;
                
            case WeaponType.Magic:
                PerformMagicAttack(attackPosition, attackDirection);
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
        GameObject projectile = CreateProjectile(position, direction, ProjectileType.Arrow);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeaponData.projectileSpeed;
        }
    }
    
    private void PerformMagicAttack(Vector3 position, Vector3 direction)
    {
        // Check mana (if implemented)
        // For now, just spawn magic projectile
        
        GameObject projectile = CreateProjectile(position, direction, ProjectileType.Magic);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeaponData.projectileSpeed;
        }
    }
    
    private GameObject CreateProjectile(Vector3 position, Vector3 direction, ProjectileType type)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.3f;
        projectile.name = $"Projectile_{type}";
        
        // Setup physics
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Visual
        Renderer renderer = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = type == ProjectileType.Magic ? Color.magenta : Color.yellow;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", mat.color * 2f);
        renderer.material = mat;
        
        // Add projectile script
        ProjectileController projController = projectile.AddComponent<ProjectileController>();
        projController.damage = currentWeaponData.damage;
        projController.owner = player.gameObject;
        projController.enemyLayer = player.enemyLayer;
        
        // Auto-destroy after 5 seconds
        Destroy(projectile, 5f);
        
        return projectile;
    }
    
    private void SpawnMeleeEffect(Vector3 position)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 1f, 1f, 0.5f);
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
        }
    }
    
    private enum ProjectileType { Arrow, Magic }
}