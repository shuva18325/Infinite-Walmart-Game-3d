// ============================================================================
// SYSTEM 8b — FLICKERING FLUORESCENT FIXTURE
// One per ceiling tube bank. Mood-reactive:
//   OpenBright   → steady on (rare courtesy flicker for texture).
//   ClosedDim    → deterministic lottery: ~30% stay on WITH heavy flicker,
//                  the rest go dead black. Same fixture, same result, every
//                  night — players learn the map by its light islands.
//   BlackoutDark → dead (emergency fixtures are a separate prefab).
// ============================================================================

using UnityEngine;

namespace InfiniteWalmart.Rendering
{
    [RequireComponent(typeof(Light))]
    public class FlickeringFluorescent : MonoBehaviour
    {
        [Header("Flicker Character — Opus shapes the curves")]
        [SerializeField] private float flickerMinIntensity = 0.1f;
        [SerializeField] private float flickerMaxIntensity = 1.1f;
        [Tooltip("Buzz audio + emissive material sync with the light — one flicker source drives all three.")]
        [SerializeField] private Renderer tubeRenderer;
        [SerializeField] private AudioSource buzzSource;

        private Light _light;
        private bool _litAtNight;
        private bool _flickering;

        private void Awake()
        {
            _light = GetComponent<Light>();

            // Deterministic night lottery — position-hashed so layout streaming
            // reproduces the identical light pattern on floor revisit.
            float hash = Mathf.Abs(Mathf.Sin(
                transform.position.x * 12.9898f + transform.position.z * 78.233f) * 43758.5453f) % 1f;
            _litAtNight = hash < (AtmosphereLightingManager.Instance?.NightLitFraction ?? 0.3f);
        }

        private void OnEnable()
        {
            if (AtmosphereLightingManager.Instance != null)
            {
                AtmosphereLightingManager.Instance.MoodChanged += ApplyMood;
                ApplyMood(AtmosphereLightingManager.Instance.Mood);
            }
        }

        private void OnDisable()
        {
            if (AtmosphereLightingManager.Instance != null)
                AtmosphereLightingManager.Instance.MoodChanged -= ApplyMood;
        }

        private void ApplyMood(LightingMood mood)
        {
            switch (mood)
            {
                case LightingMood.OpenBright:
                    SetLit(true, flicker: false);
                    break;
                case LightingMood.ClosedDim:
                    SetLit(_litAtNight, flicker: _litAtNight);
                    break;
                case LightingMood.BlackoutDark:
                    SetLit(false, flicker: false);
                    break;
            }
        }

        private void SetLit(bool lit, bool flicker)
        {
            _light.enabled = lit;
            _flickering = flicker;
            if (buzzSource != null) buzzSource.enabled = lit;
            // TODO(Opus): emissive material swap on tubeRenderer to match.
        }

        private void Update()
        {
            if (!_flickering) return;
            // TODO(Opus): replace placeholder noise with a sputter pattern —
            // long stable holds broken by rapid stutter bursts; sync buzz
            // pitch dip and emissive intensity to the same signal.
            _light.intensity = Random.Range(flickerMinIntensity, flickerMaxIntensity);
        }
    }
}
