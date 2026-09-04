using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Tiro em leque — metáfora "Fork()": a chamada de sistema que duplica um processo
    /// em vários; aqui, um disparo "forka" em N projéteis abertos em leque a partir da
    /// direção do inimigo mais próximo.
    ///
    /// Uso dos dados de nível: primaryValue = número de projéteis do leque,
    /// secondaryValue = dano por projétil. Ângulo de abertura fixo no Inspector.
    ///
    /// O prefab de projétil usa o contrato ativo Projectile.Launch.
    /// </summary>
    public class ForkShot : MonoBehaviour, IUpgradableWeapon
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range = 6f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float spreadAngleDeg = 40f;
        [SerializeField] private float fireCooldown = 1f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float damage = 5f;

        private int _projectileCount;
        private float _damage;
        private float _timer;

        public void ApplyLevel(int level, UpgradeLevelData data)
        {
            _projectileCount = data.primaryValue > 0f
                ? Mathf.Max(2, Mathf.RoundToInt(data.primaryValue))
                : Mathf.Max(2, projectileCount);
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

            FireFan(target);
            _timer = fireCooldown;
        }

        private void FireFan(Transform target)
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;

            Vector2 centerDir = (target.position - transform.position).normalized;
            float centerAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float startAngle = centerAngle - spreadAngleDeg * 0.5f;
            float step = _projectileCount > 1 ? spreadAngleDeg / (_projectileCount - 1) : 0f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = (startAngle + step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                var projectileGO = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                projectileGO.GetComponent<Projectile>()?.Launch(
                    dir,
                    projectileSpeed,
                    _damage,
                    gameObject
                );
            }
        }
    }
}
