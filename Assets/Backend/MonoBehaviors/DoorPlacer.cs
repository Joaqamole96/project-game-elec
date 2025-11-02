// DoorPlacer.cs
using UnityEngine;
using FGA.Models;
using System.Collections.Generic;

public class DoorPlacer : MonoBehaviour
{
    public FloorModel model;
    public GameObject doorPrefab;
    public Transform parent;

    public void PlaceDoors()
    {
        if (model == null || doorPrefab == null) { Debug.LogWarning("DoorPlacer missing refs"); return; }
        if (parent == null) { parent = this.transform; }
        // clear old
        foreach (Transform t in parent) DestroyImmediate(t.gameObject);
        var spawned = new List<GameObject>();

        foreach (var p in model.doorTiles)
        {
            Vector3 pos = new Vector3(p.x + 0.5f, 0f, p.y + 0.5f);
            var inst = Instantiate(doorPrefab, pos, Quaternion.identity, parent);
            spawned.Add(inst);
        }
        // mark locked doors visually
        foreach (var p in model.lockedDoorTiles)
        {
            // find spawned door at that pos (simple nearest)
            foreach (Transform t in parent)
            {
                if (Vector3.Distance(t.position, new Vector3(p.x + 0.5f, 0f, p.y + 0.5f)) < 0.01f)
                {
                    var d = t.GetComponent<Door>();
                    if (d != null) d.isLocked = true;
                }
            }
        }
    }
}