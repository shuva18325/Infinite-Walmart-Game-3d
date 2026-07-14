// ============================================================================
// SYSTEM 6a — FREIGHT ELEVATOR TRANSITION ENGINE
// The vertical seam between generated floors. Transit sequence:
//   1. SEAL     — cage doors close; safety bounds trap the player inside.
//   2. TRANSIT  — old floor unloads / new floor streams (FloorStreamingManager);
//                 atmosphere events fire (intercom static, light flicker).
//   3. ARRIVE   — waits for FloorTransitionCompleted, then unseals.
// The cabin is the loading screen — the player never sees a black screen.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.Player;

namespace InfiniteWalmart.Elevator
{
    public class FreightElevatorController : MonoBehaviour, IInteractable
    {
        public enum CabinState { IdleOpen, Sealing, InTransit, Arriving }

        [Header("Destination")]
        [SerializeField] private int floorOffset = -1;   // -1 = one floor deeper.

        [Header("Safety Boundaries")]
        [Tooltip("Solid colliders enabled during transit — nothing enters or leaves the cabin.")]
        [SerializeField] private Collider[] safetyBarriers;
        [SerializeField] private GameObject cageDoors;

        [Header("Atmosphere Hooks")]
        [SerializeField] private Light[] cabinLights;
        [SerializeField] private float minTransitSeconds = 6f; // Floor gen may extend this.

        public CabinState State { get; private set; } = CabinState.IdleOpen;

        // ----------------------------------------------------- CALL INTERACTION
        public string Prompt => State == CabinState.IdleOpen ? "Operate Freight Elevator" : "";
        public float InteractionDuration => 1.5f;   // Lever pull.
        public bool CanInteract(PlayerInteractionContext ctx) => State == CabinState.IdleOpen;

        public void OnInteractionComplete(PlayerInteractionContext ctx)
        {
            if (State != CabinState.IdleOpen) return;
            StartCoroutine(RunTransitSequence());
        }

        // ------------------------------------------------------------ SEQUENCE
        private System.Collections.IEnumerator RunTransitSequence()
        {
            // 1. SEAL
            State = CabinState.Sealing;
            SetSealed(true);
            yield return new WaitForSeconds(1.5f); // TODO(Opus): door-close anim length.

            // 2. TRANSIT — hand floor generation to the streaming manager.
            State = CabinState.InTransit;
            var transition = FloorStreamingManager.Instance.BeginTransition(floorOffset);
            GameEventBus.RaiseFloorTransitionStarted(transition);

            // Atmosphere: distorted intercom bleeds into the shaft.
            GameEventBus.RaiseAnnouncement(new IntercomAnnouncement(
                AnnouncementType.ElevatorTransit, "vo_transit_static"));

            float clock = 0f;
            bool arrived = false;
            void OnComplete(FloorTransitionInfo _) => arrived = true;
            GameEventBus.FloorTransitionCompleted += OnComplete;

            while (clock < minTransitSeconds || !arrived)
            {
                clock += Time.deltaTime;
                TickTransitAtmosphere(clock);
                yield return null;
            }
            GameEventBus.FloorTransitionCompleted -= OnComplete;

            // 3. ARRIVE
            State = CabinState.Arriving;
            yield return new WaitForSeconds(1f);   // Settle thunk.
            SetSealed(false);
            State = CabinState.IdleOpen;
        }

        private void SetSealed(bool sealed_)
        {
            foreach (var barrier in safetyBarriers) barrier.enabled = sealed_;
            if (cageDoors != null) cageDoors.SetActive(sealed_);
        }

        private void TickTransitAtmosphere(float elapsed)
        {
            // TODO(Opus): cabin light flicker pattern (hook FlickeringFluorescent
            // profiles), cable groans, cabin shudder via camera micro-shake,
            // and the rare "passing floor" silhouette flash through the cage.
            _ = cabinLights; _ = elapsed;
        }
    }
}
