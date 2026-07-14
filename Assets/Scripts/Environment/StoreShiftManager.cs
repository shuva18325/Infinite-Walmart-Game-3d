// ============================================================================
// SYSTEM 1a — GLOBAL DAY/NIGHT SHIFT STATE MACHINE
// Owns the MorningShift <-> NightShift cycle for the entire store.
// Synced 1:1 with the IntercomSystem: the announcement IS the transition
// signal — employees do not flip behavior until the intercom has spoken.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Environment
{
    public class StoreShiftManager : MonoBehaviour
    {
        [Header("Cycle Durations (seconds)")]
        [SerializeField] private float morningShiftDuration = 480f;
        [SerializeField] private float nightShiftDuration = 300f;

        [Header("Intercom Sync")]
        [Tooltip("Delay between the announcement starting and hostility flipping, so players hear the warning before employees turn.")]
        [SerializeField] private float announcementLeadTime = 4f;

        public ShiftPhase CurrentPhase { get; private set; } = ShiftPhase.MorningShift;

        private float _phaseClock;
        private bool _transitionQueued;

        private void Update()
        {
            _phaseClock += Time.deltaTime;

            float phaseLength = CurrentPhase == ShiftPhase.MorningShift
                ? morningShiftDuration
                : nightShiftDuration;

            // Announce slightly BEFORE the flip so the intercom leads the horror.
            if (!_transitionQueued && _phaseClock >= phaseLength - announcementLeadTime)
            {
                _transitionQueued = true;
                QueueTransitionAnnouncement();
            }

            if (_phaseClock >= phaseLength)
                AdvancePhase();
        }

        private void QueueTransitionAnnouncement()
        {
            var next = CurrentPhase == ShiftPhase.MorningShift
                ? new IntercomAnnouncement(AnnouncementType.StoreClosing, "vo_store_closing")
                : new IntercomAnnouncement(AnnouncementType.StoreOpening, "vo_store_opening");

            GameEventBus.RaiseAnnouncement(next);
        }

        private void AdvancePhase()
        {
            CurrentPhase = CurrentPhase == ShiftPhase.MorningShift
                ? ShiftPhase.NightShift
                : ShiftPhase.MorningShift;

            _phaseClock = 0f;
            _transitionQueued = false;

            // Every employee, light rig, and ambience controller listens for this.
            GameEventBus.RaiseShiftChanged(CurrentPhase);
        }

        /// Debug / scripted-event override (e.g. boss encounter forces night).
        public void ForcePhase(ShiftPhase phase)
        {
            if (phase == CurrentPhase) return;
            _phaseClock = 0f;
            _transitionQueued = false;
            CurrentPhase = phase;
            GameEventBus.RaiseShiftChanged(CurrentPhase);
        }
    }
}
