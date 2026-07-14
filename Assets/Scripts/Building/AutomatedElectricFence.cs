// ============================================================================
// SYSTEM 4d/7c — AUTOMATED ELECTRIC FENCE (Industrial-tier AOE trap)
// Crafted from the high-grade menu. Placed like a weldable, powered by the
// global grid: during a Blackout the fence is DEAD — your defenses fail
// exactly when the store is most dangerous. Pairs the trap system to the
// power system with zero direct coupling (event bus only).
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.AI;

namespace InfiniteWalmart.Building
{
    [RequireComponent(typeof(BoxCollider))]
    public class AutomatedElectricFence : MonoBehaviour
    {
        [Header("Stun / Damage — Opus balances")]
        [SerializeField] private float stunDuration = 4f;
        [SerializeField] private float damagePerZap = 15f;
        [SerializeField] private float rearmSeconds = 2f;   // Per-entity zap cooldown.

        [Header("Power Draw")]
        [Tooltip("Optional battery mode: consumes a HighGradeBattery charge to stay live through blackouts.")]
        [SerializeField] private bool batteryBackupInstalled;

        public bool IsLive { get; private set; } = true;

        private readonly Dictionary<EmployeeEntity, float> _rearmClock = new();

        private void OnEnable() => GameEventBus.PowerStateChanged += OnPowerChanged;
        private void OnDisable() => GameEventBus.PowerStateChanged -= OnPowerChanged;

        private void OnPowerChanged(PowerState state)
        {
            IsLive = state == PowerState.PoweredOn || batteryBackupInstalled;
            // TODO(Opus): arc VFX + hum loop toggle; battery drain while backing up.
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsLive) return;

            var employee = other.GetComponentInParent<EmployeeEntity>();
            if (employee == null) return;

            if (_rearmClock.TryGetValue(employee, out float lastZap)
                && Time.time - lastZap < rearmSeconds) return;

            _rearmClock[employee] = Time.time;
            employee.ApplyStun(stunDuration);
            // TODO(Opus): apply damagePerZap through the employee health model,
            // zap SFX (which is itself a NoiseEvent — traps ring the dinner bell).
            GameEventBus.RaiseNoise(new NoiseEvent(
                transform.position, 15f, 0.5f, "FenceZap"));
        }
    }
}
