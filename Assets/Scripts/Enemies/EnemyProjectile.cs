using UnityEngine;
using Nucleo.Core;

namespace Nucleo.Enemies
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
            var health = other.GetComponentInParent<Health>();
            if (health == null) return;

            bool isPlayer = EnemyBase.PlayerTarget != null && health.transform == EnemyBase.PlayerTarget;
            bool isCore = EnemyBase.CoreTarget != null && health.transform == EnemyBase.CoreTarget;

            if (!isPlayer && !isCore) return;

            health.TakeDamage(_damage, transform.position, knockbackForce, gameObject);
            _poolItem.ReturnToPool();
        }
    }
}