// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Torreta automática: um dos 3 upgrades de arma do SCOPE_LOCK.md.
    /// A cada intervalo, procura o inimigo vivo mais próximo dentro do
    /// alcance e dispara um projétil pooled na direção dele.
    ///
    /// Vira prefab (ver lista de GameObjects) referenciado em
    /// UpgradeData.weaponPrefab, instanciado como filho de
    /// Player/WeaponSlots quando o jogador escolhe esse upgrade.
    /// </summary>
    public class AutoTurretWeapon : MonoBehaviour
    {
        [Header("Disparo")]
        [SerializeField] private float fireRate = 2f; // tiros por segundo
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
            // A torreta é instanciada como filha do jogador (ver
            // UpgradeManager.weaponSlotsRoot) — sobe na hierarquia até achar PlayerStats.
            _ownerStats = GetComponentInParent<PlayerStats>();
        }

        private void Update()
        {
            // Guarda extra: sem isso, Update ainda roda com Time.timeScale = 0
            // (pausa de upgrade) e poderia consumir um projétil do pool sem ele
            // realmente se mover (física fica congelada, mas o Get() já teria saído do pool).
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

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<Health>();
                if (health == null || health.IsDead) continue;

                float sqrDist = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = hit.transform;
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