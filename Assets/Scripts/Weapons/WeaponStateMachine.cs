// ============================================================================
// SYSTEM 5a — WEAPON STATE MACHINE (base)
// Shared fire/reload flow for every ranged weapon. Handling characteristics
// live in a serializable parameter block so tuning is data, not code.
// Firing always broadcasts a NoiseEvent — the Silencer attachment flag is
// the ONLY thing that shrinks it (this is why the vault rifle matters).
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Weapons
{
    public enum WeaponState
    {
        Idle,
        Firing,
        ManualReloading,   // Per-shell / per-step reload loops live here.
        Jammed
    }

    /// The handling parameter data block — one per weapon archetype.
    [System.Serializable]
    public struct WeaponHandlingParams
    {
        [Header("Damage")]
        public float DamagePerProjectile;
        public int ProjectilesPerShot;      // 1 = rifle, >1 = shotgun pellets.
        public float EffectiveRange;         // Damage falloff start.

        [Header("Accuracy")]
        [Range(0f, 45f)] public float SpreadDegrees;

        [Header("Timing")]
        public float FireCooldown;
        public float ReloadStepDuration;     // One shell / one mag step.
        public int ReloadStepCount;          // Steps to full reload.

        [Header("Magazine")]
        public int MagazineSize;
        public AmmoTier AmmoTier;

        [Header("Audio Signature")]
        public float GunshotNoiseRadius;
        public float SilencedNoiseRadius;    // Used when Silencer flag present.
    }

    public abstract class WeaponStateMachine : MonoBehaviour
    {
        [SerializeField] protected WeaponHandlingParams handling;
        [SerializeField] protected WeaponAttachmentFlags attachments;

        public WeaponState State { get; protected set; } = WeaponState.Idle;
        public int RoundsLoaded { get; protected set; }

        protected float StateClock;
        protected int ReloadStepsDone;

        public bool HasAttachment(WeaponAttachmentFlags flag) => (attachments & flag) != 0;

        /// Applied when picking up pre-modified instances (vault rifle).
        public void SetAttachments(WeaponAttachmentFlags flags) => attachments = flags;

        protected virtual void Update()
        {
            StateClock += Time.deltaTime;
            switch (State)
            {
                case WeaponState.Firing:
                    if (StateClock >= handling.FireCooldown) Transition(WeaponState.Idle);
                    break;

                case WeaponState.ManualReloading:
                    TickReloadLoop();
                    break;
            }
        }

        // --------------------------------------------------------------- FIRE
        public virtual bool TryFire(Vector3 origin, Vector3 aimDirection)
        {
            if (State != WeaponState.Idle || RoundsLoaded <= 0) return false;

            Transition(WeaponState.Firing);
            RoundsLoaded--;

            EmitProjectiles(origin, aimDirection);
            EmitGunshotNoise(origin);
            return true;
        }

        protected abstract void EmitProjectiles(Vector3 origin, Vector3 aimDirection);
        // TODO(Opus): projectile spawning, spread cone math from
        // handling.SpreadDegrees, hit resolution, damage falloff curves.

        protected void EmitGunshotNoise(Vector3 origin)
        {
            float radius = HasAttachment(WeaponAttachmentFlags.Silencer)
                ? handling.SilencedNoiseRadius
                : handling.GunshotNoiseRadius;

            GameEventBus.RaiseNoise(new NoiseEvent(origin, radius, 1f, "Gunshot"));
        }

        // ------------------------------------------------------------- RELOAD
        public virtual void BeginReload()
        {
            if (State != WeaponState.Idle) return;
            if (RoundsLoaded >= EffectiveMagazineSize()) return;
            ReloadStepsDone = 0;
            Transition(WeaponState.ManualReloading);
        }

        /// Step-wise loop: each step loads a shell/segment. Interruptible —
        /// cancelling keeps whatever was already loaded (survival tension).
        protected virtual void TickReloadLoop()
        {
            if (StateClock < handling.ReloadStepDuration) return;
            StateClock = 0f;
            ReloadStepsDone++;

            // TODO(Opus): consume ammo of handling.AmmoTier from inventory here.
            RoundsLoaded = Mathf.Min(EffectiveMagazineSize(), RoundsLoaded + 1);

            if (ReloadStepsDone >= handling.ReloadStepCount
                || RoundsLoaded >= EffectiveMagazineSize())
                Transition(WeaponState.Idle);
        }

        public void CancelReload()
        {
            if (State == WeaponState.ManualReloading) Transition(WeaponState.Idle);
        }

        protected int EffectiveMagazineSize()
            => HasAttachment(WeaponAttachmentFlags.DrumMag)
                ? handling.MagazineSize * 2
                : handling.MagazineSize;

        protected void Transition(WeaponState next)
        {
            State = next;
            StateClock = 0f;
        }
    }
}
