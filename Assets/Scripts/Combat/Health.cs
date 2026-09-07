using System;
using UnityEngine;
using Nucleo.GameFeel;
using Nucleo.Enemies;

namespace Nucleo
{
    [RequireComponent(typeof(DamageFlash))]
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHP = 100f;

        [Header("Invulnerabilidade (i-frames)")]
        [Tooltip("Se ativado, bloqueia novos danos por um curto período após ser atingido.")]
        [SerializeField] private bool useIFrames = false;
        [Tooltip("Tempo (em segundos) que o objeto fica imune a novos danos.")]
        [SerializeField] private float iFrameDuration = 0.4f;

        public float MaxHP => maxHP;
        public float CurrentHP { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => useIFrames && (Time.time - _lastDamageTime < iFrameDuration);

        public event Action<float, float> OnHealthChanged;
        public event Action<float, GameObject> OnDamaged;
        public event Action OnDeath;

        public static event Action<Health, float, GameObject> AnyDamaged;
        public static event Action<Health> AnyDeath;

        private float _lastDamageTime = -999f;

        private void Awake()
        {
            if (GetComponent<DamageFlash>() == null)
                gameObject.AddComponent<DamageFlash>();

            CurrentHP = maxHP;
            IsDead = false;
        }

        private void OnDamagedFlash(float amount, GameObject source)
        {
            GetComponent<DamageFlash>()?.Flash();
        }

        private void OnEnable()
        {
            OnDamaged += OnDamagedFlash;
        }

        private void OnDisable()
        {
            OnDamaged -= OnDamagedFlash;
        }

        public void ResetHealth()
        {
            CurrentHP = maxHP;
            IsDead = false;
            _lastDamageTime = -999f;
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        public void SetMaxHP(float newMax, bool healToFull = true)
        {
            maxHP = Mathf.Max(1f, newMax);
            CurrentHP = healToFull ? maxHP : Mathf.Min(CurrentHP, maxHP);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        public void TakeDamage(float amount, GameObject source = null)
        {
            ApplyDamage(amount, source, null, 0f);
        }

        public void TakeDamage(float amount, Vector3 hitSourcePosition, float knockbackForce = 5f, GameObject source = null)
        {
            ApplyDamage(amount, source, hitSourcePosition, knockbackForce);
        }

        private void ApplyDamage(float amount, GameObject source, Vector3? hitSourcePosition, float knockbackForce)
        {
            if (IsDead || amount <= 0f) return;

            if (useIFrames && Time.time - _lastDamageTime < iFrameDuration) return;

            _lastDamageTime = Time.time;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            Debug.Log($"[Dano] {gameObject.name} recebeu {amount} de {(source != null ? source.name : "?")} → HP {CurrentHP}/{maxHP}");
            OnDamaged?.Invoke(amount, source);
            AnyDamaged?.Invoke(this, amount, source);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);

            bool isPlayer = EnemyBase.PlayerTarget != null && transform.root == EnemyBase.PlayerTarget.root;
            bool isCore = EnemyBase.CoreTarget != null && transform.root == EnemyBase.CoreTarget.root;

            if (TryGetComponent<DamageFlash>(out var flash))
                flash.Flash();

            if (isPlayer)
            {
                HitStop.Trigger(this, 30f); 
            }
            else if (isCore && CurrentHP > 0f)
            {
                HitStop.Trigger(this, 15f); 
            }

            if (hitSourcePosition.HasValue && knockbackForce > 0f && TryGetComponent<Rigidbody2D>(out var rb))
            {
                if (rb.bodyType != RigidbodyType2D.Static)
                {
                    Vector2 pushDirection = ((Vector2)transform.position - (Vector2)hitSourcePosition.Value).normalized;
                    transform.position += (Vector3)(pushDirection * (knockbackForce * 0.04f));
                    rb.linearVelocity = pushDirection * knockbackForce;
                }
            }

            if (isCore)
            {
                ScreenShake.Trigger(this, amplitude: 0.3f, duration: 0.15f);
            }
            else if (isPlayer)
            {
                ScreenShake.Trigger(this, amplitude: 0.15f, duration: 0.1f);
            }

            if (CurrentHP <= 0f)
            {
                IsDead = true;

                if (isCore)
                {
                    ScreenShake.Trigger(this, amplitude: 0.8f, duration: 0.3f);
                }

                OnDeath?.Invoke();
                AnyDeath?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }
    }
}