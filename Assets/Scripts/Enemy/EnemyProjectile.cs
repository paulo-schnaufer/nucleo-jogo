// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Projétil disparado pelo Atirador (DDoS) contra o Núcleo. Segue o mesmo padrão de
    /// pooling já usado no projeto (ObjectPool.Instance.Get / PoolItem.ReturnToPool), visto
    /// em EnemyBase.SpawnXPOrb — nunca Instantiate/Destroy em runtime.
    /// O prefab precisa ter um componente PoolItem, como qualquer objeto gerenciado pelo
    /// ObjectPool (mesmo requisito do xpOrbPrefab).
    /// </summary>
    [RequireComponent(typeof(PoolItem))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifetime = 4f;

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
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            _elapsed += Time.deltaTime;
            if (_elapsed >= lifetime) _poolItem.ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Mesmo filtro usado em EnemyBase.OnTriggerStay2D: só jogador/Núcleo tomam dano.
            if (other.transform != EnemyBase.CoreTarget && other.transform != EnemyBase.PlayerTarget) return;

            var health = other.GetComponent<Health>();
            if (health != null) health.TakeDamage(_damage, gameObject);

            _poolItem.ReturnToPool();
        }
    }
}
