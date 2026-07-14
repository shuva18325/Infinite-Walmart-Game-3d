// ============================================================================
// SYSTEM 3b — GENERATOR CONTROL ROOM VAULT (the repair race)
// One-shot, permanent-outcome logic:
//   PLAYER repairs generator before background timer → vault UNLOCKS forever.
//   Background timer finishes first                  → vault LOCKS forever.
// The generator repair panel is an IInteractable; the race resolves through
// PowerGridManager's events so this class never polls.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.Player;

namespace InfiniteWalmart.Power
{
    public class GeneratorVaultController : MonoBehaviour, IInteractable
    {
        [Header("Repair Interaction")]
        [SerializeField] private float repairDuration = 15f;   // Long hold — the whole race is this bar.

        [Header("Vault Geometry")]
        [SerializeField] private GameObject vaultDoor;
        [SerializeField] private Transform lootSpawnAnchor;

        public VaultOutcome Outcome { get; private set; } = VaultOutcome.Unresolved;

        // ---------------------------------------------------------- RACE WIRES
        private void OnEnable()
        {
            if (PowerGridManager.Instance != null)
                PowerGridManager.Instance.BackgroundRepairCompleted += OnBackgroundWon;
        }

        private void OnDisable()
        {
            if (PowerGridManager.Instance != null)
                PowerGridManager.Instance.BackgroundRepairCompleted -= OnBackgroundWon;
        }

        private void OnBackgroundWon()
        {
            if (Outcome != VaultOutcome.Unresolved) return;
            Outcome = VaultOutcome.PermanentlyLocked;
            GameEventBus.RaiseVaultResolved(Outcome);
            // TODO(Opus): heavy magnetic CLUNK from the vault door, one-time
            // intercom sting. The player should FEEL the loss.
        }

        // -------------------------------------------------- REPAIR INTERACTION
        public string Prompt => Outcome switch
        {
            VaultOutcome.Unresolved       => "Override Generator Repair",
            VaultOutcome.PlayerUnlocked   => "Vault (Unlocked)",
            _                             => "Vault (Sealed Permanently)"
        };

        public float InteractionDuration => repairDuration;

        public bool CanInteract(PlayerInteractionContext ctx)
        {
            // Only interactable during an active blackout race.
            return Outcome == VaultOutcome.Unresolved
                && PowerGridManager.Instance != null
                && PowerGridManager.Instance.BackgroundRepairActive;
        }

        public void OnInteractionComplete(PlayerInteractionContext ctx)
        {
            if (Outcome != VaultOutcome.Unresolved) return;

            Outcome = VaultOutcome.PlayerUnlocked;
            PowerGridManager.Instance.PlayerCompletedRepair();
            GameEventBus.RaiseVaultResolved(Outcome);

            OpenVault();
        }

        // ------------------------------------------------------------- PAYOUT
        private void OpenVault()
        {
            if (vaultDoor != null) vaultDoor.SetActive(false);

            // Spawn the registry via the data asset — contents defined there,
            // not here, so designers rebalance without touching code.
            var registry = GetComponent<VaultLootRegistry>();
            registry?.SpawnAll(lootSpawnAnchor != null ? lootSpawnAnchor : transform);

            // TODO(Opus): hydraulic door animation, interior light sweep-on,
            // reward audio sting.
        }
    }
}
