using UnityEngine;
using Nucleo.Player;
using Nucleo.Core;
using Nucleo.Combat;

namespace Nucleo
{
    public class FanShotWeapon : MonoBehaviour
    {
        [Header("Disparo")]
        [SerializeField] private float fireRate = 1f; 
        [SerializeField] private float range = 6f;
        [Tooltip("Dano POR PROJÉTIL. O leque compensa em dano total (várias flechas), não em dano por flecha.")]
        [SerializeField] private float baseDamage = 6f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Leque")]
        [Tooltip("Quantidade de projéteis disparados por rajada.")]
        [SerializeField] private int projectileCount = 5;
        [Tooltip("Ângulo total (graus) coberto pelo leque, centrado na direção do alvo.")]
        [SerializeField] private float spreadAngleDegrees = 45f;

        [Tooltip("Layer dos inimigos, pra Physics2D.OverlapCircleAll não pegar jogador/Núcleo.")]
        [SerializeField] private LayerMask enemyLayer;

        private float _cooldown;
        private PlayerStats _ownerStats;

        private void Awake()
        {
            _ownerStats = GetComponentInParent<PlayerStats>();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;

            FireFan(target);
            _cooldown = 1f / fireRate;
        }

        private Transform FindNearestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
            Transform nearest = null;
            float nearestSqrDist = float.MaxValue;
            float maxSqrRange = range * range; 

            foreach (var hit in hits)
            {
                var health = hit.GetComponentInParent<Health>();
                if (health == null || health.IsDead) continue;

                Vector2 enemyPos = health.transform.position;
                float sqrDist = (enemyPos - (Vector2)transform.position).sqrMagnitude;

                if (sqrDist > maxSqrRange) continue;

                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = health.transform;
                }
            }
            return nearest;
        }

        private void FireFan(Transform target)
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;
            if (projectileCount <= 0) return;

            Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float dmgMultiplier = _ownerStats != null ? _ownerStats.DamageMultiplier : 1f;
            Debug.Log($"[{name}] dmgMultiplier={dmgMultiplier} baseDamage={baseDamage} final={baseDamage * dmgMultiplier}");
            float finalDamage = baseDamage * dmgMultiplier;

            if (projectileCount == 1)
            {
                SpawnProjectile(baseDir, finalDamage);
                return;
            }

            float startAngle = -spreadAngleDegrees / 2f;
            float angleStep = spreadAngleDegrees / (projectileCount - 1);

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 dir = RotateVector(baseDir, angle);
                SpawnProjectile(dir, finalDamage);
            }
        }

        private void SpawnProjectile(Vector2 dir, float damage)
        {
            var projGO = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
            var proj = projGO.GetComponent<Projectile>();
            proj.Launch(dir, projectileSpeed, damage, gameObject);
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}