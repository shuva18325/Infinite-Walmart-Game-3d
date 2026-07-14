// ============================================================================
// EMPLOYEE TIER — STOCKER
// Blind. Navigates purely by sound. The counterweight to the crouch mechanic:
// a crouched player's shrunken noise radius lets them slip past Stockers that
// would instantly lock onto running footsteps.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.AI
{
    public class StockerAI : EmployeeEntity
    {
        [Header("Hearing")]
        [Tooltip("Multiplier applied to incoming NoiseEvent radii. >1 = supernatural hearing.")]
        [SerializeField] private float hearingSensitivity = 1.6f;
        [SerializeField] private float memoryDuration = 8f;   // How long a heard position stays "hot".

        private Vector3 _lastHeardPosition;
        private float _lastHeardTime = float.NegativeInfinity;
        private bool HasFreshStimulus => Time.time - _lastHeardTime < memoryDuration;

        protected override void OnNoiseHeard(NoiseEvent noise)
        {
            float effectiveRadius = noise.Radius * hearingSensitivity;
            if ((noise.Origin - transform.position).sqrMagnitude > effectiveRadius * effectiveRadius)
                return; // Out of earshot.

            _lastHeardPosition = noise.Origin;
            _lastHeardTime = Time.time;

            // Blind: NEVER sees the player — sound is the only path into aggression.
            if (CurrentShift == ShiftPhase.NightShift)
                TransitionTo(EmployeeBehaviorState.Hunting);
            else
                TransitionTo(EmployeeBehaviorState.Investigating);
        }

        protected override void TickPassiveWander()
        {
            // Morning: restocks shelves at aisle waypoints. Harmless.
            // TODO(Opus): waypoint patrol + shelf-restock idle animation loop.
        }

        protected override void TickInvestigate()
        {
            Agent.SetDestination(_lastHeardPosition);
            if (!HasFreshStimulus)
                TransitionTo(EmployeeBehaviorState.PassiveWander);
        }

        protected override void TickHunt()
        {
            // Night hunt = chase the SOUND, not the player transform.
            // Silence starves the Stocker back into a blind roam.
            if (HasFreshStimulus)
                Agent.SetDestination(_lastHeardPosition);
            // TODO(Opus): else — erratic sweep pattern around last-heard point.
        }
    }
}
