// PlayerInventory.cs
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int keys = 0;
    public void AddKey() { keys++; Debug.Log($"Key picked up. Keys={keys}"); }
    public bool UseKey() { if (keys>0) { keys--; Debug.Log($"Used key. Remaining={keys}"); return true; } return false; }
}
