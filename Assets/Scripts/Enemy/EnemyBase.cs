// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Comportamento base de inimigo: persegue o alvo mais próximo entre
    /// Jogador e Núcleo, causa dano por contato, e libera um orbe de XP ao
    /// morrer.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PoolItem))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] protected float moveSpeed = 2f;

        [Header("Combate")]
        [SerializeField] protected float contactDamage = 10f;
        [SerializeField] protected float contactDamageInterval = 1f;
        [SerializeField] protected float knockbackForce = 6f;

        [Header("Recompensa")]
        [SerializeField] private GameObject xpOrbPrefab;
        [SerializeField] private int xpValue = 1;

        protected Health _health;
        protected Rigidbody2D _rb;
        protected Transform _currentTarget;
        private float _lastContactDamageTime;

        public static Transform PlayerTarget;
        public static Transform CoreTarget;

        protected virtual void Awake()
        {
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        protected virtual void OnEnable()
        {
            _health.ResetHealth();
            _health.OnDeath += HandleDeath;
            _lastContactDamageTime = -999f;
        }

        protected virtual void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        protected virtual void Update()
        {
            _currentTarget = PickNearestTarget();
        }

        protected virtual void FixedUpdate()
        {
            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
            transform.up = dir;
            _rb.linearVelocity = dir * moveSpeed;
        }

        protected virtual Transform PickNearestTarget()
        {
            if (PlayerTarget == null) return CoreTarget;
            if (CoreTarget == null) return PlayerTarget;

            float distToPlayer = Vector2.SqrMagnitude(PlayerTarget.position - transform.position);
            float distToCore = Vector2.SqrMagnitude(CoreTarget.position - transform.position);
            return distToPlayer <= distToCore ? PlayerTarget : CoreTarget;
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time - _lastContactDamageTime < contactDamageInterval) return;

            if (other.transform.root != PlayerTarget.root && other.transform.root != CoreTarget.root) return;

            var targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;

            // Repassa a posição do inimigo e a força para empurrar o alvo
            targetHealth.TakeDamage(contactDamage, transform.position, knockbackForce, gameObject);
            _lastContactDamageTime = Time.time;
        }

        public void ApplyDamage(float amount, GameObject source = null)
        {
            _health.TakeDamage(amount, source);
        }

        private void HandleDeath()
        {
            SpawnXPOrb();
            GetComponent<PoolItem>().ReturnToPool();
        }

        private void SpawnXPOrb()
        {
            if (xpOrbPrefab == null || ObjectPool.Instance == null) return;
            var orbGO = ObjectPool.Instance.Get(xpOrbPrefab, transform.position, Quaternion.identity);
            var xpOrb = orbGO.GetComponent<XPOrb>();
            if (xpOrb != null) xpOrb.SetValue(xpValue);
        }
    }
}