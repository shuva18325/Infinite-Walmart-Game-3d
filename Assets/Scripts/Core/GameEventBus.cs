// ============================================================================
// INFINITE WALMART — GLOBAL EVENT BUS
// The communication spine of the entire game. Every system publishes and
// subscribes here; no system holds a direct reference to another system.
// This keeps all eight modules hot-swappable and independently testable.
//
// RULE: systems RAISE events about themselves, LISTEN for events about others.
// ============================================================================

using System;

namespace InfiniteWalmart.Core
{
    public static class GameEventBus
    {
        // ----------------------------------------------------- SHIFT / INTERCOM
        /// Fired by StoreShiftManager the moment the day/night state flips.
        public static event Action<ShiftPhase> ShiftChanged;

        /// Fired by IntercomSystem when an announcement begins playback.
        public static event Action<IntercomAnnouncement> AnnouncementPlayed;

        // ----------------------------------------------------------------- POWER
        /// Fired by PowerGridManager on PoweredOn <-> Blackout flips.
        public static event Action<PowerState> PowerStateChanged;

        /// Fired exactly once by GeneratorVaultController when the repair race ends.
        public static event Action<VaultOutcome> VaultResolved;

        // ----------------------------------------------------------------- NOISE
        /// Fired by ANY audible actor: footsteps, gunshots, crowbar breaches,
        /// barricade impacts. Sound-sensitive AI subscribes here.
        public static event Action<NoiseEvent> NoiseEmitted;

        // ---------------------------------------------------------------- PLAYER
        /// Fired by PlayerController when the movement state machine transitions.
        public static event Action<MovementState> PlayerMovementChanged;

        /// Fired by PlayerVisibilityProfile when concealment level changes.
        public static event Action<VisibilityState> PlayerVisibilityChanged;

        /// Fired by PlayerStats when health reaches zero.
        public static event Action PlayerDied;

        // -------------------------------------------------------------- CRAFTING
        /// Fired by CraftingProgressionManager when a tier is permanently unlocked.
        public static event Action<CraftingTier> CraftingTierUnlocked;

        // ---------------------------------------------------------------- FLOORS
        /// Fired by FreightElevatorController when the cabin seals and departs.
        public static event Action<FloorTransitionInfo> FloorTransitionStarted;

        /// Fired by FloorStreamingManager once the destination floor is fully
        /// streamed in and the cabin doors are cleared to open.
        public static event Action<FloorTransitionInfo> FloorTransitionCompleted;

        // ------------------------------------------------------------ BARRICADES
        /// Fired by BarricadeDurability when a welded object is destroyed.
        /// AI pathing listens to re-open blocked aisle routes.
        public static event Action<UnityEngine.Vector3> BarricadeDestroyed;

        // ================================================================ RAISERS
        // Null-safe raise wrappers. Systems call these; never invoke events raw.

        public static void RaiseShiftChanged(ShiftPhase phase)                  => ShiftChanged?.Invoke(phase);
        public static void RaiseAnnouncement(IntercomAnnouncement a)            => AnnouncementPlayed?.Invoke(a);
        public static void RaisePowerStateChanged(PowerState state)             => PowerStateChanged?.Invoke(state);
        public static void RaiseVaultResolved(VaultOutcome outcome)             => VaultResolved?.Invoke(outcome);
        public static void RaiseNoise(NoiseEvent noise)                         => NoiseEmitted?.Invoke(noise);
        public static void RaisePlayerMovementChanged(MovementState s)          => PlayerMovementChanged?.Invoke(s);
        public static void RaisePlayerVisibilityChanged(VisibilityState s)      => PlayerVisibilityChanged?.Invoke(s);
        public static void RaisePlayerDied()                                    => PlayerDied?.Invoke();
        public static void RaiseCraftingTierUnlocked(CraftingTier tier)         => CraftingTierUnlocked?.Invoke(tier);
        public static void RaiseFloorTransitionStarted(FloorTransitionInfo i)   => FloorTransitionStarted?.Invoke(i);
        public static void RaiseFloorTransitionCompleted(FloorTransitionInfo i) => FloorTransitionCompleted?.Invoke(i);
        public static void RaiseBarricadeDestroyed(UnityEngine.Vector3 pos)     => BarricadeDestroyed?.Invoke(pos);
    }
}
