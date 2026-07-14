// ============================================================================
// SYSTEM 1c — EMPLOYEE ENTITY BASE (all tiers derive from this)
// One behavior state machine, shift-reactive. Morning = passive wander,
// Night = tier-specific hunting. Animation distortion is driven here so
// every derived tier inherits the eerie night-time animation warp for free.
// ============================================================================

using UnityEngine;
using UnityEngine.AI;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EmployeeEntity : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] protected EmployeeTier tier;

        [Header("Locomotion — Opus tunes per-tier values")]
        [SerializeField] protected float passiveWanderSpeed = 1.2f;
        [SerializeField] protected float huntSpeed = 4.5f;

        [Header("Animation Distortion (Night Shift)")]
        [Tooltip("Animator speed multiplier at night — >1 with jitter reads as wrong/eerie.")]
        [SerializeField] protected float nightAnimSpeedMultiplier = 1.35f;

        [Header("Barricade Interaction")]
        [SerializeField] protected float barricadeDamagePerHit = 10f;
        [SerializeField] protected float barricadeAttackInterval = 1.5f;

        public EmployeeTier Tier => tier;
        public EmployeeBehaviorState State { get; protected set; } = EmployeeBehaviorState.PassiveWander;

        protected NavMeshAgent Agent;
        protected Animator Animator;
        protected ShiftPhase CurrentShift = ShiftPhase.MorningShift;

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<Animator>();
        }

        protected virtual void OnEnable()
        {
            GameEventBus.ShiftChanged += OnShiftChanged;
            GameEventBus.NoiseEmitted += OnNoiseHeard;
        }

        protected virtual void OnDisable()
        {
            GameEventBus.ShiftChanged -= OnShiftChanged;
            GameEventBus.NoiseEmitted -= OnNoiseHeard;
        }

        // ------------------------------------------------------ SHIFT REACTION
        protected virtual void OnShiftChanged(ShiftPhase phase)
        {
            CurrentShift = phase;

            if (phase == ShiftPhase.MorningShift)
            {
                TransitionTo(EmployeeBehaviorState.PassiveWander);
                Agent.speed = passiveWanderSpeed;
                ApplyAnimationProfile(distorted: false);
            }
            else
            {
                TransitionTo(EmployeeBehaviorState.Hunting);
                Agent.speed = huntSpeed;
                ApplyAnimationProfile(distorted: true);
            }
        }

        /// Distorted profile = sped-up, jittering, wrong-looking movement.
        protected virtual void ApplyAnimationProfile(bool distorted)
        {
            if (Animator == null) return;
            Animator.speed = distorted ? nightAnimSpeedMultiplier : 1f;
            Animator.SetBool("Distorted", distorted);
            // TODO(Opus): layer procedural head-twitch / limb-snap noise on the
            // animator rig when distorted == true.
        }

        // ------------------------------------------------------- STATE MACHINE
        protected void TransitionTo(EmployeeBehaviorState next)
        {
            if (State == next) return;
            OnStateExit(State);
            State = next;
            OnStateEnter(next);
        }

        protected virtual void OnStateEnter(EmployeeBehaviorState state) { }
        protected virtual void OnStateExit(EmployeeBehaviorState state) { }

        protected virtual void Update()
        {
            switch (State)
            {
                case EmployeeBehaviorState.PassiveWander:   TickPassiveWander(); break;
                case EmployeeBehaviorState.Investigating:   TickInvestigate();   break;
                case EmployeeBehaviorState.Hunting:         TickHunt();          break;
                case EmployeeBehaviorState.AttackingBarricade: TickBarricadeAttack(); break;
                case EmployeeBehaviorState.Stunned:         /* immobile */       break;
            }
        }

        // -------------------------------------------------- TIER-SPECIFIC HOOKS
        /// Morning behavior. Default: amble between aisle waypoints.
        protected abstract void TickPassiveWander();

        /// Move to last stimulus location, sweep, then return to prior state.
        protected abstract void TickInvestigate();

        /// Night behavior. Each tier hunts differently.
        protected abstract void TickHunt();

        /// Sensor callback — tiers decide their own sensitivity.
        protected abstract void OnNoiseHeard(NoiseEvent noise);

        // ---------------------------------------------------------- BARRICADES
        /// Entered when the nav path to the hunt target is blocked by a weld.
        protected virtual void TickBarricadeAttack()
        {
            // TODO(Opus): interval timer → apply barricadeDamagePerHit to the
            // blocking BarricadeDurability, raise impact NoiseEvent per swing.
        }

        // ------------------------------------------------------------- EXTERNAL
        /// Called by AutomatedElectricFence traps.
        public virtual void ApplyStun(float duration)
        {
            TransitionTo(EmployeeBehaviorState.Stunned);
            CancelInvoke(nameof(RecoverFromStun));
            Invoke(nameof(RecoverFromStun), duration);
        }

        private void RecoverFromStun() => OnShiftChanged(CurrentShift);
    }
}
