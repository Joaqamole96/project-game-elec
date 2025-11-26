// ================================================== //
// Scripts/Configs/WeaponConfig.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

public class WeaponConfig : MonoBehaviour
{
    public static WeaponConfig Instance { get; private set; }
    public GameObject swordPrefab;
    public GameObject axePrefab;
    public GameObject daggerPrefab;
    public GameObject bowPrefab;
    public GameObject crossbowPrefab;
    public GameObject staffPrefab;
    public GameObject wandPrefab;
    
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
            { "Sword", new WeaponData
            {
                weaponName = "Iron Sword",
                weaponType = WeaponType.Melee,
                damage = 15,
                attackSpeed = 1.0f,
                range = 2f,
                description = "A reliable balanced blade",
                prefab = swordPrefab
            }},
            
            { "Axe", new WeaponData
            {
                weaponName = "Battle Axe",
                weaponType = WeaponType.Melee,
                damage = 25,
                attackSpeed = 1.5f,
                range = 2.5f,
                description = "Heavy weapon with devastating power",
                prefab = axePrefab
            }},
            
            { "Dagger", new WeaponData
            {
                weaponName = "Swift Dagger",
                weaponType = WeaponType.Melee,
                damage = 10,
                attackSpeed = 0.5f,
                range = 1.5f,
                description = "Fast strikes with lower damage",
                prefab = daggerPrefab
            }},

            { "Bow", new WeaponData
            {
                weaponName = "Hunter's Bow",
                weaponType = WeaponType.Ranged,
                damage = 20,
                attackSpeed = 1.2f,
                range = 15f,
                projectileSpeed = 20f,
                description = "Long range with precision",
                prefab = bowPrefab
            }},
            
            { "Crossbow", new WeaponData
            {
                weaponName = "Heavy Crossbow",
                weaponType = WeaponType.Ranged,
                damage = 30,
                attackSpeed = 2.0f,
                range = 20f,
                projectileSpeed = 30f,
                description = "Powerful but slow",
                prefab = crossbowPrefab
            }},
            
            { "Staff", new WeaponData
            {
                weaponName = "Fireball Staff",
                weaponType = WeaponType.Magic,
                damage = 18,
                attackSpeed = 1.5f,
                range = 12f,
                projectileSpeed = 15f,
                manaCost = 5,
                description = "Launches explosive fireballs",
                prefab = staffPrefab
            }},
            
            { "Wand", new WeaponData
            {
                weaponName = "Lightning Wand",
                weaponType = WeaponType.Magic,
                damage = 12,
                attackSpeed = 0.8f,
                range = 10f,
                projectileSpeed = 25f,
                manaCost = 3,
                description = "Rapid magical bolts",
                prefab = wandPrefab
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
    
    public WeaponData GetRandomWeaponByType(WeaponType type)
    {
        var matching = new List<WeaponData>();
        
        foreach (var weapon in weaponRegistry.Values)
            if (weapon.weaponType == type) matching.Add(weapon);
        
        if (matching.Count == 0) return null;
        return matching[Random.Range(0, matching.Count)];
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
        _ => Color.white
    };
}