// ============================================================================
// SYSTEM 2a — PLAYER CHARACTER CONTROLLER
// Movement state machine: Idle / Walking / Running / Crouching.
// Each state carries a visibility profile and a noise radius modifier —
// this is THE stealth contract the entire AI sensor layer reads from.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Speeds — Opus tunes")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 6.5f;
        [SerializeField] private float crouchSpeed = 1.8f;

        [Header("Crouch Profile")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 0.9f;

        [Header("Noise Emission")]
        [Tooltip("Base audible radius of a walking footstep. States scale this.")]
        [SerializeField] private float baseFootstepRadius = 8f;
        [SerializeField] private float footstepInterval = 0.55f;

        public MovementState State { get; private set; } = MovementState.Idle;

        /// State-driven multipliers the stealth layer is built on.
        /// Crouch: quarter noise radius + Lowered visibility → sneaks past Stockers.
        public float CurrentNoiseMultiplier => State switch
        {
            MovementState.Idle      => 0f,
            MovementState.Walking   => 1f,
            MovementState.Running   => 2.2f,
            MovementState.Crouching => 0.25f,
            _ => 1f
        };

        private CharacterController _cc;
        private PlayerVisibilityProfile _visibility;
        private PlayerStats _stats;
        private float _footstepClock;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _visibility = GetComponent<PlayerVisibilityProfile>();
            _stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            var input = ReadMovementInput();
            var next = ResolveState(input);
            if (next != State) TransitionTo(next);

            ApplyLocomotion(input);
            TickFootstepNoise();
        }

        // ------------------------------------------------------- STATE MACHINE
        private MovementState ResolveState(Vector2 input)
        {
            bool moving = input.sqrMagnitude > 0.01f;
            bool crouchHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            bool sprintHeld = Input.GetKey(KeyCode.LeftShift);

            if (crouchHeld) return MovementState.Crouching;   // Crouch overrides sprint.
            if (!moving) return MovementState.Idle;
            if (sprintHeld && _stats != null && _stats.CanSprint) return MovementState.Running;
            return MovementState.Walking;
        }

        private void TransitionTo(MovementState next)
        {
            State = next;

            // Capsule resize for crouch — enables the under-register sightlines.
            _cc.height = next == MovementState.Crouching ? crouchHeight : standingHeight;
            // TODO(Opus): smooth height lerp + stand-up ceiling clearance check.

            // Publish visibility so AI vision cones re-evaluate immediately.
            _visibility?.SetBaseVisibility(next == MovementState.Crouching
                ? VisibilityState.Lowered
                : VisibilityState.Exposed);

            GameEventBus.RaisePlayerMovementChanged(next);
        }

        // ---------------------------------------------------------- LOCOMOTION
        private Vector2 ReadMovementInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void ApplyLocomotion(Vector2 input)
        {
            float speed = State switch
            {
                MovementState.Running   => runSpeed,
                MovementState.Crouching => crouchSpeed,
                MovementState.Walking   => walkSpeed,
                _ => 0f
            };

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            _cc.SimpleMove(move.normalized * speed);
            // TODO(Opus): gravity refinement, slope handling, camera bob per state.

            if (State == MovementState.Running)
                _stats?.DrainSprintEnergy(Time.deltaTime);
        }

        // -------------------------------------------------------- NOISE OUTPUT
        /// Footsteps broadcast onto the bus — Stockers do the listening.
        private void TickFootstepNoise()
        {
            if (State == MovementState.Idle) return;

            _footstepClock += Time.deltaTime;
            float interval = State == MovementState.Running ? footstepInterval * 0.6f : footstepInterval;
            if (_footstepClock < interval) return;
            _footstepClock = 0f;

            float radius = baseFootstepRadius * CurrentNoiseMultiplier;
            if (radius <= 0f) return;

            GameEventBus.RaiseNoise(new NoiseEvent(
                transform.position, radius, CurrentNoiseMultiplier * 0.3f, "Footstep"));
        }
    }
}
