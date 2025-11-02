// FloorView.cs
using UnityEngine;
using FGA.Models;
using System.Collections.Generic;

namespace FGA.Presentation
{
    [ExecuteAlways]
    public class FloorView : MonoBehaviour
    {
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject doorPrefab;
        public GameObject keyPrefab;

        public float tileSize = 1f;
        public bool instantiateTiles = true;

        [Header("Gizmos")]
        public bool showGizmos = true;
        public bool showTilesGizmo = false;

        Transform container;
        public FloorModel lastModel;

        public void Render(FloorModel model)
        {
            lastModel = model;
            if (model == null) return;

            if (instantiateTiles)
            {
                Clear();

                container = new GameObject("Tiles").transform;
                container.SetParent(transform, false);

                for (int x = 0; x < model.width; x++)
                for (int y = 0; y < model.height; y++)
                {
                    var t = model.GetTile(x, y);
                    GameObject prefab = null;
                    switch (t)
                    {
                        case TileType.Floor: prefab = floorPrefab; break;
                        case TileType.Wall: prefab = wallPrefab; break;
                        case TileType.Door: prefab = doorPrefab; break;
                        case TileType.LockedDoor: prefab = doorPrefab; break;
                        case TileType.Key: prefab = keyPrefab; break;
                    }
                    if (prefab == null) continue;
                    Vector3 pos = new Vector3(x * tileSize + tileSize * 0.5f, 0f, y * tileSize + tileSize * 0.5f);
                    var go = Instantiate(prefab, pos, Quaternion.identity, container);
                    if (model.lockedDoorTiles.Contains(new Vector2Int(x, y)))
                    {
                        var d = go.GetComponent<MonoBehaviour>(); // door script optional
                    }
                }
            }
        }

        public void Clear()
        {
            if (container == null) return;
            #if UNITY_EDITOR
            DestroyImmediate(container.gameObject);
            #else
            Destroy(container.gameObject);
            #endif
            container = null;
        }

        void OnDrawGizmos()
        {
            if (!showGizmos || lastModel == null) return;

            if (showTilesGizmo)
            {
                for (int x = 0; x < lastModel.width; x++)
                for (int y = 0; y < lastModel.height; y++)
                {
                    var t = lastModel.GetTile(x, y);
                    Vector3 center = transform.position + new Vector3(x * tileSize + tileSize * 0.5f, 0.01f, y * tileSize + tileSize * 0.5f);
                    if (t == TileType.Floor) { Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.25f); Gizmos.DrawCube(center, new Vector3(tileSize * 0.9f, 0.02f, tileSize * 0.9f)); }
                    else if (t == TileType.Wall) { Gizmos.color = new Color(0.15f, 0.15f, 0.15f, 0.6f); Gizmos.DrawCube(center + Vector3.up * 0.5f, new Vector3(tileSize, 1f, tileSize)); }
                    else if (t == TileType.Door) { Gizmos.color = new Color(0.45f, 0.25f, 0.07f); Gizmos.DrawCube(center + Vector3.up * 0.25f, new Vector3(tileSize * 0.9f, 0.5f, tileSize * 0.9f)); }
                    else if (t == TileType.LockedDoor) { Gizmos.color = Color.red; Gizmos.DrawCube(center + Vector3.up * 0.25f, new Vector3(tileSize * 0.95f, 0.6f, tileSize * 0.95f)); }
                    else if (t == TileType.Key) { Gizmos.color = Color.yellow; Gizmos.DrawSphere(center + Vector3.up * 0.15f, tileSize * 0.12f); }
                }
            }

            // rooms outlines
            foreach (var r in lastModel.rooms)
            {
                var rect = r.rect;
                Vector3 center = transform.position + new Vector3(rect.center.x * tileSize, 0.02f, rect.center.y * tileSize);
                Vector3 size = new Vector3(rect.width * tileSize, 0.02f, rect.height * tileSize);
                Gizmos.color = new Color(0f, 1f, 0f, 0.12f); Gizmos.DrawCube(center, size);
                Gizmos.color = Color.green; Gizmos.DrawWireCube(center, size);
            }

            // door markers
            foreach (var d in lastModel.doorTiles)
            {
                Vector3 pos = transform.position + new Vector3(d.x * tileSize + tileSize * 0.5f, 0.02f, d.y * tileSize + tileSize * 0.5f);
                Gizmos.color = new Color(0.45f, 0.25f, 0.07f);
                Gizmos.DrawWireCube(pos, new Vector3(tileSize * 0.9f, 0.04f, tileSize * 0.9f));
            }

            foreach (var ld in lastModel.lockedDoorTiles)
            {
                Vector3 pos = transform.position + new Vector3(ld.x * tileSize + tileSize * 0.5f, 0.02f, ld.y * tileSize + tileSize * 0.5f);
                Gizmos.color = Color.red; Gizmos.DrawWireCube(pos, new Vector3(tileSize * 0.95f, 0.06f, tileSize * 0.95f));
            }

            foreach (var k in lastModel.keyTiles)
            {
                Vector3 pos = transform.position + new Vector3(k.x * tileSize + tileSize * 0.5f, 0.12f, k.y * tileSize + tileSize * 0.5f);
                Gizmos.color = Color.yellow; Gizmos.DrawSphere(pos, tileSize * 0.12f);
            }
        }
    }
}