// ============================================================================
// EMPLOYEE TIER — NIGHT SHIFT MANAGER (BOSS)
// Towering apex entity. Exists ONLY during NightShift — despawns into the
// backrooms at the opening announcement. Combines full senses: hears like a
// Stocker, sees like a Roller, and commands nearby employees like a Greeter.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.AI
{
    public class NightShiftManagerBoss : EmployeeEntity
    {
        [Header("Boss Presence")]
        [SerializeField] private float footstepNoiseInterval = 1.8f;  // Its OWN steps warn the player.
        [SerializeField] private float footstepAudibleRadius = 40f;

        [Header("Phases — Opus implements phase behaviors")]
        [SerializeField] private float enrageHealthFraction = 0.4f;

        public enum BossPhase { Dormant, Stalking, Enraged }
        public BossPhase Phase { get; private set; } = BossPhase.Dormant;

        private float _footstepClock;

        protected override void OnShiftChanged(ShiftPhase phase)
        {
            base.OnShiftChanged(phase);

            if (phase == ShiftPhase.NightShift)
            {
                Phase = BossPhase.Stalking;
                gameObject.SetActive(true);
                // TODO(Opus): spawn VFX — lights die aisle-by-aisle toward spawn point.
            }
            else
            {
                Phase = BossPhase.Dormant;
                // TODO(Opus): retreat-to-backrooms sequence, then despawn.
            }
        }

        protected override void Update()
        {
            base.Update();

            // The boss broadcasts its own footsteps: players track it by sound
            // exactly the way Stockers track them. Symmetry = fair horror.
            if (Phase != BossPhase.Dormant)
            {
                _footstepClock += Time.deltaTime;
                if (_footstepClock >= footstepNoiseInterval)
                {
                    _footstepClock = 0f;
                    GameEventBus.RaiseNoise(new NoiseEvent(
                        transform.position, footstepAudibleRadius, 0.4f, "ManagerFootstep"));
                }
            }
        }

        protected override void OnNoiseHeard(NoiseEvent noise)
        {
            if (Phase == BossPhase.Dormant) return;
            if (noise.SourceTag == "ManagerFootstep") return; // Ignore self.
            Agent.SetDestination(noise.Origin);
        }

        protected override void TickPassiveWander() { /* Never passive. Dormant = despawned. */ }

        protected override void TickInvestigate() { }

        protected override void TickHunt()
        {
            // TODO(Opus): Stalking — patrol between department hubs, full-sense
            //   acquisition (hearing + vision + Greeter-relayed alerts).
            // TODO(Opus): Enraged (health < enrageHealthFraction) — tears
            //   barricades down in one hit, speed spike, lights flicker within
            //   a radius around it (hook AtmosphereLightingManager proximity API).
            _ = enrageHealthFraction;
        }

        public override void ApplyStun(float duration)
        {
            // Boss resists stuns: quarter duration, never below one second.
            base.ApplyStun(Mathf.Max(1f, duration * 0.25f));
        }
    }
}
