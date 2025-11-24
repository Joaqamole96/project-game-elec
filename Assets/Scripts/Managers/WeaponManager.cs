// -------------------------------------------------- //
// Scripts/Managers/WeaponManager.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Manages player's equipped weapons and switching
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    public WeaponModel primaryWeapon;
    public WeaponModel secondaryWeapon;
    public WeaponModel currentWeapon;
    
    [Header("Weapon Inventory")]
    public WeaponModel[] availableWeapons = new WeaponModel[0];
    
    void Start()
    {
        if (primaryWeapon != null)
        {
            EquipWeapon(primaryWeapon);
        }
    }
    
    void Update()
    {
        // Switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1) && primaryWeapon != null)
        {
            EquipWeapon(primaryWeapon);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && secondaryWeapon != null)
        {
            EquipWeapon(secondaryWeapon);
        }
        
        // Scroll to switch
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            SwitchToNextWeapon(scroll > 0);
        }
    }
    
    public void EquipWeapon(WeaponModel weapon)
    {
        if (weapon == null) return;
        
        // Unequip current
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
        }
        
        // Equip new
        currentWeapon = weapon;
        currentWeapon.Equip();
    }
    
    public void Attack(Vector3 position, Vector3 direction)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Attack(position, direction);
        }
    }
    
    private void SwitchToNextWeapon(bool forward)
    {
        if (primaryWeapon == null && secondaryWeapon == null) return;
        
        if (currentWeapon == primaryWeapon && secondaryWeapon != null)
        {
            EquipWeapon(secondaryWeapon);
        }
        else if (primaryWeapon != null)
        {
            EquipWeapon(primaryWeapon);
        }
    }
    
    public void PickupWeapon(WeaponModel newWeapon)
    {
        if (primaryWeapon == null)
        {
            primaryWeapon = newWeapon;
            EquipWeapon(primaryWeapon);
        }
        else if (secondaryWeapon == null)
        {
            secondaryWeapon = newWeapon;
        }
        else
        {
            // Replace current weapon
            if (currentWeapon == primaryWeapon)
            {
                primaryWeapon = newWeapon;
                EquipWeapon(primaryWeapon);
            }
            else
            {
                secondaryWeapon = newWeapon;
                EquipWeapon(secondaryWeapon);
            }
        }
        
        Debug.Log($"Picked up: {newWeapon.weaponName}");
    }
}