// ============================================================================
// Views for BSP Dungeon Visualization (Gizmos-based Debug Rendering)
// Each view corresponds to a model and is responsible for drawing it in Scene view.
// Each script should be placed under FGA/Views
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using FGA.Models;

namespace FGA.Views
{
    // ============================================================================
    // FloorView.cs
    // Renders the complete BSP dungeon floor using Gizmos for visual debugging
    // ============================================================================
    [ExecuteInEditMode]
    public class FloorView : MonoBehaviour
    {
        [Header("References")]
        public FloorModel floorModel; // Reference to the data model holding partitions, rooms, and corridors

        [Header("Visualization Settings")]
        public bool drawPartitions = true; // Toggle for partition visualization
        public bool drawRooms = true;       // Toggle for room visualization
        public bool drawCorridors = true;   // Toggle for corridor visualization

        private void OnDrawGizmos()
        {
            // Avoid drawing if model is missing
            if (floorModel == null) return;

            // Draw hierarchical BSP partitions (colored by depth)
            if (drawPartitions && floorModel.RootPartition != null)
                DrawPartition(floorModel.RootPartition);

            // Draw rooms with filled color and outlines
            if (drawRooms && floorModel.Rooms != null)
            {
                foreach (RoomModel room in floorModel.Rooms)
                    DrawRoom(room);
            }

            // Draw corridor connections as yellow line segments
            if (drawCorridors && floorModel.Corridors != null)
            {
                foreach (CorridorModel corridor in floorModel.Corridors)
                    DrawCorridor(corridor);
            }
        }

        // Recursively draws all partitions with color indicating depth
        private void DrawPartition(PartitionModel partition)
        {
            if (partition == null) return;

            // Lerp between red (shallow) and blue (deep) for visual depth cue
            Gizmos.color = Color.Lerp(Color.red, Color.blue, partition.Depth / 5f);

            // Convert 2D rect (x, y) into world 3D coordinates (x, z)
            Vector3 center = new Vector3(partition.Bounds.x + partition.Bounds.width / 2f, 0, partition.Bounds.y + partition.Bounds.height / 2f);
            Vector3 size = new Vector3(partition.Bounds.width, 0.1f, partition.Bounds.height);
            Gizmos.DrawWireCube(center, size);

            // Recursively draw children partitions
            DrawPartition(partition.LeftChild);
            DrawPartition(partition.RightChild);
        }

        // Draws a room as a semi-transparent green cube with wireframe edges
        private void DrawRoom(RoomModel room)
        {
            if (room == null) return;

            Vector3 center = new Vector3(room.Bounds.x + room.Bounds.width / 2f, 0, room.Bounds.y + room.Bounds.height / 2f);
            Vector3 size = new Vector3(room.Bounds.width, 0.1f, room.Bounds.height);

            // Fill color (semi-transparent green)
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawCube(center, size);

            // Outline for better visibility
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }

        // Draws corridors as connected line segments between grid coordinates
        private void DrawCorridor(CorridorModel corridor)
        {
            if (corridor == null || corridor.Path == null || corridor.Path.Count < 2) return;

            Gizmos.color = Color.yellow;

            // Iterate through all path points and draw connecting lines
            for (int i = 0; i < corridor.Path.Count - 1; i++)
            {
                Vector3 start = new Vector3(corridor.Path[i].x, 0, corridor.Path[i].y);
                Vector3 end = new Vector3(corridor.Path[i + 1].x, 0, corridor.Path[i + 1].y);
                Gizmos.DrawLine(start, end);
            }
        }
    }

    // ============================================================================
    // DebugGizmoSettings.cs
    // Provides global toggles and parameters for controlling Gizmo visibility
    // ============================================================================
    [CreateAssetMenu(fileName = "DebugGizmoSettings", menuName = "FGA/Debug Gizmo Settings")]
    public class DebugGizmoSettings : ScriptableObject
    {
        [Header("Global Toggles")]
        public bool showPartitions = true;
        public bool showRooms = true;
        public bool showCorridors = true;

        [Header("Visual Parameters")]
        [Range(0f, 1f)] public float roomTransparency = 0.3f; // Adjusts room opacity in Gizmos
    }
}