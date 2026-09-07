using System.Collections.Generic;
using UnityEngine;
using Nucleo.Player;

namespace Nucleo
{
    public class OrbitalBladesWeapon : MonoBehaviour
    {
        [Header("Órbita")]
        [SerializeField] private int bladeCount = 3;
        [SerializeField] private float orbitRadius = 1.8f;
        [SerializeField] private float rotationSpeedDegPerSec = 120f;

        [Header("Dano")]
        [SerializeField] private float baseDamage = 4f;
        [Tooltip("Tempo mínimo entre dois hits da MESMA lâmina no MESMO inimigo (evita dano repetido enquanto o collider fica sobreposto).")]
        [SerializeField] private float perEnemyHitCooldown = 0.5f;

        [Header("Visual/Física")]
        [Tooltip("Prefab pequeno com SpriteRenderer + Collider2D (Is Trigger = true) representando 1 lâmina.")]
        [SerializeField] private GameObject bladeVisualPrefab;
        [SerializeField] private LayerMask enemyLayer;

        private readonly List<Transform> _blades = new List<Transform>();

        private readonly Dictionary<(Transform blade, Health enemy), float> _lastHitTime =
            new Dictionary<(Transform, Health), float>();

        private PlayerStats _ownerStats;

        private void Awake()
        {
            _ownerStats = GetComponentInParent<PlayerStats>();
            SpawnBlades();
        }

        private void SpawnBlades()
        {
            if (bladeVisualPrefab == null || bladeCount <= 0) return;

            for (int i = 0; i < bladeCount; i++)
            {
                float angle = (360f / bladeCount) * i;
                Vector2 offset = RotateVector(Vector2.right * orbitRadius, angle);

                var bladeGO = Instantiate(bladeVisualPrefab, transform);
                bladeGO.transform.localPosition = offset;

                var contact = bladeGO.GetComponent<OrbitalBladeContact>();
                if (contact == null) contact = bladeGO.AddComponent<OrbitalBladeContact>();
                contact.Init(this);

                _blades.Add(bladeGO.transform);
            }
        }

        public void AddBlades(int amount)
        {
            bladeCount += amount;
            
            foreach (Transform blade in _blades)
            {
                if (blade != null) Destroy(blade.gameObject);
            }
            _blades.Clear();
            
            SpawnBlades();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            transform.Rotate(Vector3.forward, rotationSpeedDegPerSec * Time.deltaTime);
        }

        public void HandleBladeHit(Transform blade, Collider2D other)
        {
            if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

            var health = other.GetComponent<Health>();
            if (health == null || health.IsDead) return;

            var key = (blade, health);
            if (_lastHitTime.TryGetValue(key, out float lastTime) && Time.time - lastTime < perEnemyHitCooldown)
                return;

            float dmgMultiplier = _ownerStats != null ? _ownerStats.DamageMultiplier : 1f;

            health.TakeDamage(baseDamage * dmgMultiplier, gameObject);

            _lastHitTime[key] = Time.time;
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }

    public class OrbitalBladeContact : MonoBehaviour
    {
        private OrbitalBladesWeapon _owner;

        public void Init(OrbitalBladesWeapon owner) => _owner = owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            _owner?.HandleBladeHit(transform, other);
        }
    }
}