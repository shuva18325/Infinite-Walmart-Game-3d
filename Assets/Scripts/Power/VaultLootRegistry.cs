// ============================================================================
// SYSTEM 3c — VAULT ITEM REGISTRY
// Canonical high-tier payout, defined as data slots. Default manifest:
//   - High-Grade Batteries      x3
//   - Common Wires              x10  (exactly one Industrial Converter's worth)
//   - Pressure Machine          x1   (the OTHER converter ingredient — the
//                                     vault is the intended industrial on-ramp)
//   - Military-Grade Assault Rifle x1, pre-modified:
//       Silencer | DrumMag | Flashlight | ExtraGrip
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Power
{
    public class VaultLootRegistry : MonoBehaviour
    {
        [System.Serializable]
        public struct VaultSlot
        {
            public ItemId Item;
            public int Count;
            public WeaponAttachmentFlags Attachments;
        }

        [Header("Registry Slots")]
        [SerializeField] private List<VaultSlot> slots = new()
        {
            new VaultSlot { Item = ItemId.HighGradeBattery, Count = 3 },
            new VaultSlot { Item = ItemId.CommonWires,      Count = 10 },
            new VaultSlot { Item = ItemId.PressureMachine,  Count = 1 },
            new VaultSlot
            {
                Item = ItemId.MilitaryAssaultRifle,
                Count = 1,
                Attachments = WeaponAttachmentFlags.Silencer
                            | WeaponAttachmentFlags.DrumMag
                            | WeaponAttachmentFlags.Flashlight
                            | WeaponAttachmentFlags.ExtraGrip
            }
        };

        /// Instantiates world pickups for every slot around the anchor.
        public void SpawnAll(Transform anchor)
        {
            foreach (var slot in slots)
            {
                // TODO(Opus): resolve ItemDefinition.WorldPrefab from a central
                // ItemDatabase, instantiate at shelf positions inside the vault,
                // and stamp the pickup with slot.Attachments so the rifle
                // arrives pre-modified.
                Debug.Log($"[Vault] Spawn {slot.Count}x {slot.Item} @ {anchor.position}");
            }
        }
    }
}
