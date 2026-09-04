// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Projétil pooled genérico: usado pela torreta agora e por qualquer
    /// arma futura que dispare em linha reta (lâminas orbitais e tiro em
    /// leque provavelmente precisam de variantes próprias depois).
    /// Move-se na direção lançada, aplica dano no primeiro Health que
    /// colidir, e volta pro pool (por tempo de vida ou por impacto).
    ///
    /// Requer Collider2D marcado como "Is Trigger" e Layer
    /// "PlayerProjectile" (ver Collision Matrix na lista de GameObjects).
    /// </summary>
    [RequireComponent(typeof(PoolItem))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D _rb;
        private float _damage;
        private GameObject _owner;
        private float _spawnTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        /// <summary>Chamado pela arma no momento do disparo (ex.: AutoTurretWeapon.Fire).</summary>
        public void Launch(Vector2 direction, float speed, float damage, GameObject owner)
        {
            _damage = damage;
            _owner = owner;
            _spawnTime = Time.time;
            _rb.linearVelocity = direction.normalized * speed;

            if (direction.sqrMagnitude > 0.001f)
                transform.up = direction; // gira o sprite pra apontar na direção do tiro
        }

        private void Update()
        {
            // Time.time fica congelado durante a pausa de upgrade (Time.timeScale = 0),
            // então este projétil não "expira" indevidamente enquanto o jogo está pausado.
            if (Time.time - _spawnTime >= lifetime)
                GetComponent<PoolItem>().ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;

            var health = other.GetComponent<Health>();
            if (health == null) return; // ignora paredes/cenário sem Health

            health.TakeDamage(_damage, _owner);
            GetComponent<PoolItem>().ReturnToPool();
        }
    }
}
