// ============================================================================
// SYSTEM 2e — INTERACTION ENGINE ("E to Loot")
// Central hold-to-interact driver. Raycasts from camera for an IInteractable,
// accumulates delta-time progress while E is held, and enforces three
// cancellation fail-safes:
//   1. Key released        → progress resets.
//   2. Target lost / moved → progress resets.
//   3. Player took damage  → progress resets (no free loots mid-attack).
// ============================================================================

using System;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    /// Anything lootable/usable implements this. Duration lives on the target
    /// so a register search and an armory breach can price time differently.
    public interface IInteractable
    {
        string Prompt { get; }                    // "Search Register", "Pry Open (Crowbar)"...
        float InteractionDuration { get; }        // Seconds of held progress required.
        bool CanInteract(PlayerInteractionContext ctx);
        void OnInteractionComplete(PlayerInteractionContext ctx);
        void OnInteractionCancelled(PlayerInteractionContext ctx) { }
    }

    /// Everything a target might need to gate or resolve an interaction.
    public readonly struct PlayerInteractionContext
    {
        public readonly InventorySystem Inventory;
        public readonly PlayerController Controller;
        public readonly PlayerStats Stats;

        public PlayerInteractionContext(InventorySystem inv, PlayerController pc, PlayerStats stats)
        {
            Inventory = inv; Controller = pc; Stats = stats;
        }
    }

    public class InteractionEngine : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private LayerMask interactableMask;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        /// HUD progress bar binds here: (current, required). Null target = hide bar.
        public event Action<float, float> ProgressChanged;
        public event Action<IInteractable> TargetChanged;

        private IInteractable _target;
        private float _progress;
        private PlayerInteractionContext _ctx;

        private void Awake()
        {
            _ctx = new PlayerInteractionContext(
                GetComponent<InventorySystem>(),
                GetComponent<PlayerController>(),
                GetComponent<PlayerStats>());

            // Fail-safe #3: damage interrupts looting.
            var stats = GetComponent<PlayerStats>();
            if (stats != null)
                stats.HealthChanged += (cur, max) => { if (_progress > 0f) Cancel(); };
        }

        private void Update()
        {
            var hit = RaycastForTarget();

            // Fail-safe #2: target changed or left range mid-hold.
            if (!ReferenceEquals(hit, _target))
            {
                if (_progress > 0f) Cancel();
                _target = hit;
                TargetChanged?.Invoke(_target);
            }

            if (_target == null) return;

            if (Input.GetKey(interactKey) && _target.CanInteract(_ctx))
            {
                _progress += Time.deltaTime;   // Delta-time accumulation, frame-rate safe.
                ProgressChanged?.Invoke(_progress, _target.InteractionDuration);

                if (_progress >= _target.InteractionDuration)
                {
                    var completed = _target;
                    ResetProgress();
                    completed.OnInteractionComplete(_ctx);
                }
            }
            else if (_progress > 0f)
            {
                Cancel(); // Fail-safe #1: key released.
            }
        }

        private IInteractable RaycastForTarget()
        {
            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out var hit, interactRange, interactableMask))
                return hit.collider.GetComponentInParent<IInteractable>();
            return null;
        }

        private void Cancel()
        {
            _target?.OnInteractionCancelled(_ctx);
            ResetProgress();
        }

        private void ResetProgress()
        {
            _progress = 0f;
            ProgressChanged?.Invoke(0f, 1f);
        }
    }
}
