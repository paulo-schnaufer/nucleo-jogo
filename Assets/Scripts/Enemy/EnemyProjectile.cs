// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    [RequireComponent(typeof(PoolItem))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float knockbackForce = 4f;

        private Vector2 _direction;
        private float _damage;
        private float _elapsed;
        private PoolItem _poolItem;

        private void Awake()
        {
            _poolItem = GetComponent<PoolItem>();
        }

        private void OnEnable()
        {
            _elapsed = 0f;
        }

        public void Launch(Vector2 direction, float damage)
        {
            _direction = direction.normalized;
            _damage = damage;
            transform.up = _direction;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            _elapsed += Time.deltaTime;
            if (_elapsed >= lifetime) _poolItem.ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 1. Busca o Health no objeto colidido ou em seus pais
            var health = other.GetComponentInParent<Health>();
            if (health == null) return;

            // 2. Valida diretamente se o Health encontrado pertence ao Jogador ou ao Núcleo
            bool isPlayer = EnemyBase.PlayerTarget != null && health.transform == EnemyBase.PlayerTarget;
            bool isCore = EnemyBase.CoreTarget != null && health.transform == EnemyBase.CoreTarget;

            if (!isPlayer && !isCore) return;

            // 3. Aplica o dano e retorna o projétil ao pool
            health.TakeDamage(_damage, transform.position, knockbackForce, gameObject);
            _poolItem.ReturnToPool();
        }
    }
}