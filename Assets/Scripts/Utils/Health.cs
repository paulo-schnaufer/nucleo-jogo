// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Componente genérico de vida. Usado no jogador, em inimigos e no
    /// Núcleo (Integridade — ver DECISIONS.md sobre CORE-VIOLETA no
    /// STYLE_GUIDE.md). Não sabe nada sobre quem é o dono — só gerencia HP
    /// e dispara eventos; cada dono decide o que fazer com a morte.
    /// </summary>
    [RequireComponent(typeof(DamageFlash))]
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHP = 100f;

        public float MaxHP => maxHP;
        public float CurrentHP { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>current, max — pra qualquer barra de vida de UI.</summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary>amount, source (pode ser null)</summary>
        public event Action<float, GameObject> OnDamaged;
        public event Action OnDeath;

        public static event Action<Health, float, GameObject> AnyDamaged;
        public static event Action<Health> AnyDeath;

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
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        public void SetMaxHP(float newMax, bool healToFull = true)
        {
            maxHP = Mathf.Max(1f, newMax);
            CurrentHP = healToFull ? maxHP : Mathf.Min(CurrentHP, maxHP);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        /// <summary> Dano padrão (com origem do GameObject). </summary>
        public void TakeDamage(float amount, GameObject source = null)
        {
            ApplyDamage(amount, source, null, 0f);
        }

        /// <summary> Dano com cálculo de Knockback (afastamento). </summary>
        public void TakeDamage(float amount, Vector3 hitSourcePosition, float knockbackForce = 5f, GameObject source = null)
        {
            ApplyDamage(amount, source, hitSourcePosition, knockbackForce);
        }

        private void ApplyDamage(float amount, GameObject source, Vector3? hitSourcePosition, float knockbackForce)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            OnDamaged?.Invoke(amount, source);
            AnyDamaged?.Invoke(this, amount, source);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);

            // Identifica se a entidade é o Player ou o Núcleo
            bool isPlayer = EnemyBase.PlayerTarget != null && transform.root == EnemyBase.PlayerTarget.root;
            bool isCore = EnemyBase.CoreTarget != null && transform.root == EnemyBase.CoreTarget.root;

            // Flash de dano no Sprite
            if (TryGetComponent<DamageFlash>(out var flash))
                flash.Flash();

            // Congelamento de impacto (HitStop) para dar peso aos tiros
            if (isPlayer)
            {
                HitStop.Trigger(this, 40f); 
            }
            else if (!isCore && CurrentHP > 0f) // Só dá a pausa se o inimigo CONTINUAR VIVO
            {
                HitStop.Trigger(this, 25f); 
            }

            // Afastamento (Knockback) imediato (apenas corpos não estáticos)
            if (hitSourcePosition.HasValue && knockbackForce > 0f && TryGetComponent<Rigidbody2D>(out var rb))
            {
                if (rb.bodyType != RigidbodyType2D.Static)
                {
                    Vector2 pushDirection = ((Vector2)transform.position - (Vector2)hitSourcePosition.Value).normalized;
                    transform.position += (Vector3)(pushDirection * (knockbackForce * 0.04f));
                    rb.linearVelocity = pushDirection * knockbackForce;
                }
            }

            // Shake de câmera por dano convencional
            if (isCore)
            {
                ScreenShake.Trigger(this, amplitude: 0.6f, duration: 0.28f);
            }
            else if (isPlayer)
            {
                ScreenShake.Trigger(this, amplitude: 0.3f, duration: 0.15f);
            }

            if (CurrentHP <= 0f)
            {
                IsDead = true;

                // Shake massivo e prolongado no momento em que o Núcleo é destruído
                if (isCore)
                {
                    ScreenShake.Trigger(this, amplitude: 1.8f, duration: 0.75f);
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