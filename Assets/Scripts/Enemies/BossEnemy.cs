using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Nucleo.Core;
using Nucleo.GameFeel;

namespace Nucleo.Enemies
{
    public class BossEnemy : EnemyBase
    {
        private enum BossPhase { Approaching, Telegraphing, Overflowing, TelegraphingBarrage, Barrage, Cooldown }

        [Header("Boss - Stack Overflow")]
        [SerializeField] private AudioClip bossMusicClip;
        [SerializeField] private float attackRange = 2.8f;
        [SerializeField] private float telegraphDuration = 0.5f;
        [SerializeField] private float overflowWindow = 0.15f;
        [SerializeField] private float overflowRadius = 3.8f;
        [SerializeField] private float overflowDamage = 50f;
        [SerializeField] private float cooldownDuration = 0.6f;

        public UnityEvent OnTelegraphStart;
        public UnityEvent OnOverflowFire;

        [Header("Boss - Movimento Variável (Lunge)")]
        [Tooltip("Velocidade do dash repentino usado pra fechar distância de forma imprevisível. Bem mais alta que o moveSpeed normal, de propósito.")]
        [SerializeField] private float lungeSpeed = 12f;
        [SerializeField] private float lungeDuration = 0.3f;
        [Tooltip("A cada quanto tempo (aprox) o boss tenta um lunge durante a perseguição.")]
        [SerializeField] private float lungeInterval = 1.3f;

