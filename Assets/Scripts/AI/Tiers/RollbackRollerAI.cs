// ============================================================================
// EMPLOYEE TIER — ROLLBACK ROLLER
// Rides a motorized cart. Fastest tier in straight aisles, terrible at
// corners. Sighted hunter: uses a vision cone, so crouching behind clutter
// (not silence) is the counter-play.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.AI
{
    public class RollbackRollerAI : EmployeeEntity
    {
        [Header("Cart Dynamics — Opus implements the drive model")]
        [SerializeField] private float chargeSpeed = 8f;        // Straight-line burst.
        [SerializeField] private float corneringPenalty = 0.35f; // Speed fraction in turns.

        [Header("Vision")]
        [SerializeField] private float visionRange = 25f;
        [SerializeField] private float visionConeDegrees = 70f;

        private Transform _sightedTarget;

        protected override void OnNoiseHeard(NoiseEvent noise)
        {
            // Hears poorly over its own motor — only reacts to LOUD events.
            if (noise.Loudness < 0.7f) return;
            if (State == EmployeeBehaviorState.Hunting) return;
            TransitionTo(EmployeeBehaviorState.Investigating);
            Agent.SetDestination(noise.Origin);
        }

        protected override void TickPassiveWander()
        {
            // Morning: slow patrol lap of the main aisles, motor humming.
            // The hum is an audio landmark players learn to track.
        }

        protected override void TickInvestigate()
        {
            TryAcquireVisualTarget();
            if (Agent.remainingDistance < 1f)
                TransitionTo(CurrentShift == ShiftPhase.NightShift
                    ? EmployeeBehaviorState.Hunting
                    : EmployeeBehaviorState.PassiveWander);
        }

        protected override void TickHunt()
        {
            TryAcquireVisualTarget();
            if (_sightedTarget != null)
            {
                Agent.SetDestination(_sightedTarget.position);
                // TODO(Opus): drive model — chargeSpeed on straights, apply
                // corneringPenalty from path curvature, drift SFX + skid marks.
            }
        }

        private void TryAcquireVisualTarget()
        {
            // TODO(Opus): cone check against PlayerVisibilityProfile:
            //  - VisibilityState.Hidden  → never acquirable.
            //  - VisibilityState.Lowered → visionRange halved (crouch payoff).
            //  - Occlusion raycast through shelving/barricades.
            // Assign _sightedTarget on success, clear after lose-sight grace.
            _ = visionRange; _ = visionConeDegrees; _ = chargeSpeed; _ = corneringPenalty;
        }
    }
}
