// ================================================== //
// Scripts/Configs/WeaponConfig.cs (SIMPLIFIED)
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple weapon registry - no complex systems, just data
/// </summary>
public class WeaponConfig : MonoBehaviour
{
    public static WeaponConfig Instance { get; private set; }
    
    private Dictionary<string, WeaponData> weaponRegistry;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            InitializeWeaponRegistry();
        }
        else Destroy(gameObject);
    }
    
    private void InitializeWeaponRegistry()
    {
        weaponRegistry = new Dictionary<string, WeaponData>
        {
            // MELEE - Fast swing, normal damage
            { "Sword", new WeaponData
            {
                weaponName = "Iron Sword",
                weaponType = WeaponType.Melee,
                damage = 15,
                attackSpeed = 1.0f,
                range = 2f,
                description = "A reliable balanced blade",
                prefab = ResourceService.LoadSwordPrefab(),
            }},
            
            // HEAVY - Slow swing, high damage, larger area
            { "Axe", new WeaponData
            {
                weaponName = "Battle Axe",
                weaponType = WeaponType.Charge,
                damage = 30,
                attackSpeed = 1.5f,
                range = 2.5f,
                description = "Heavy weapon with devastating power",
                prefab = ResourceService.LoadAxePrefab(),
            }},

            // RANGED - Projectile, medium damage
            { "Bow", new WeaponData
            {
                weaponName = "Hunter's Bow",
                weaponType = WeaponType.Ranged,
                damage = 20,
                attackSpeed = 1.2f,
                range = 15f,
                projectileSpeed = 20f,
                description = "Long range with precision",
                prefab = ResourceService.LoadBowPrefab(),
            }},
            
            // MAGIC - Projectile, high damage, slow speed
            { "Staff", new WeaponData
            {
                weaponName = "Fireball Staff",
                weaponType = WeaponType.Magic,
                damage = 25,
                attackSpeed = 1.8f,
                range = 12f,
                projectileSpeed = 15f,
                manaCost = 5,
                description = "Launches explosive fireballs",
                prefab = ResourceService.LoadStaffPrefab()
            }}
        };
        
        Debug.Log($"WeaponConfig: Initialized {weaponRegistry.Count} weapons");
    }
    
    public WeaponData GetWeaponData(string weaponKey)
    {
        if (weaponRegistry.TryGetValue(weaponKey, out WeaponData data)) return data;
        
        Debug.LogWarning($"Weapon '{weaponKey}' not found in database");
        return null;
    }
    
    public WeaponData GetRandomWeapon()
    {
        var keys = new List<string>(weaponRegistry.Keys);
        string randomKey = keys[Random.Range(0, keys.Count)];
        return GetWeaponData(randomKey);
    }
    
    public GameObject SpawnWeaponPickup(string weaponKey, Vector3 position)
    {
        WeaponData data = GetWeaponData(weaponKey);
        if (data == null) return null;
        
        GameObject pickup = new($"WeaponPickup_{data.weaponName}");
        pickup.transform.position = position;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(pickup.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        Renderer renderer = visual.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Standard"))
        {
            color = GetWeaponTypeColor(data.weaponType)
        };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", mat.color);
        renderer.material = mat;
        
        SphereCollider collider = pickup.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        WeaponController pickupComponent = pickup.AddComponent<WeaponController>();
        pickupComponent.weaponKey = weaponKey;
        
        return pickup;
    }
    
    private Color GetWeaponTypeColor(WeaponType type) => type switch
    {
        WeaponType.Melee => new Color(0.8f, 0.2f, 0.2f),
        WeaponType.Ranged => new Color(0.2f, 0.8f, 0.2f),
        WeaponType.Magic => new Color(0.5f, 0.2f, 1f),
        WeaponType.Charge => new Color(1f, 0.5f, 0f),
        _ => Color.white
    };
}