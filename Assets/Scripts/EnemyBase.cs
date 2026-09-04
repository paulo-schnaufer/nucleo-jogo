// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Comportamento base de inimigo: persegue o alvo mais próximo entre
    /// Jogador e Núcleo, causa dano por contato, e libera um orbe de XP ao
    /// morrer.
    ///
    /// ESCOPO DESTA SESSÃO: implementa só o padrão "rusher" (perseguição +
    /// contato corpo a corpo), que já cobre o inimigo tipo "loop infinito"
    /// do SCOPE_LOCK.md. Os outros 3 tipos (atirador/DDoS, tanque/memory
    /// leak, boss/stack overflow) devem herdar desta classe e sobrescrever
    /// os métodos virtuais abaixo — fica pra próxima sessão.
    ///
    /// DECISÃO DE DESIGN PENDENTE DE CONFIRMAÇÃO: a regra de alvo aqui é
    /// "sempre persegue o mais próximo entre jogador e Núcleo", igual pros
    /// 4 tipos. Pode ser que só certos tipos ameacem o Núcleo (ex.: faria
    /// sentido temático o "atirador/DDoS" mirar o Núcleo de propósito, já
    /// que DDoS ataca infraestrutura, não um usuário) — ver aviso na
    /// resposta desta sessão.
    ///
    /// Requer que o Collider2D deste prefab esteja marcado como "Is
    /// Trigger" (dano por contato é detectado via OnTriggerStay2D).
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PoolItem))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] protected float moveSpeed = 2f;

        [Header("Combate")]
        [SerializeField] protected float contactDamage = 10f;
        [SerializeField] protected float contactDamageInterval = 1f;

        [Header("Recompensa")]
        [SerializeField] private GameObject xpOrbPrefab;
        [SerializeField] private int xpValue = 1;

        protected Health _health;
        protected Rigidbody2D _rb;
        protected Transform _currentTarget;
        private float _lastContactDamageTime;

        // Referências estáticas simples pro alvo — atribuídas por
        // PlayerController.Awake() e CoreIntegrity.Awake(). Simplificação
        // intencional pro protótipo: 1 jogador, 1 Núcleo, sem lista de squads.
        public static Transform PlayerTarget;
        public static Transform CoreTarget;

        protected virtual void Awake()
        {
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        protected virtual void OnEnable()
        {
            // Objeto voltou do pool: reseta estado (senão volta morto/com HP zerado).
            _health.ResetHealth();
            _health.OnDeath += HandleDeath;
            _lastContactDamageTime = -999f;
        }

        protected virtual void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        protected virtual void Update()
        {
            _currentTarget = PickNearestTarget();
        }

        protected virtual void FixedUpdate()
        {
            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
            _rb.linearVelocity = dir * moveSpeed;
        }

        /// <summary>
        /// Escolhe o alvo mais próximo entre jogador e Núcleo. Sobrescreva
        /// pra tipos com prioridade fixa (ex.: um "atirador" que sempre
        /// mira o Núcleo, ignorando o jogador).
        /// </summary>
        protected virtual Transform PickNearestTarget()
        {
            if (PlayerTarget == null) return CoreTarget;
            if (CoreTarget == null) return PlayerTarget;

            float distToPlayer = Vector2.SqrMagnitude(PlayerTarget.position - transform.position);
            float distToCore = Vector2.SqrMagnitude(CoreTarget.position - transform.position);
            return distToPlayer <= distToCore ? PlayerTarget : CoreTarget;
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time - _lastContactDamageTime < contactDamageInterval) return;

            // Filtra pra só o jogador/Núcleo tomarem dano — evita dano acidental
            // entre inimigos mesmo que a Collision Matrix permita a sobreposição.
            if (other.transform != PlayerTarget && other.transform != CoreTarget) return;

            var targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;

            targetHealth.TakeDamage(contactDamage, gameObject);
            _lastContactDamageTime = Time.time;
        }

        /// <summary>Chame de fora (ex.: Projectile) pra aplicar dano a este inimigo.</summary>
        public void ApplyDamage(float amount, GameObject source = null)
        {
            _health.TakeDamage(amount, source);
        }

        private void HandleDeath()
        {
            SpawnXPOrb();
            GetComponent<PoolItem>().ReturnToPool();
        }

        private void SpawnXPOrb()
        {
            if (xpOrbPrefab == null || ObjectPool.Instance == null) return;
            var orbGO = ObjectPool.Instance.Get(xpOrbPrefab, transform.position, Quaternion.identity);
            var xpOrb = orbGO.GetComponent<XPOrb>();
            if (xpOrb != null) xpOrb.SetValue(xpValue);
        }
    }
}
