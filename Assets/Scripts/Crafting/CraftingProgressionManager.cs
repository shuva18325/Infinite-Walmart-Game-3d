// ============================================================================
// SYSTEM 4b — CRAFTING PROGRESSION MANAGER
// Tracks the unlocked tier and hosts the special COMBINATION CHECK:
//   Pressure Machine + 10x Common Wires
//     → consume both, instantiate an Industrial-Grade Converter world node.
// Activating that node (see IndustrialConverterNode) permanently flags
// CraftingTier.Industrial, opening the high-grade menu:
//   Automated Electric Fences / Night Vision Goggles /
//   Riot Helmet / Bulletproof Vest / Combat Boots.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.Player;

namespace InfiniteWalmart.Crafting
{
    public class CraftingProgressionManager : MonoBehaviour
    {
        public static CraftingProgressionManager Instance { get; private set; }

        [Header("Recipe Books")]
        [SerializeField] private List<CraftingRecipe> improvisedRecipes = new();
        [Tooltip("Hidden until an Industrial Converter node is activated.")]
        [SerializeField] private List<CraftingRecipe> industrialRecipes = new();

        [Header("Converter Combination")]
        [SerializeField] private int wiresRequired = 10;
        [SerializeField] private GameObject industrialConverterPrefab;

        /// Permanent flag. Never downgrades once Industrial is reached.
        public CraftingTier UnlockedTier { get; private set; } = CraftingTier.Improvised;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ----------------------------------------------------------- MENU FEED
        /// Craft menu UI pulls this — industrial entries simply don't exist
        /// in the returned set until the tier flag flips.
        public IEnumerable<CraftingRecipe> GetAvailableRecipes()
        {
            foreach (var r in improvisedRecipes) yield return r;
            if (UnlockedTier >= CraftingTier.Industrial)
                foreach (var r in industrialRecipes) yield return r;
        }

        public bool TryCraft(CraftingRecipe recipe, InventorySystem inventory)
            => recipe.TryExecute(inventory, UnlockedTier);

        // ------------------------------------------- CONVERTER COMBINATION CHECK
        /// Called when the player uses an ACTIVE Pressure Machine item.
        /// Consumes machine + wires atomically, then drops the converter node
        /// into the world in front of the player.
        public bool TryCombineIntoConverter(InventorySystem inventory, Transform placementOrigin)
        {
            bool consumed = inventory.TryConsume(
                (ItemId.PressureMachine, 1),
                (ItemId.CommonWires, wiresRequired));
            if (!consumed) return false;

            Vector3 spawnPos = placementOrigin.position + placementOrigin.forward * 1.5f;
            Instantiate(industrialConverterPrefab, spawnPos, Quaternion.identity);
            // TODO(Opus): ground-snap raycast + placement validity check.
            return true;
        }

        // ------------------------------------------------------- TIER UNLOCK
        /// Called by IndustrialConverterNode on activation. Permanent.
        public void UnlockIndustrialTier()
        {
            if (UnlockedTier >= CraftingTier.Industrial) return;
            UnlockedTier = CraftingTier.Industrial;
            GameEventBus.RaiseCraftingTierUnlocked(UnlockedTier);
        }
    }
}
