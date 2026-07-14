// ============================================================================
// EMPLOYEE TIER — GREETER
// The alarm system. Weak and slow, but on spotting the player it emits a
// piercing screech: a maximum-loudness NoiseEvent that every Stocker and
// Roller on the floor hears. Kill it quietly or be swarmed.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.AI
{
    public class GreeterAI : EmployeeEntity
    {
        [Header("Screech Alert")]
        [SerializeField] private float screechRadius = 60f;
        [SerializeField] private float screechCooldown = 12f;

        [Header("Vision")]
        [SerializeField] private float visionRange = 15f;

        private float _lastScreechTime = float.NegativeInfinity;

        protected override void OnNoiseHeard(NoiseEvent noise)
        {
            // Greeters barely react to sound — they are watchers, not listeners.
        }

        protected override void TickPassiveWander()
        {
            // Morning: stands near department entrances, waves at the player.
            // The same wave animation, distorted at night, is the horror beat.
        }

        protected override void TickInvestigate() { /* Greeters don't investigate. */ }

        protected override void TickHunt()
        {
            // "Hunting" for a Greeter = shuffling toward the player while
            // screaming the floor down. It is the alert, not the threat.
            if (PlayerInSight() && Time.time - _lastScreechTime > screechCooldown)
                Screech();
            // TODO(Opus): slow shamble toward player between screeches.
        }

        private void Screech()
        {
            _lastScreechTime = Time.time;

            // Loudness 1.0 = top priority for every sound-sensitive entity.
            GameEventBus.RaiseNoise(new NoiseEvent(
                transform.position, screechRadius, 1f, "GreeterScreech"));

            // TODO(Opus): screech audio + animator trigger + brief camera-shake
            // if the player is within close range.
        }

        private bool PlayerInSight()
        {
            // TODO(Opus): range + occlusion check against PlayerVisibilityProfile,
            // honoring Hidden/Lowered states like other sighted tiers.
            _ = visionRange;
            return false;
        }
    }
}
