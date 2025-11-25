// -------------------------------------------------- //
// Scripts/Configs/BiomeConfig.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeConfig", menuName = "Configs/Biome Config")]
[System.Serializable]
public class BiomeConfig : ScriptableObject
{
    public List<BiomeModel> Biomes = new()
    {
        new BiomeModel("Grasslands", 1, 5),

        new BiomeModel("Caverns", 1, 5),
        /// A gentle, earthy cave system beneath ancient forests.
        /// Design: Damp stone floor, mossy walls, wooden-root doorways.
        /// Enemies: Cave beasts, moss-coated wildlife, and low-rank corrupted critters.
        /// Gimmick: Spores and dim lighting that mildly reduce detection or stamina regen.

        new BiomeModel("Town", 3, 6),
        /// A collapsed watchtown belonging to a forgotten kingdom.
        /// Design: Broken stonework, vine-choked halls, loose rubble floors, iron-banded doors.
        /// Enemies: Low-tier undead, corrupted militia remnants, animated debris.
        /// Gimmick: Weak walls and floors that occasionally crumble or open shortcuts.

        new BiomeModel("Cliffs", 4, 7),
        /// A mountain pass battered by constant wind and occasional rain.
        /// Design: Rocky paths, narrow ledges, wooden palisades, rope-bridge doorways.
        /// Enemies: Cliff wolves, mountain bandits, storm-touched beasts.
        /// Gimmick: Gusts that influence movement but never become “physics magic”—just environmental pressure.
        
        new BiomeModel("Hollows", 6, 10),
        /// A forest burned during the Cataclysm, still smoldering at its heart.
        /// Design: Charcoal bark, ash-coated ground, scorched stone, ember gate doors.
        /// Enemies: Fire-scarred beasts, ash spirits, corrupted wildlife.
        /// Gimmick: Persistent ember pools and occasional flare-ups.
        
        new BiomeModel("Keeps", 6, 10),
        /// A frozen stronghold once used by wardens who tried to contain magic leaks.
        /// Design: Frosted stone floors, icy walls, cold-metal doors.
        /// Enemies: Frost wolves, chilled spirits, armored guardians left behind.
        /// Gimmick: Cold zones that slow movement slightly unless managed.

        new BiomeModel("Woodlands", 9, 12),
        /// A dense, overgrown woodland where sunlight barely reaches.
        /// Design: Thick underbrush, old trees, dirt floors, wooden stockade doors.
        /// Enemies: Ambush predators, corrupted beasts, stealthy humanoids.
        /// Gimmick: Shadows and brush patches that help enemies hide and flank.

        new BiomeModel("Warrens", 10, 13),
        /// An abandoned dwarven mining complex partially collapsed.
        /// Design: Metal tracks, carved stone tunnels, heavy iron doors, mine supports.
        /// Enemies: Burrowing creatures, rogue constructs, corrupted miners.
        /// Gimmick: Mine hazards—falling beams, loose gravel, rolling carts.
        
        new BiomeModel("Highlands", 11, 15),
        /// A dark plateau under eternal cloud cover.
        /// Design: Black rock, tall cliffs, dim lanterns, reinforced gate-doors.
        /// Enemies: Shadow-touched knights, elite beasts, spectral stalkers.
        /// Gimmick: Low light levels—enemies get better at ambush, but never supernatural phasing.

        new BiomeModel("Forge", 11, 15),
        /// A magma-lined cavern system once used as a grand forge.
        /// Design: Basalt floors, glowing cracks, metal bridges, blast-doors.
        /// Enemies: Lava beasts, armored elementals, corrupted smith-constructs.
        /// Gimmick: Heat surges—predictable patterns of hot zones.
    };
}