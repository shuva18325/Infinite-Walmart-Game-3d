// ============================================================================
// SYSTEM 6b — FLOOR STREAMING MANAGER
// Structural streaming architecture behind the elevators. Each floor is a
// self-contained generated scene/chunk-set keyed by (floorIndex → seed):
// the same floor index ALWAYS regenerates identically — floors feel
// persistent without ever being saved.
// Flow: unload old floor → seed-pick biome → stream/generate new floor
//       → rebake nav → raise FloorTransitionCompleted (elevator unseals).
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Elevator
{
    public class FloorStreamingManager : MonoBehaviour
    {
        public static FloorStreamingManager Instance { get; private set; }

        [Header("World Seed")]
        [SerializeField] private int worldSeed = 0;   // 0 = randomize at boot.

        [Header("Biome Distribution — Opus tunes weights")]
        [Tooltip("ArmorySportingGoods must stay RARE — it is the jackpot floor.")]
        [SerializeField] private float armoryFloorChance = 0.06f;

        public int CurrentFloorIndex { get; private set; } = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (worldSeed == 0) worldSeed = System.Environment.TickCount;
        }

        // ----------------------------------------------------------- TRANSITION
        /// Called by an elevator at the start of transit. Returns the info
        /// payload immediately; completion is announced via the event bus.
        public FloorTransitionInfo BeginTransition(int floorOffset)
        {
            int from = CurrentFloorIndex;
            int to = from + floorOffset;
            int floorSeed = DeriveFloorSeed(to);
            var biome = PickBiome(to, floorSeed);

            var info = new FloorTransitionInfo(from, to, biome, floorSeed);
            StartCoroutine(StreamFloor(info));
            return info;
        }

        private System.Collections.IEnumerator StreamFloor(FloorTransitionInfo info)
        {
            // 1. UNLOAD — everything except the elevator shaft + cabin.
            // TODO(Opus): async chunk unload, entity pool return, GC-friendly.
            yield return null;

            // 2. GENERATE — deterministic from info.GenerationSeed.
            // TODO(Opus): aisle-graph layout generator per biome, department
            //   placement, loot spawner seeding, employee population table
            //   (deep floors trend heavier tiers), light-fixture placement.
            yield return null;

            // 3. NAVMESH — rebake/stitch for the new layout.
            // TODO(Opus): runtime NavMesh build across streamed chunks.
            yield return null;

            CurrentFloorIndex = info.ToFloorIndex;
            GameEventBus.RaiseFloorTransitionCompleted(info);
        }

        // ------------------------------------------------------------- SEEDING
        /// floorIndex → stable seed. Same world, same floor, same layout.
        private int DeriveFloorSeed(int floorIndex)
        {
            unchecked { return worldSeed * 486187739 + floorIndex * 1000003; }
        }

        private FloorBiome PickBiome(int floorIndex, int floorSeed)
        {
            if (floorIndex == 0) return FloorBiome.RetailStandard; // Anchor floor.

            var rng = new System.Random(floorSeed);
            if (rng.NextDouble() < armoryFloorChance)
                return FloorBiome.ArmorySportingGoods;

            // Deeper floors skew toward backrooms; shallow stays retail.
            // TODO(Opus): full weighted table over remaining biomes vs. depth.
            return Mathf.Abs(floorIndex) > 3 ? FloorBiome.DeepBackrooms : FloorBiome.RetailStandard;
        }
    }
}
