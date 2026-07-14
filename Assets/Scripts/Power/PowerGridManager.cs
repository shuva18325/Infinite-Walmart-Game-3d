// ============================================================================
// SYSTEM 3a — GLOBAL POWER & GENERATOR STATE MACHINE
// Owns the PoweredOn <-> Blackout world state. During a Blackout, a hidden
// background countdown runs: unseen AI employees are "walking to the
// generator room". The vault race (GeneratorVaultController) is timed
// against this counter. Lighting, elevators, and crafting nodes all listen.
// ============================================================================

using System;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Power
{
    public class PowerGridManager : MonoBehaviour
    {
        public static PowerGridManager Instance { get; private set; }

        [Header("Blackout Scheduling — Opus tunes randomness")]
        [SerializeField] private Vector2 blackoutIntervalRange = new(180f, 420f);

        [Header("Background AI Repair")]
        [Tooltip("Seconds until unseen employee AI restores power on its own.")]
        [SerializeField] private float backgroundRepairDuration = 120f;

        public PowerState State { get; private set; } = PowerState.PoweredOn;

        /// Countdown remaining on the automated repair event. Only meaningful
        /// during Blackout. The vault UI displays this as mounting pressure.
        public float BackgroundRepairRemaining { get; private set; }
        public bool BackgroundRepairActive => State == PowerState.Blackout && BackgroundRepairRemaining > 0f;

        /// Fired every frame during blackout — HUD timers subscribe.
        public event Action<float> RepairCountdownTicked;

        /// Fired when the BACKGROUND AI (not the player) completes the repair.
        /// GeneratorVaultController resolves the race off this.
        public event Action BackgroundRepairCompleted;

        private float _nextBlackoutClock;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ScheduleNextBlackout();
        }

        private void Update()
        {
            switch (State)
            {
                case PowerState.PoweredOn:
                    _nextBlackoutClock -= Time.deltaTime;
                    if (_nextBlackoutClock <= 0f) TriggerBlackout();
                    break;

                case PowerState.Blackout:
                    BackgroundRepairRemaining -= Time.deltaTime;
                    RepairCountdownTicked?.Invoke(BackgroundRepairRemaining);
                    if (BackgroundRepairRemaining <= 0f)
                    {
                        // The invisible employees beat the player to it.
                        BackgroundRepairCompleted?.Invoke();
                        RestorePower();
                    }
                    break;
            }
        }

        // ------------------------------------------------------------- ACTIONS
        public void TriggerBlackout()
        {
            if (State == PowerState.Blackout) return;
            State = PowerState.Blackout;
            BackgroundRepairRemaining = backgroundRepairDuration;

            GameEventBus.RaisePowerStateChanged(State);
            GameEventBus.RaiseAnnouncement(new IntercomAnnouncement(
                AnnouncementType.BlackoutNotice, "vo_blackout_notice"));
        }

        /// Called by GeneratorVaultController when the PLAYER completes the
        /// manual override. Halts the background counter — player won the race.
        public void PlayerCompletedRepair()
        {
            if (State != PowerState.Blackout) return;
            BackgroundRepairRemaining = 0f;
            RestorePower();
        }

        private void RestorePower()
        {
            State = PowerState.PoweredOn;
            ScheduleNextBlackout();
            GameEventBus.RaisePowerStateChanged(State);
        }

        private void ScheduleNextBlackout()
        {
            _nextBlackoutClock = UnityEngine.Random.Range(
                blackoutIntervalRange.x, blackoutIntervalRange.y);
        }
    }
}
