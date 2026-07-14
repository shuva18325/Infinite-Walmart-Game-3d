// ============================================================================
// SYSTEM 7a — MODULAR WELDING ENGINE (raycast grab / rotate / weld)
// Three-phase placement flow, driven off the player camera:
//   TARGET — raycast for a WeldableClutter (gondola shelving, clothing racks,
//            display cases) within grab range.
//   CARRY  — object floats at a hold anchor, physics suspended; player
//            rotates it freely in 3D (yaw/pitch/roll).
//   WELD   — validity check (touching a wall or another weldable), then the
//            object freezes solid, gains a BarricadeDurability pool, and the
//            weld arc SOUND pings the noise bus.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Building
{
    public class WeldPlacementSystem : MonoBehaviour
    {
        public enum WeldPhase { None, Carrying }

        [Header("Targeting")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float grabRange = 3.5f;
        [SerializeField] private LayerMask weldableMask;
        [SerializeField] private LayerMask weldSurfaceMask;   // Walls + welded clutter.

        [Header("Carry")]
        [SerializeField] private Transform holdAnchor;        // In front of camera.
        [SerializeField] private float rotationStepDegrees = 15f;

        [Header("Weld Audio Signature")]
        [SerializeField] private float weldNoiseRadius = 18f;

        [Header("Input")]
        [SerializeField] private KeyCode grabKey = KeyCode.F;
        [SerializeField] private KeyCode weldKey = KeyCode.Mouse0;

        public WeldPhase Phase { get; private set; } = WeldPhase.None;
        private WeldableClutter _held;

        private void Update()
        {
            switch (Phase)
            {
                case WeldPhase.None:
                    if (Input.GetKeyDown(grabKey)) TryGrab();
                    break;

                case WeldPhase.Carrying:
                    TickCarry();
                    if (Input.GetKeyDown(grabKey)) Drop();          // F again = release.
                    else if (Input.GetKeyDown(weldKey)) TryWeld();  // Click = weld.
                    break;
            }
        }

        // ---------------------------------------------------------------- GRAB
        private void TryGrab()
        {
            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (!Physics.Raycast(ray, out var hit, grabRange, weldableMask)) return;

            var clutter = hit.collider.GetComponentInParent<WeldableClutter>();
            if (clutter == null || clutter.IsWelded) return; // Welded = must be broken, not re-grabbed.

            _held = clutter;
            _held.EnterCarryState(holdAnchor);
            Phase = WeldPhase.Carrying;
        }

        // --------------------------------------------------------------- CARRY
        private void TickCarry()
        {
            // Free 3D rotation on key steps: R/T = yaw, scroll = pitch, roll on modifier.
            if (Input.GetKeyDown(KeyCode.R)) _held.RotateStep(Vector3.up, rotationStepDegrees);
            if (Input.GetKeyDown(KeyCode.T)) _held.RotateStep(Vector3.right, rotationStepDegrees);
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
                _held.RotateStep(Vector3.forward, scroll * rotationStepDegrees);

            // TODO(Opus): smooth-follow to holdAnchor with collision push-back,
            // red/green ghost tint from CanWeldHere() for placement feedback.
        }

        // ---------------------------------------------------------------- WELD
        private void TryWeld()
        {
            if (!CanWeldHere()) return;

            _held.Weld();

            // Welding is LOUD. Every barricade placed is an announcement.
            GameEventBus.RaiseNoise(new NoiseEvent(
                _held.transform.position, weldNoiseRadius, 0.8f, "WeldingArc"));

            _held = null;
            Phase = WeldPhase.None;
        }

        /// Structural validity: the held object must contact a wall or an
        /// already-welded object. No floating barricades.
        private bool CanWeldHere()
        {
            // TODO(Opus): overlap test of the held object's bounds against
            // weldSurfaceMask, minimum contact area threshold, and a navmesh
            // sanity check so players can't weld themselves into a sealed box.
            _ = weldSurfaceMask;
            return _held != null;
        }

        private void Drop()
        {
            _held.ExitCarryState();
            _held = null;
            Phase = WeldPhase.None;
        }
    }
}
