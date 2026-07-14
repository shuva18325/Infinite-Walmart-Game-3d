// ============================================================================
// SYSTEM 8c — SHADOW MODEL SYSTEM (performance silhouettes)
// The optimization trick that IS an aesthetic: beyond a distance threshold,
// full-detail objects and entities swap to ultra-low-poly, unlit, pure-black
// 'shadow models'. Down a dark aisle you see a SHAPE — cheap to render,
// impossible to read, maximally creepy. Detail streams back in on approach.
//
//   ShadowModelSystem  — central distance manager (single Update, no per-
//                        object Update cost; staggered batch evaluation).
//   ShadowModelProxy   — per-object component holding the two renderer sets.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace InfiniteWalmart.Rendering
{
    public class ShadowModelSystem : MonoBehaviour
    {
        public static ShadowModelSystem Instance { get; private set; }

        [Header("Swap Distances — Opus tunes per mood")]
        [Tooltip("Beyond this: silhouette. Within: full detail.")]
        [SerializeField] private float detailDistance = 18f;
        [Tooltip("Hysteresis so objects at the boundary don't strobe between LODs.")]
        [SerializeField] private float hysteresis = 3f;

        [Header("Batching")]
        [Tooltip("Proxies evaluated per frame — spreads the cost across frames.")]
        [SerializeField] private int evaluationsPerFrame = 50;

        private readonly List<ShadowModelProxy> _proxies = new();
        private Transform _viewer;   // Player camera.
        private int _cursor;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SetViewer(Transform viewer) => _viewer = viewer;
        public void Register(ShadowModelProxy proxy) => _proxies.Add(proxy);
        public void Unregister(ShadowModelProxy proxy) => _proxies.Remove(proxy);

        private void Update()
        {
            if (_viewer == null || _proxies.Count == 0) return;

            // Round-robin batch — total cost is flat regardless of scene size.
            int count = Mathf.Min(evaluationsPerFrame, _proxies.Count);
            for (int i = 0; i < count; i++)
            {
                _cursor = (_cursor + 1) % _proxies.Count;
                Evaluate(_proxies[_cursor]);
            }
        }

        private void Evaluate(ShadowModelProxy proxy)
        {
            float dist = Vector3.Distance(_viewer.position, proxy.transform.position);

            // Hysteresis band: must cross detailDistance + hysteresis to shadow,
            // come back inside detailDistance to detail.
            if (proxy.IsShadow && dist < detailDistance)
                proxy.SetShadow(false);
            else if (!proxy.IsShadow && dist > detailDistance + hysteresis)
                proxy.SetShadow(true);
        }
    }

    /// Attach to any prefab that should silhouette at distance: shelving,
    /// clutter, and CRUCIALLY employee entities — a Greeter at 40m is just
    /// a person-shaped void at the end of the aisle.
    public class ShadowModelProxy : MonoBehaviour
    {
        [Header("Renderer Sets")]
        [SerializeField] private GameObject detailModel;
        [Tooltip("Ultra-low-poly mesh, unlit pure-black material, no shadows cast/received, no animator — near-zero cost.")]
        [SerializeField] private GameObject shadowModel;

        public bool IsShadow { get; private set; }

        private void OnEnable()
        {
            ShadowModelSystem.Instance?.Register(this);
            SetShadow(true); // Spawn cheap; system promotes on approach.
        }

        private void OnDisable() => ShadowModelSystem.Instance?.Unregister(this);

        public void SetShadow(bool shadow)
        {
            IsShadow = shadow;
            if (detailModel != null) detailModel.SetActive(!shadow);
            if (shadowModel != null) shadowModel.SetActive(shadow);
            // TODO(Opus): for employee entities, keep the NavMeshAgent live but
            // disable Animator + skinned mesh in shadow mode; a 1-bone bob on
            // the silhouette sells movement for ~free.
        }
    }
}
