// ================================================== //
// Scripts/Weapons/WeaponController.cs
// ================================================== //

using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public string weaponKey;
    private readonly float rotationSpeed = 50f;
    private readonly float bobSpeed = 2f;
    private readonly float bobAmount = 0.3f;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Rotate
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.weaponManager != null)
            {
                WeaponModel weaponData = WeaponConfig.Instance.GetWeaponModel(weaponKey);
                if (weaponData != null)
                {
                    player.weaponManager.PickupWeapon(weaponData);
                    Debug.Log($"Picked up: {weaponData.weaponName}");
                    Destroy(gameObject);
                }
            }
        }
    }
}