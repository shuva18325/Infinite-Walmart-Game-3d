// ============================================================================
// SYSTEM 8a — ATMOSPHERE LIGHTING MANAGER
// The visual director's console. Owns the store-wide lighting MOOD as a
// state derived from (ShiftPhase × PowerState):
//   Morning + Power   → OpenBright     : flat, sterile retail glare.
//   Night   + Power   → ClosedDim      : sparse banks lit, long dark gaps —
//                                        the signature look: islands of
//                                        flickering light in pitch black.
//   Any     + Blackout→ BlackoutDark   : emergency reds only. NVG territory.
// Individual fixtures (FlickeringFluorescent) subscribe to the mood; this
// manager never touches lights directly — it broadcasts intent.
// ============================================================================

using System;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Rendering
{
    public enum LightingMood { OpenBright, ClosedDim, BlackoutDark }

    public class AtmosphereLightingManager : MonoBehaviour
    {
        public static AtmosphereLightingManager Instance { get; private set; }

        [Header("Global Rendering Targets — Opus wires to render pipeline")]
        [SerializeField] private float openAmbientIntensity = 1.0f;
        [SerializeField] private float closedAmbientIntensity = 0.08f;  // Near-black gaps.
        [SerializeField] private float blackoutAmbientIntensity = 0.02f;

        [Header("Night Fixture Culling")]
        [Tooltip("Fraction of fixtures that stay ON at night. The rest go dark — high contrast is the whole aesthetic.")]
        [Range(0f, 1f)] [SerializeField] private float nightLitFixtureFraction = 0.3f;

        public LightingMood Mood { get; private set; } = LightingMood.OpenBright;

        /// Fixtures, post-processing, and ambience beds all subscribe here.
        public event Action<LightingMood> MoodChanged;

        /// Deterministic per-fixture selection input (see FlickeringFluorescent).
        public float NightLitFraction => nightLitFixtureFraction;

        private ShiftPhase _shift = ShiftPhase.MorningShift;
        private PowerState _power = PowerState.PoweredOn;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.ShiftChanged += OnShift;
            GameEventBus.PowerStateChanged += OnPower;
        }

        private void OnDisable()
        {
            GameEventBus.ShiftChanged -= OnShift;
            GameEventBus.PowerStateChanged -= OnPower;
        }

        private void OnShift(ShiftPhase s) { _shift = s; Recompute(); }
        private void OnPower(PowerState p) { _power = p; Recompute(); }

        private void Recompute()
        {
            var next = _power == PowerState.Blackout
                ? LightingMood.BlackoutDark
                : _shift == ShiftPhase.MorningShift
                    ? LightingMood.OpenBright
                    : LightingMood.ClosedDim;

            if (next == Mood) return;
            Mood = next;

            ApplyGlobalAmbient(next);
            MoodChanged?.Invoke(next);
        }

        private void ApplyGlobalAmbient(LightingMood mood)
        {
            RenderSettings.ambientIntensity = mood switch
            {
                LightingMood.OpenBright   => openAmbientIntensity,
                LightingMood.ClosedDim    => closedAmbientIntensity,
                _                         => blackoutAmbientIntensity
            };
            // TODO(Opus): crossfade instead of snap; post-processing volume
            // blend (desaturation + vignette at night, red tint in blackout);
            // fog density per mood. NightVisionGoggles hook: when equipped,
            // swap in the amplification post-profile instead of the mood one.
        }
    }
}
