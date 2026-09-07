using UnityEngine;
using Nucleo.Player;
using Nucleo.Core;
using Nucleo.Combat;

namespace Nucleo
{
    public class AutoTurretWeapon : MonoBehaviour
    {
        [Header("Disparo")]
        [SerializeField] private float fireRate = 2f; 
        [SerializeField] private float range = 6f;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private GameObject projectilePrefab;

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

            Fire(target);
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

        private void Fire(Transform target)
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;

            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            var projGO = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);

            float dmgMultiplier = _ownerStats != null ? _ownerStats.DamageMultiplier : 1f;
            Debug.Log($"[{name}] dmgMultiplier={dmgMultiplier} baseDamage={baseDamage} final={baseDamage * dmgMultiplier}");
            var proj = projGO.GetComponent<Projectile>();
            proj.Launch(dir, projectileSpeed, baseDamage * dmgMultiplier, gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}