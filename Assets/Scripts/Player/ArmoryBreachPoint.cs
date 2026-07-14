// ============================================================================
// SYSTEM 2g — ARMORY / SPORTING GOODS BREACH POINT
// Placed on the sealed shutters of the rare ruined Armory department
// (FloorBiome.ArmorySportingGoods). The risk/reward contract:
//   REQUIREMENT — crowbar in inventory, long pry time.
//   COST        — a massive NoiseEvent on completion. Every Stocker on the
//                 floor converges. Loot fast, or fortify first.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    public class ArmoryBreachPoint : MonoBehaviour, IInteractable
    {
        [Header("Breach Parameters")]
        [SerializeField] private float pryDuration = 8f;          // Long, tense hold.
        [SerializeField] private float breachNoiseRadius = 55f;   // Floor-wide alert.
        [SerializeField] private GameObject sealedShutter;        // Disabled on breach.

        [Header("Partial-pry alerts")]
        [Tooltip("Creaks emitted at progress milestones — pressure builds BEFORE the payoff.")]
        [SerializeField] private float creakNoiseRadius = 15f;

        public bool Breached { get; private set; }

        public string Prompt => Breached ? "" : "Pry Open Shutter (Requires Crowbar)";
        public float InteractionDuration => pryDuration;

        public bool CanInteract(PlayerInteractionContext ctx)
        {
            return !Breached && ctx.Inventory != null && ctx.Inventory.Has(ItemId.Crowbar);
        }

        public void OnInteractionComplete(PlayerInteractionContext ctx)
        {
            Breached = true;
            if (sealedShutter != null) sealedShutter.SetActive(false);

            // The loud audio alert — the defining cost of the armory.
            GameEventBus.RaiseNoise(new NoiseEvent(
                transform.position, breachNoiseRadius, 1f, "CrowbarBreach"));

            // TODO(Opus): shutter tear-off physics, metal shriek SFX, dust burst.
            // Interior loot spawners activate on breach; rare pool includes
            // MilitaryGrade ammo, WeldingTorch, and armor pieces.
        }

        public void OnInteractionCancelled(PlayerInteractionContext ctx)
        {
            // Even a FAILED pry creaks — abandoning the attempt isn't free.
            GameEventBus.RaiseNoise(new NoiseEvent(
                transform.position, creakNoiseRadius, 0.4f, "CrowbarCreak"));
        }
    }
}
