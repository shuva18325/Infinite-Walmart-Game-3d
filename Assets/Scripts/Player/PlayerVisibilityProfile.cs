// ============================================================================
// SYSTEM 2b — PLAYER VISIBILITY PROFILE
// The single component AI vision sensors query. Combines the movement
// state's base visibility (Exposed/Lowered) with situational overrides
// (Hidden while inside a register hiding volume). Overrides always win.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    public class PlayerVisibilityProfile : MonoBehaviour
    {
        public VisibilityState Current { get; private set; } = VisibilityState.Exposed;

        private VisibilityState _base = VisibilityState.Exposed;
        private int _hiddenOverrides; // Ref-counted: nested hiding volumes stack safely.

        /// Called by PlayerController on movement state transitions.
        public void SetBaseVisibility(VisibilityState state)
        {
            _base = state;
            Recompute();
        }

        /// Called by hiding spots (RegisterHidingSpot etc.) on enter/exit.
        public void PushHiddenOverride()  { _hiddenOverrides++; Recompute(); }
        public void PopHiddenOverride()   { _hiddenOverrides = Mathf.Max(0, _hiddenOverrides - 1); Recompute(); }

        private void Recompute()
        {
            var next = _hiddenOverrides > 0 ? VisibilityState.Hidden : _base;
            if (next == Current) return;
            Current = next;
            GameEventBus.RaisePlayerVisibilityChanged(Current);
        }
    }
}
