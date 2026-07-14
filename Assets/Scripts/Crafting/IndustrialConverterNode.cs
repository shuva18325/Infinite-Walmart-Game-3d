// ============================================================================
// SYSTEM 4c — INDUSTRIAL-GRADE CONVERTER (world node)
// Instantiated by the Pressure Machine + wires combination. Sits inert until
// the player activates it (hold-interact). Activation:
//   1. Permanently unlocks CraftingTier.Industrial account-wide.
//   2. Becomes the physical crafting station for the high-grade menu.
// It also HUMS — a persistent low NoiseEvent. Industrial power isn't subtle.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.Player;

namespace InfiniteWalmart.Crafting
{
    public class IndustrialConverterNode : MonoBehaviour, IInteractable
    {
        [Header("Activation")]
        [SerializeField] private float activationDuration = 5f;

        [Header("Running Hum")]
        [SerializeField] private float humNoiseRadius = 12f;
        [SerializeField] private float humNoiseInterval = 4f;

        public bool Activated { get; private set; }
        private float _humClock;

        public string Prompt => Activated ? "Industrial Crafting" : "Activate Converter";
        public float InteractionDuration => Activated ? 0.2f : activationDuration;

        public bool CanInteract(PlayerInteractionContext ctx) => true;

        public void OnInteractionComplete(PlayerInteractionContext ctx)
        {
            if (!Activated)
            {
                Activated = true;
                CraftingProgressionManager.Instance?.UnlockIndustrialTier();
                // TODO(Opus): power-up sequence — coil glow, breaker thunk,
                // machine light stack turning green.
            }
            else
            {
                // TODO(Opus): open the crafting menu UI filtered to
                // CraftingProgressionManager.GetAvailableRecipes().
            }
        }

        private void Update()
        {
            if (!Activated) return;

            // The converter's hum periodically pings the noise bus: building
            // your forge next to a Stocker patrol route is a choice.
            _humClock += Time.deltaTime;
            if (_humClock >= humNoiseInterval)
            {
                _humClock = 0f;
                GameEventBus.RaiseNoise(new NoiseEvent(
                    transform.position, humNoiseRadius, 0.15f, "ConverterHum"));
            }
        }
    }
}
