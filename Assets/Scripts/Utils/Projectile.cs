// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Projétil pooled genérico: usado pela torreta agora e por qualquer
    /// arma futura que dispare em linha reta.
    /// </summary>
    [RequireComponent(typeof(PoolItem))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float knockbackForce = 5f;

        private Rigidbody2D _rb;
        private float _damage;
        private GameObject _owner;
        private float _spawnTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        public void Launch(Vector2 direction, float speed, float damage, GameObject owner)
        {
            _damage = damage;
            _owner = owner;
            _spawnTime = Time.time;
            _rb.linearVelocity = direction.normalized * speed;

            if (direction.sqrMagnitude > 0.001f)
                transform.up = direction;
        }

        private void Update()
        {
            if (Time.time - _spawnTime >= lifetime)
                GetComponent<PoolItem>().ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;

            var health = other.GetComponent<Health>();
            if (health == null) return;

            // Envia o dano, a posição do projétil, a força de empurrão e o dono
            health.TakeDamage(_damage, transform.position, knockbackForce, _owner);
            GetComponent<PoolItem>().ReturnToPool();
        }
    }
}