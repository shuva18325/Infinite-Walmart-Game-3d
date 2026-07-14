// ============================================================================
// SYSTEM 4a — CRAFTING RECIPE (data asset) + VALIDATION ENGINE
// One recipe = required tier + ingredient list + output. The validation
// engine is a pure function over InventorySystem: recipes never mutate
// inventory themselves — CraftingProgressionManager owns the transaction.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;
using InfiniteWalmart.Player;

namespace InfiniteWalmart.Crafting
{
    [CreateAssetMenu(menuName = "InfiniteWalmart/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [System.Serializable]
        public struct Ingredient
        {
            public ItemId Item;
            public int Count;
        }

        [Header("Gate")]
        public CraftingTier RequiredTier = CraftingTier.Improvised;

        [Header("Transaction")]
        public List<Ingredient> Ingredients = new();
        public ItemId Output;
        public int OutputCount = 1;
        public WeaponAttachmentFlags OutputAttachments = WeaponAttachmentFlags.None;

        // ---------------------------------------------------------- VALIDATION
        public bool Validate(InventorySystem inventory, CraftingTier unlockedTier)
        {
            if (RequiredTier > unlockedTier) return false;
            foreach (var ing in Ingredients)
                if (!inventory.Has(ing.Item, ing.Count)) return false;
            return true;
        }

        /// Atomic craft: consume all ingredients, grant output. False = no-op.
        public bool TryExecute(InventorySystem inventory, CraftingTier unlockedTier)
        {
            if (!Validate(inventory, unlockedTier)) return false;

            var requirements = new (ItemId, int)[Ingredients.Count];
            for (int i = 0; i < Ingredients.Count; i++)
                requirements[i] = (Ingredients[i].Item, Ingredients[i].Count);

            if (!inventory.TryConsume(requirements)) return false;

            return inventory.TryAdd(Output, OutputCount, OutputAttachments);
        }
    }
}
