// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Atirador — reskin "DDoS".
    /// Decisão TRAVADA em DECISIONS.md: mira o Núcleo de propósito, em vez do jogador (DDoS
    /// ataca infraestrutura, não o cliente). Por isso sobrescreve PickNearestTarget() pra
    /// sempre devolver CoreTarget, ignorando a distância até o jogador.
    ///
    /// Também sobrescreve FixedUpdate(): o padrão da EnemyBase sempre avança até encostar no
    /// alvo (correto pro Rusher, corpo a corpo); o Atirador precisa parar numa banda de
    /// distância do Núcleo e atirar, em vez de colidir com ele.
    ///
    /// Recomendação de tuning no prefab: contactDamage = 0 (o dano dele vem dos projéteis,
    /// não de contato — evita dano duplicado se ele acabar encostando no Núcleo).
    /// </summary>
    public class AtiradorEnemy : EnemyBase
    {
        [Header("Atirador - DDoS")]
        [SerializeField] private float minRange = 3.5f;
        [SerializeField] private float maxRange = 6f;
        [SerializeField] private float burstCooldown = 2.2f;
        [SerializeField] private int projectilesPerBurst = 3;
        [SerializeField] private float burstSpreadAngle = 20f;
        [SerializeField] private float projectileDamage = 5f;
        [SerializeField] private GameObject projectilePrefab;

        private float _burstTimer;
        [SerializeField] AudioClip hitClip;
        [SerializeField] float pitchMin = 0.45f, pitchMax = 0.60f; // por tipo de inimigo
        AudioSource src;

        void PlayHit() {
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.PlayOneShot(hitClip);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _burstTimer = Random.Range(0f, burstCooldown); // dessincroniza vários atiradores entre si
        }

        protected override Transform PickNearestTarget()
        {
            return CoreTarget; // regra travada em DECISIONS.md — não considera PlayerTarget
        }

        protected override void FixedUpdate()
        {
            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(_rb.position, _currentTarget.position);
            Vector2 dirToTarget = ((Vector2)_currentTarget.position - _rb.position).normalized;
            transform.up = dirToTarget;

            if (dist > maxRange)
                _rb.linearVelocity = dirToTarget * moveSpeed;
            else if (dist < minRange)
                _rb.linearVelocity = -dirToTarget * moveSpeed; // recua, não quer contato
            else
                _rb.linearVelocity = Vector2.zero; // dentro da banda de tiro: parado

            _burstTimer -= Time.fixedDeltaTime;
            if (_burstTimer <= 0f && dist <= maxRange)
            {
                _burstTimer = burstCooldown;
                FireBurst(_currentTarget.position);
            }
        }

        private void FireBurst(Vector2 aimPoint)
        {
            Vector2 baseDir = (aimPoint - _rb.position).normalized;
            float startAngle = -burstSpreadAngle * 0.5f;
            float step = projectilesPerBurst > 1 ? burstSpreadAngle / (projectilesPerBurst - 1) : 0f;

            for (int i = 0; i < projectilesPerBurst; i++)
            {
                float angle = startAngle + step * i;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;

                // Mesmo padrão de pooling usado em EnemyBase.SpawnXPOrb: ObjectPool.Instance.Get
                // devolve o GameObject; devolução ao pool é responsabilidade do próprio projétil
                // (EnemyProjectile.ReturnToPool via PoolItem), nunca Destroy.
                GameObject go = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                if (go.TryGetComponent(out EnemyProjectile proj))
                    proj.Launch(dir, projectileDamage);
            }
        }
    }
}
