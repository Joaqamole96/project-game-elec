// KeyPickup.cs
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;
        inv.AddKey();
        Destroy(gameObject);
    }
}