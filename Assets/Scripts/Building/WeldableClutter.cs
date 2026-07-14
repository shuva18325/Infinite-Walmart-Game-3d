// ============================================================================
// SYSTEM 7b — WELDABLE CLUTTER + BARRICADE DURABILITY FRAMEWORK
// Attach WeldableClutter to any store furniture prefab (heavy gondola
// shelving, clothing racks, display cases). Lifecycle:
//   LOOSE → CARRIED → WELDED (BarricadeDurability activates)
//         → DESTROYED (AI broke it; navmesh cut removed; bus notified)
// Heavier archetypes get bigger health pools — the material tier IS the
// balance knob.
// ============================================================================

using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Building
{
    [RequireComponent(typeof(Rigidbody))]
    public class WeldableClutter : MonoBehaviour
    {
        public enum ClutterArchetype
        {
            ClothingRack,       // Light. Fast to place, paper-thin.
            DisplayCase,        // Medium.
            GondolaShelving     // Heavy metal. The real wall.
        }

        [Header("Archetype")]
        [SerializeField] private ClutterArchetype archetype = ClutterArchetype.ClothingRack;

        [Header("Durability Pools per Archetype — Opus balances")]
        [SerializeField] private float clothingRackHealth = 40f;
        [SerializeField] private float displayCaseHealth = 90f;
        [SerializeField] private float gondolaHealth = 220f;

        public bool IsWelded { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        private Rigidbody _body;
        private Transform _carryAnchor;

        private void Awake() => _body = GetComponent<Rigidbody>();

        // ------------------------------------------------------------ CARRYING
        public void EnterCarryState(Transform anchor)
        {
            _carryAnchor = anchor;
            _body.isKinematic = true;
            // TODO(Opus): collision layer swap to 'Carried' (no AI blocking while held).
        }

        public void ExitCarryState()
        {
            _carryAnchor = null;
            _body.isKinematic = false;
        }

        public void RotateStep(Vector3 axis, float degrees)
            => transform.Rotate(axis, degrees, Space.World);

        private void LateUpdate()
        {
            if (_carryAnchor != null && !IsWelded)
                transform.position = _carryAnchor.position;
            // TODO(Opus): replace hard-snap with damped follow + obstruction clamp.
        }

        // -------------------------------------------------------------- WELDED
        public void Weld()
        {
            IsWelded = true;
            _carryAnchor = null;
            _body.isKinematic = true;

            MaxHealth = archetype switch
            {
                ClutterArchetype.GondolaShelving => gondolaHealth,
                ClutterArchetype.DisplayCase     => displayCaseHealth,
                _                                => clothingRackHealth
            };
            Health = MaxHealth;

            // TODO(Opus): weld-seam decal at contact points, NavMeshObstacle
            // carve ON so employee pathing must route around or attack through.
        }

        // ---------------------------------------------- DURABILITY (AI attacks)
        /// Called by EmployeeEntity.TickBarricadeAttack. Each hit is audible —
        /// the player HEARS their walls failing from across the store.
        public void TakeBarricadeDamage(float amount, Vector3 hitPoint)
        {
            if (!IsWelded) return;

            Health -= amount;
            GameEventBus.RaiseNoise(new NoiseEvent(hitPoint, 20f, 0.6f, "BarricadeImpact"));
            // TODO(Opus): shake + dent visual at damage thresholds (75/50/25%).

            if (Health <= 0f) Break();
        }

        private void Break()
        {
            GameEventBus.RaiseBarricadeDestroyed(transform.position);
            // TODO(Opus): debris burst, navmesh obstacle release, then despawn.
            Destroy(gameObject);
        }
    }
}
