// ============================================================================
// INFINITE WALMART — ITEM DEFINITION (ScriptableObject registry entry)
// Every lootable, craftable, or equippable object in the game is described
// by one of these data assets. Systems reference ItemId; only presentation
// layers touch the prefab/icon fields.
// ============================================================================

using UnityEngine;

namespace InfiniteWalmart.Core
{
    [CreateAssetMenu(menuName = "InfiniteWalmart/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public ItemId Id;
        public ItemCategory Category;
        public string DisplayName;
        [TextArea] public string FlavorText;

        [Header("Stacking")]
        public bool Stackable;
        public int MaxStack = 1;

        [Header("Presentation — Opus wires these")]
        public GameObject WorldPrefab;   // Dropped / placed representation.
        public Sprite InventoryIcon;

        [Header("Weapon-only data (ignored otherwise)")]
        public AmmoTier CompatibleAmmoTier;
        public WeaponAttachmentFlags DefaultAttachments = WeaponAttachmentFlags.None;
    }

    /// A live inventory entry: definition + count + per-instance state.
    [System.Serializable]
    public class ItemStack
    {
        public ItemId Id;
        public int Count;

        /// Per-instance flags (e.g. the vault rifle ships pre-modified with
        /// Silencer | DrumMag | Flashlight | ExtraGrip regardless of defaults).
        public WeaponAttachmentFlags InstanceAttachments;

        public ItemStack(ItemId id, int count,
                         WeaponAttachmentFlags attachments = WeaponAttachmentFlags.None)
        {
            Id = id;
            Count = count;
            InstanceAttachments = attachments;
        }
    }
}
