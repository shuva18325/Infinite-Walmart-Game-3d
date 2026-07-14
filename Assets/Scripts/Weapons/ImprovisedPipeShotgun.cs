// ============================================================================
// SYSTEM 5b — THE IMPROVISED PIPE SHOTGUN
// The early-game equalizer, built from four baseline scrap components:
//   Hardware Bolts + Gunpowder Container + Salvaged Metal Pipe
//   + Wooden Display Handle
// Identity in three numbers: MASSIVE close blast, terrible spread, and a
// reload so long you plan your life around it.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Weapons
{
    public class ImprovisedPipeShotgun : WeaponStateMachine
    {
        /// Canonical scrap recipe — CraftingRecipe assets mirror this. Kept in
        /// code as the authoritative baseline so the recipe asset and weapon
        /// can never silently diverge.
        public static readonly (ItemId item, int count)[] Recipe =
        {
            (ItemId.HardwareBolts,       4),
            (ItemId.GunpowderContainer,  1),
            (ItemId.SalvagedMetalPipe,   2),
            (ItemId.WoodenDisplayHandle, 1)
        };

        private void Reset()
        {
            // Archetype defaults — Opus balances, but the SHAPE is fixed:
            handling = new WeaponHandlingParams
            {
                DamagePerProjectile = 12f,
                ProjectilesPerShot  = 10,     // 120 potential at point blank...
                EffectiveRange      = 6f,     // ...gone beyond one aisle width.
                SpreadDegrees       = 18f,    // Extreme pellet spread.
                FireCooldown        = 1.2f,
                ReloadStepDuration  = 2.5f,   // Lengthy manual loop:
                ReloadStepCount     = 2,      // powder, then shot. 5s exposed.
                MagazineSize        = 2,
                AmmoTier            = AmmoTier.Improvised,
                GunshotNoiseRadius  = 45f,    // Deafening. Everything comes.
                SilencedNoiseRadius = 45f     // Cannot be silenced. Ever.
            };
        }

        protected override void EmitProjectiles(Vector3 origin, Vector3 aimDirection)
        {
            for (int i = 0; i < handling.ProjectilesPerShot; i++)
            {
                // TODO(Opus): random unit vector inside SpreadDegrees cone,
                // raycast pellet, apply DamagePerProjectile with hard falloff
                // past EffectiveRange. Recoil kick + smoke burst VFX.
            }
        }

        public override void BeginReload()
        {
            base.BeginReload();
            // The reload itself is audible — fumbling shells into a pipe.
            if (State == WeaponState.ManualReloading)
                GameEventBus.RaiseNoise(new NoiseEvent(
                    transform.position, 8f, 0.3f, "ShotgunReload"));
        }
    }
}
