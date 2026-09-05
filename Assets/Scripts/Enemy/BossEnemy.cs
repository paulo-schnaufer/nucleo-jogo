// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;
using UnityEngine.Events;

namespace Nucleo
{
    /// <summary>
    /// Boss — reskin "stack overflow". Único padrão de ataque telegrafado (SCOPE_LOCK: onda
    /// 6, "boss com 1 padrão de ataque telegrafado").
    /// Reaproveita a escolha de alvo padrão de EnemyBase (persegue o mais próximo entre
    /// jogador e Núcleo — sem regra própria em DECISIONS.md pra este tipo) só na fase de
    /// aproximação. Ao entrar no alcance de ataque, para de se mover e roda o ciclo
    /// telegraph -> overflow (dano em área, pode acertar jogador e Núcleo ao mesmo tempo,
    /// já que "overflow" não distingue vítima) -> cooldown -> aproxima de novo.
    /// Sobrescreve FixedUpdate porque o movimento padrão da base (sempre avançar até
    /// encostar) não serve fora da fase de aproximação.
    /// OnTelegraphStart/OnOverflowFire existem pro Animator/Particle System (âmbar, ver
    /// STYLE_GUIDE.md seção 5 — telegraph ÂMBAR-ALERTA) se inscreverem sem acoplar esta
    /// classe a VFX.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        private enum BossPhase { Approaching, Telegraphing, Overflowing, Cooldown }

        [Header("Boss - Stack Overflow")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float telegraphDuration = 1.2f;
        [SerializeField] private float overflowWindow = 0.15f;
        [SerializeField] private float overflowRadius = 3f;
        [SerializeField] private float overflowDamage = 25f;
        [SerializeField] private float cooldownDuration = 1.8f;

        public UnityEvent OnTelegraphStart;
        public UnityEvent OnOverflowFire;

        private BossPhase _phase;
        private float _phaseTimer;

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
            _phase = BossPhase.Approaching;
            _phaseTimer = 0f;
        }

        protected override void FixedUpdate()
        {
            if (_phase == BossPhase.Approaching)
                TickApproaching();
            else
                _rb.linearVelocity = Vector2.zero;

            TickPhaseTimer(Time.fixedDeltaTime);
        }

        private void TickApproaching()
        {
            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(_rb.position, _currentTarget.position);
            if (dist <= attackRange)
            {
                _rb.linearVelocity = Vector2.zero;
                _phase = BossPhase.Telegraphing;
                _phaseTimer = 0f;
                OnTelegraphStart?.Invoke();
            }
            else
            {
                Vector2 dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
                _rb.linearVelocity = dir * moveSpeed;
            }
        }

        private void TickPhaseTimer(float dt)
        {
            switch (_phase)
            {
                case BossPhase.Telegraphing:
                    _phaseTimer += dt;
                    if (_phaseTimer >= telegraphDuration) BeginOverflow();
                    break;
                case BossPhase.Overflowing:
                    _phaseTimer += dt;
                    if (_phaseTimer >= overflowWindow) BeginCooldown();
                    break;
                case BossPhase.Cooldown:
                    _phaseTimer += dt;
                    if (_phaseTimer >= cooldownDuration) _phase = BossPhase.Approaching;
                    break;
            }
        }

        private void BeginOverflow()
        {
            _phase = BossPhase.Overflowing;
            _phaseTimer = 0f;
            OnOverflowFire?.Invoke();

            if (PlayerTarget != null && Vector2.Distance(_rb.position, PlayerTarget.position) <= overflowRadius)
                PlayerTarget.GetComponent<Health>()?.TakeDamage(overflowDamage, gameObject);

            if (CoreTarget != null && Vector2.Distance(_rb.position, CoreTarget.position) <= overflowRadius)
                CoreTarget.GetComponent<Health>()?.TakeDamage(overflowDamage, gameObject);
        }

        private void BeginCooldown()
        {
            _phase = BossPhase.Cooldown;
            _phaseTimer = 0f;
        }
    }
}
