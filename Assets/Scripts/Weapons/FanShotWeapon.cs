// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Tiro em leque: um dos 3 upgrades de arma do SCOPE_LOCK.md (nome de
    /// exibição sugerido nesta sessão: "Fork" — metáfora de um processo que
    /// se bifurca em N filhos, um por projétil do leque).
    ///
    /// Estrutura de cooldown/alvo idêntica à torreta (AutoTurretWeapon.cs);
    /// a diferença é que, em vez de 1 projétil na direção do alvo, dispara
    /// <see cref="projectileCount"/> projéteis espalhados em
    /// <see cref="spreadAngleDegrees"/> graus ao redor dessa direção.
    ///
    /// Vira prefab referenciado em UpgradeData.weaponPrefab, instanciado como
    /// filho de Player/WeaponSlots quando o jogador escolhe esse upgrade
    /// (mesmo fluxo de AutoTurretWeapon — nenhuma mudança em UpgradeManager
    /// ou UpgradeData foi necessária pra isso funcionar).
    /// </summary>
    public class FanShotWeapon : MonoBehaviour
    {
        [Header("Disparo")]
        [SerializeField] private float fireRate = 1f; // rajadas por segundo
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
            // Mesma hierarquia da torreta: instanciada como filha do jogador.
            _ownerStats = GetComponentInParent<PlayerStats>();
        }

        private void Update()
        {
            // Mesma guarda de AutoTurretWeapon: Update roda mesmo com
            // Time.timeScale = 0 (painel de upgrade aberto).
            if (Time.timeScale <= 0f) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;

            FireFan(target);
            _cooldown = 1f / fireRate;
        }

        // Duplicado de AutoTurretWeapon.FindNearestEnemy de propósito: são
        // ~10 linhas, e extrair uma classe-base só pra isso reabriria uma
        // decisão de arquitetura (como as armas se compõem) que não está no
        // escopo desta tarefa — ver observação no STATUS.md desta sessão.
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

        private void FireFan(Transform target)
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;
            if (projectileCount <= 0) return;

            Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float dmgMultiplier = _ownerStats != null ? _ownerStats.DamageMultiplier : 1f;
            float finalDamage = baseDamage * dmgMultiplier;

            // 1 projétil: dispara reto, sem leque (evita divisão por zero no passo de ângulo).
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
