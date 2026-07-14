// ============================================================================
// SYSTEM 2f — ABANDONED CASH REGISTER (hide + search)
// Dual-purpose interactable, crouch-gated both ways:
//   SEARCH — crouched players loot the under-counter cubby (one-time table roll).
//   HIDE   — the crouch volume under the register pushes a Hidden override
//            onto the visibility profile: sighted AI cannot acquire the player.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    public class RegisterHidingSpot : MonoBehaviour, IInteractable
    {
        [Header("Search Loot")]
        [SerializeField] private float searchDuration = 3f;
        [Tooltip("Weighted table — Opus implements the roll. Registers skew toward scrap + rare ammo.")]
        [SerializeField] private ItemId[] lootTable =
            { ItemId.HardwareBolts, ItemId.CommonWires, ItemId.GunpowderContainer };

        [Header("Hiding Volume")]
        [Tooltip("Trigger collider under the counter. Only a CROUCHED player fits.")]
        [SerializeField] private Collider hideVolume;

        private bool _searched;

        // ------------------------------------------------------------- SEARCH
        public string Prompt => _searched ? "Register (empty)" : "Search Under Register";
        public float InteractionDuration => searchDuration;

        public bool CanInteract(PlayerInteractionContext ctx)
        {
            // Crouch-gated: you cannot see the cubby standing up.
            return !_searched
                && ctx.Controller != null
                && ctx.Controller.State == MovementState.Crouching;
        }

        public void OnInteractionComplete(PlayerInteractionContext ctx)
        {
            _searched = true;
            // TODO(Opus): weighted roll over lootTable, count ranges, rare-slot chance.
            if (lootTable.Length > 0)
                ctx.Inventory?.TryAdd(lootTable[Random.Range(0, lootTable.Length)]);

            // Rummaging is audible — quiet, but a nearby Stocker will notice.
            GameEventBus.RaiseNoise(new NoiseEvent(transform.position, 5f, 0.2f, "RegisterSearch"));
        }

        // --------------------------------------------------------------- HIDE
        private void OnTriggerEnter(Collider other)
        {
            var profile = other.GetComponentInParent<PlayerVisibilityProfile>();
            var controller = other.GetComponentInParent<PlayerController>();
            if (profile == null || controller == null) return;
            if (controller.State != MovementState.Crouching) return; // Must crawl in.

            profile.PushHiddenOverride();
        }

        private void OnTriggerExit(Collider other)
        {
            other.GetComponentInParent<PlayerVisibilityProfile>()?.PopHiddenOverride();
            // NOTE: standing up inside the volume also pops the capsule out of
            // the trigger via the height change — exit handles both cases.
        }
    }
}