        [Header("Boss - Barrage (Espiral de Projéteis)")]
        [Tooltip("Prefab do projétil do boss. Precisa ter EnemyProjectile + PoolItem.")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileDamage = 16f;
        [Tooltip("Quantos tiros a rajada dispara antes de encerrar (base, sem fúria).")]
        [SerializeField] private int barrageShotCount = 20;
        [Tooltip("Intervalo entre cada tiro da rajada, em segundos.")]
        [SerializeField] private float barrageShotInterval = 0.05f;
        [Tooltip("Quantos graus cada projétil gira em relação ao anterior. Cria o padrão de espiral.")]
        [SerializeField] private float spiralAngleStep = 25f;
        [SerializeField] private float telegraphBarrageDuration = 0.45f;
        [Tooltip("Se o boss ficar esse tempo (segundos) sem conseguir alcançar o alvo pra um ataque corpo a corpo, ele interrompe a perseguição e dispara a espiral de onde estiver.")]
        [SerializeField] private float maxTimeWithoutAttack = 1.6f;

        public UnityEvent OnBarrageTelegraphStart;
        public UnityEvent OnBarrageShotFired;

        [Header("Boss - Fúria (HP baixo)")]
        [Tooltip("Fração de HP (0-1) em que o boss entra em fúria: mais rápido, mais projéteis, cooldowns mais curtos.")]
        [SerializeField, Range(0f, 1f)] private float enrageHpThreshold = 0.3f;
        [SerializeField] private float enrageSpeedMultiplier = 1.3f;
        [SerializeField] private float enrageBarrageShotMultiplier = 1.8f;
        [SerializeField] private float enrageCooldownMultiplier = 0.7f;

        public UnityEvent OnEnrageStart;

        [Header("Boss - Feedback Visual do Telegraph")]
        [Tooltip("Se vazio, tenta pegar automaticamente um SpriteRenderer nos filhos.")]
        [SerializeField] private SpriteRenderer bossSprite;
        [SerializeField] private Color telegraphColor = new Color(1f, 0.75f, 0.1f); // âmbar, igual ao resto do jogo

        [Header("Boss - Zoom de Câmera")]
        [SerializeField] private float bossCamSize = 8.5f;
        [SerializeField] private float zoomDuration = 1.5f;

        private BossPhase _phase;
        private float _phaseTimer;
        private float _timeSinceLastAttack;

        private bool _isLunging;
        private float _lungeTimer;
        private float _lungeIntervalTimer;

        private int _barrageShotsFired;
        private float _barrageShotTimer;
        private float _currentSpiralAngle;
        private Vector2 _barrageBaseDir;

        private bool _enraged;
        private Color _baseSpriteColor;

        protected override void Awake()
        {
            base.Awake();
            if (bossSprite == null) bossSprite = GetComponentInChildren<SpriteRenderer>();
            if (bossSprite != null) _baseSpriteColor = bossSprite.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _phase = BossPhase.Approaching;
            _phaseTimer = 0f;
            _timeSinceLastAttack = 0f;
            _isLunging = false;
            _lungeTimer = 0f;
            _lungeIntervalTimer = 0f;
            _barrageShotsFired = 0;
            _barrageShotTimer = 0f;
            _enraged = false;

            if (bossSprite != null) bossSprite.color = _baseSpriteColor;

            if (gameObject.activeInHierarchy && gameObject.scene.isLoaded)
            {
                if (CameraZoom.Instance != null)
                    CameraZoom.Instance.SetZoom(bossCamSize, zoomDuration);

                if (bossMusicClip != null && AudioManager.Instance != null)
                    AudioManager.Instance.ChangeMusic(bossMusicClip);
            }
        }

        protected override void Update()
        {
            base.Update();
            LookAtTarget();
        }

        protected override void FixedUpdate()
        {
            CheckEnrage();

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

        private void CheckEnrage()
        {
            if (_enraged || _health == null || _health.MaxHP <= 0f) return;

            if (_health.CurrentHP / _health.MaxHP <= enrageHpThreshold)
            {
                _enraged = true;
                OnEnrageStart?.Invoke();

                ScreenShake.Trigger(this, amplitude: 0.6f, duration: 0.35f);
                transform.DOPunchScale(Vector3.one * 0.5f, 0.4f);

                if (bossMusicClip != null && AudioManager.Instance != null)
                    AudioManager.Instance.ChangeMusic(bossMusicClip);

                if (bossSprite != null)
                {
                    _baseSpriteColor = Color.Lerp(_baseSpriteColor, new Color(1f, 0.35f, 0.35f), 0.35f);
                    bossSprite.DOKill();
                    bossSprite.DOColor(_baseSpriteColor, 0.3f);
                }
            }
        }

        private float EffectiveLungeSpeed => _enraged ? lungeSpeed * enrageSpeedMultiplier : lungeSpeed;
        private float EffectiveCooldown => _enraged ? cooldownDuration * enrageCooldownMultiplier : cooldownDuration;
        private int EffectiveBarrageShotCount => _enraged
            ? Mathf.RoundToInt(barrageShotCount * enrageBarrageShotMultiplier)
            : barrageShotCount;

        private void TickApproaching()
        {
            _timeSinceLastAttack += Time.fixedDeltaTime;
            if (_timeSinceLastAttack >= maxTimeWithoutAttack)
            {
                _rb.linearVelocity = Vector2.zero;
                BeginTelegraphBarrage();
                return;
            }

            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(_rb.position, _currentTarget.position);
            if (dist <= attackRange)
            {
                _rb.linearVelocity = Vector2.zero;
                BeginTelegraphMelee();
                return;
            }

            UpdateLungeState();

            float currentSpeed = _isLunging ? EffectiveLungeSpeed : moveSpeed;
            Vector2 dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
            _rb.linearVelocity = dir * currentSpeed;
        }

        private void UpdateLungeState()
        {
            if (_isLunging)
            {
                _lungeTimer -= Time.fixedDeltaTime;
                if (_lungeTimer <= 0f) _isLunging = false;
                return;
            }

            _lungeIntervalTimer += Time.fixedDeltaTime;
            if (_lungeIntervalTimer >= lungeInterval)
            {
                _isLunging = true;
                _lungeTimer = lungeDuration;
                _lungeIntervalTimer = 0f;

                transform.DOPunchScale(new Vector3(0.2f, -0.15f, 0f), lungeDuration * 0.6f);
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

                case BossPhase.TelegraphingBarrage:
                    _phaseTimer += dt;
                    if (_phaseTimer >= telegraphBarrageDuration) BeginBarrage();
                    break;

                case BossPhase.Barrage:
                    TickBarrage(dt);
                    break;

                case BossPhase.Cooldown:
                    _phaseTimer += dt;
                    if (_phaseTimer >= EffectiveCooldown) _phase = BossPhase.Approaching;
                    break;
            }
        }

        private void BeginTelegraphMelee()
        {
            _phase = BossPhase.Telegraphing;
            _phaseTimer = 0f;
            _isLunging = false;

            transform.DOShakePosition(telegraphDuration, 0.1f);
            PulseTelegraphColor(telegraphDuration);
            OnTelegraphStart?.Invoke();
        }

        private void BeginOverflow()
        {
            _phase = BossPhase.Overflowing;
            _phaseTimer = 0f;
            _timeSinceLastAttack = 0f;
            OnOverflowFire?.Invoke();

            if (Camera.main != null)
                Camera.main.transform.DOShakePosition(0.3f, 0.6f);

            transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);

            if (PlayerTarget != null && Vector2.Distance(_rb.position, PlayerTarget.position) <= overflowRadius)
                PlayerTarget.GetComponent<Health>()?.TakeDamage(overflowDamage, gameObject);

            if (CoreTarget != null && Vector2.Distance(_rb.position, CoreTarget.position) <= overflowRadius)
                CoreTarget.GetComponent<Health>()?.TakeDamage(overflowDamage, gameObject);
        }

        private void BeginTelegraphBarrage()
        {
            _phase = BossPhase.TelegraphingBarrage;
            _phaseTimer = 0f;
            _timeSinceLastAttack = 0f;
            _isLunging = false;
            _barrageShotsFired = 0;
            _barrageShotTimer = 0f;

            transform.DOShakePosition(telegraphBarrageDuration, 0.05f);
            PulseTelegraphColor(telegraphBarrageDuration);
            OnBarrageTelegraphStart?.Invoke();
        }

        private void BeginBarrage()
        {
            _phase = BossPhase.Barrage;
            _phaseTimer = 0f;
            _barrageShotsFired = 0;
            _barrageShotTimer = barrageShotInterval; // dispara o primeiro tiro imediatamente
            _currentSpiralAngle = 0f;

            _barrageBaseDir = _currentTarget != null
                ? ((Vector2)_currentTarget.position - _rb.position).normalized
                : (Vector2)transform.up;
        }

        private void TickBarrage(float dt)
        {
            _barrageShotTimer += dt;
            int effectiveShotCount = EffectiveBarrageShotCount;

            if (_barrageShotTimer >= barrageShotInterval && _barrageShotsFired < effectiveShotCount)
            {
                _barrageShotTimer = 0f;
                FireSpiralShot();
                _barrageShotsFired++;
            }

            if (_barrageShotsFired >= effectiveShotCount)
                BeginCooldown();
        }

        private void FireSpiralShot()
        {
            if (projectilePrefab == null || ObjectPool.Instance == null) return;

            Vector2 dir = RotateVector(_barrageBaseDir, _currentSpiralAngle);
            _currentSpiralAngle += spiralAngleStep;

            var projGO = ObjectPool.Instance.Get(projectilePrefab, _rb.position, Quaternion.identity);
            var proj = projGO.GetComponent<EnemyProjectile>();
            if (proj != null) proj.Launch(dir, projectileDamage);

            OnBarrageShotFired?.Invoke();
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private void BeginCooldown()
        {
            _phase = BossPhase.Cooldown;
            _phaseTimer = 0f;
        }

        private void PulseTelegraphColor(float duration)
        {
            if (bossSprite == null) return;

            bossSprite.DOKill();
            bossSprite.DOColor(telegraphColor, duration * 0.3f)
                .OnComplete(() => bossSprite.DOColor(_baseSpriteColor, duration * 0.7f));
        }
    }
}