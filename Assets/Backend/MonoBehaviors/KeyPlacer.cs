// KeyPlacer.cs
using UnityEngine;
using FGA.Models;

public class KeyPlacer : MonoBehaviour
{
    public FloorModel model;
    public GameObject keyPrefab;
    public Transform parent;

    public void PlaceKeys()
    {
        if (model == null || keyPrefab == null) { Debug.LogWarning("KeyPlacer missing refs"); return; }
        if (parent == null) parent = transform;
        foreach (Transform t in parent) DestroyImmediate(t.gameObject);

        foreach (var k in model.keyTiles)
        {
            Vector3 pos = new Vector3(k.x + 0.5f, 0.25f, k.y + 0.5f);
            var inst = Instantiate(keyPrefab, pos, Quaternion.identity, parent);
            // add collider + KeyPickup if not present
            if (inst.GetComponent<KeyPickup>() == null) inst.AddComponent<BoxCollider>().isTrigger = true;
            if (inst.GetComponent<KeyPickup>() == null) inst.AddComponent<KeyPickup>();
        }
    }
}