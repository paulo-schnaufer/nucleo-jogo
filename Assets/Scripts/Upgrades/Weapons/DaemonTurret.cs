using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Torreta automática — metáfora "Daemon": processo em segundo plano que roda
    /// sozinho, sem input do jogador, igual um daemon de SO. Mira e atira automaticamente
    /// no inimigo mais próximo dentro do alcance.
    ///
    /// Uso dos dados de nível: primaryValue = cadência de tiro (s entre disparos),
    /// secondaryValue = dano por disparo.
    ///
    /// O prefab de projétil usa o contrato ativo Projectile.Launch.
    /// </summary>
    public class DaemonTurret : MonoBehaviour, IUpgradableWeapon
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range = 6f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float fireCooldown = 0.5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float projectileSpeed = 12f;

        private float _fireCooldown;
        private float _damage;
        private float _timer;

        public void ApplyLevel(int level, UpgradeLevelData data)
        {
            _fireCooldown = data.primaryValue > 0f ? data.primaryValue : fireCooldown;
            _damage = data.secondaryValue > 0f ? data.secondaryValue : damage;
        }

        private void Awake()
        {
            ApplyLevel(1, new UpgradeLevelData());
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            Transform target = TargetingUtils.FindNearestEnemy(transform.position, range, enemyLayer, gameObject);
            if (target == null) return;

            Fire(target);
            _timer = _fireCooldown;
        }

        private void Fire(Transform target)
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;

            Vector2 direction = (target.position - transform.position).normalized;
            var projectileGO = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
            projectileGO.GetComponent<Projectile>()?.Launch(
                direction,
                projectileSpeed,
                _damage,
                gameObject
            );
        }
    }
}
