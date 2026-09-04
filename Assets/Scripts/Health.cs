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

        private void Awake()
        {
            CurrentHP = maxHP;
            IsDead = false;
        }

        /// <summary>
        /// Chame isso ao reciclar um objeto do pool (ex.: inimigo que voltou
        /// a ficar ativo depois de ter morrido antes). Sem isso, um inimigo
        /// reciclado voltaria com HP zerado/morto.
        /// </summary>
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

        public void TakeDamage(float amount, GameObject source = null)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            OnDamaged?.Invoke(amount, source);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);

            if (CurrentHP <= 0f)
            {
                IsDead = true;
                OnDeath?.Invoke();
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
