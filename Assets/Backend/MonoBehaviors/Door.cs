// Door.cs
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isLocked = false;
    public bool isOpen = false;
    public GameObject doorVisual; // optional child to hide

    void Awake() { ApplyState(); }

    void ApplyState()
    {
        if (doorVisual) doorVisual.SetActive(!isOpen);
        var col = GetComponent<Collider>();
        if (col) col.enabled = !isOpen;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public void Open()
    {
        isOpen = true; ApplyState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;
        var inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;
        if (!isLocked) Open();
        else if (inv.UseKey()) { Unlock(); Open(); }
        else { Debug.Log("Door locked. Need a key."); }
    }
}