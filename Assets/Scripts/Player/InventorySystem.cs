// ============================================================================
// SYSTEM 2d — INVENTORY SYSTEM
// Slot-based storage for weapons, ammo tiers, scrap, and building blocks.
// Crafting systems consume through TryConsume — never mutate slots directly.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Player
{
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private int slotCapacity = 24;

        private readonly List<ItemStack> _slots = new();

        /// Fired on any mutation — HUD and crafting menus refresh from this.
        public event Action InventoryChanged;

        // ------------------------------------------------------------- QUERIES
        public int CountOf(ItemId id)
        {
            int total = 0;
            foreach (var s in _slots)
                if (s.Id == id) total += s.Count;
            return total;
        }

        public bool Has(ItemId id, int count = 1) => CountOf(id) >= count;

        public IReadOnlyList<ItemStack> Slots => _slots;

        // ------------------------------------------------------------ MUTATION
        public bool TryAdd(ItemId id, int count = 1,
                           WeaponAttachmentFlags attachments = WeaponAttachmentFlags.None)
        {
            // TODO(Opus): stack-merge pass against ItemDefinition.MaxStack
            // before opening a new slot.
            if (_slots.Count >= slotCapacity) return false;

            _slots.Add(new ItemStack(id, count, attachments));
            InventoryChanged?.Invoke();
            return true;
        }

        /// Atomic multi-item consume: either everything is removed or nothing is.
        /// This is the primitive every crafting recipe validation stands on.
        public bool TryConsume(params (ItemId id, int count)[] requirements)
        {
            foreach (var (id, count) in requirements)
                if (!Has(id, count)) return false;

            foreach (var (id, count) in requirements)
                RemoveInternal(id, count);

            InventoryChanged?.Invoke();
            return true;
        }

        private void RemoveInternal(ItemId id, int count)
        {
            for (int i = _slots.Count - 1; i >= 0 && count > 0; i--)
            {
                if (_slots[i].Id != id) continue;
                int taken = Mathf.Min(_slots[i].Count, count);
                _slots[i].Count -= taken;
                count -= taken;
                if (_slots[i].Count <= 0) _slots.RemoveAt(i);
            }
        }
    }
}
