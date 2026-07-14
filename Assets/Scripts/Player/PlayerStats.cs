// ============================================================================
// SYSTEM 2c — PLAYER STATS (health / energy)
// Pure state container with events. No regen curves here — Opus owns those.
// ============================================================================

using System;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Pools")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxEnergy = 100f;

        [Header("Energy Costs — Opus tunes")]
        [SerializeField] private float sprintDrainPerSecond = 12f;
        [SerializeField] private float minEnergyToSprint = 5f;

        public float Health { get; private set; }
        public float Energy { get; private set; }
        public bool CanSprint => Energy > minEnergyToSprint;
        public bool IsDead => Health <= 0f;

        /// HUD subscribes to these (current, max).
        public event Action<float, float> HealthChanged;
        public event Action<float, float> EnergyChanged;

        [Header("Armor Mitigation Flags — set by equipping Industrial gear")]
        public bool RiotHelmetEquipped;
        public bool BulletproofVestEquipped;
        public bool CombatBootsEquipped;   // Also reduces footstep noise — PlayerController may read this.

        private void Awake()
        {
            Health = maxHealth;
            Energy = maxEnergy;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            // TODO(Opus): mitigation formula from equipped armor flags.
            Health = Mathf.Max(0f, Health - amount);
            HealthChanged?.Invoke(Health, maxHealth);

            if (IsDead)
                GameEventBus.RaisePlayerDied();
        }

        public void Heal(float amount)
        {
            Health = Mathf.Min(maxHealth, Health + amount);
            HealthChanged?.Invoke(Health, maxHealth);
        }

        public void DrainSprintEnergy(float deltaTime)
        {
            Energy = Mathf.Max(0f, Energy - sprintDrainPerSecond * deltaTime);
            EnergyChanged?.Invoke(Energy, maxEnergy);
        }

        public void RestoreEnergy(float amount)
        {
            Energy = Mathf.Min(maxEnergy, Energy + amount);
            EnergyChanged?.Invoke(Energy, maxEnergy);
        }

        // TODO(Opus): idle energy regen tick (suppressed while sprinting).
    }
}
