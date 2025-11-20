// WeaponModel.cs
using System.Collections.Generic;
using UnityEngine;

public class WeaponModel
{
    public string Name;
    public WeaponRarity Rarity;

    public static List<WeaponModel> Weapons = new()
    {
        new("Iron Sword", WeaponRarity.Common),
        new("Stone Hammer", WeaponRarity.Common),
        new("Wooden Bow", WeaponRarity.Common),
        new("Sling", WeaponRarity.Common),
    };

    public WeaponModel(string name, WeaponRarity rarity)
    {
        Name = name;
        Rarity = rarity;
    }

    public WeaponModel GetWeaponByName(string name)
    {
        return Weapons.Find(weapon => weapon.Name == name);
    }

    public WeaponModel GetRandomWeaponByRarity(WeaponRarity rarity)
    {
        var filteredWeapons = Weapons.FindAll(weapon => weapon.Rarity == rarity);
        if (filteredWeapons.Count == 0) 
            return null;
        var randomIndex = Random.Range(0, filteredWeapons.Count);
        return filteredWeapons[randomIndex];
    }
}