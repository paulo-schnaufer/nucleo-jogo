// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening; // Adicionado para os efeitos visuais do boss

namespace Nucleo
{
    public class BossEnemy : EnemyBase
    {
        private enum BossPhase { Approaching, Telegraphing, Overflowing, Cooldown }

        [Header("Boss - Stack Overflow")]
        [SerializeField] private AudioClip bossMusicClip;
        [SerializeField] private float attackRange = 2.8f;
        [SerializeField] private float telegraphDuration = 1.0f;
        [SerializeField] private float overflowWindow = 0.2f;
        [SerializeField] private float overflowRadius = 3.8f;
        [SerializeField] private float overflowDamage = 25f;
        [SerializeField] private float cooldownDuration = 1.2f;

        public UnityEvent OnTelegraphStart;
        public UnityEvent OnOverflowFire;

        private BossPhase _phase;
        private float _phaseTimer;

        [Header("Boss - Zoom de Câmera")]
        [SerializeField] private float bossCamSize = 8.5f;
        [SerializeField] private float zoomDuration = 1.5f;

        protected override void OnEnable()
        {
            base.OnEnable();
            _phase = BossPhase.Approaching;
            _phaseTimer = 0f;

            if (bossMusicClip != null && AudioManager.Instance != null)
                AudioManager.Instance.ChangeMusic(bossMusicClip);

            if (CameraZoom.Instance != null)
                CameraZoom.Instance.SetZoom(bossCamSize, zoomDuration);
        }

        protected override void Update()
        {
            base.Update();
            LookAtTarget(); // Rotação contínua voltada para o jogador/núcleo
        }

        protected override void FixedUpdate()
        {
            if (_phase == BossPhase.Approaching)
                TickApproaching();
            else
                _rb.linearVelocity = Vector2.zero;

            TickPhaseTimer(Time.fixedDeltaTime);
        }

        private void LookAtTarget()
        {
            if (_currentTarget == null) return;
            Vector2 dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
            if (dir != Vector2.zero)
                transform.up = dir;
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
                
                // Animação de "carregando ataque": o boss treme levemente enquanto telegrafa
                transform.DOShakePosition(telegraphDuration, 0.1f);
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

            // IMPACTO VISUAL: Treme a câmera do jogo ao disparar o ataque
            if (Camera.main != null)
                Camera.main.transform.DOShakePosition(0.3f, 0.6f);

            // Animação do Boss "pulsando" na hora da explosão
            transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);

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