// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Marca o GameObject do Núcleo (Integridade) e expõe o evento de
    /// "integridade crítica" (ver STYLE_GUIDE.md seção 5 — telegraph
    /// ÂMBAR-ALERTA) sem duplicar lógica de HP, que continua no componente
    /// Health. Também registra este Transform como alvo dos inimigos
    /// (EnemyBase.CoreTarget).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class CoreIntegrity : MonoBehaviour
    {
        [Range(0f, 1f)]
        [Tooltip("Fração de HP restante abaixo da qual a Integridade é considerada crítica (gatilho de telegraph âmbar).")]
        [SerializeField] private float criticalThreshold = 0.25f;

        public Health Health { get; private set; }
        public bool IsCritical { get; private set; }

        /// <summary>Dispara ao cruzar o limiar crítico pra baixo (pra VFX/áudio/telegraph).</summary>
        public event System.Action OnCriticalEntered;

        private void Awake()
        {
            Health = GetComponent<Health>();
            Health.OnHealthChanged += HandleHealthChanged;

            // Simplificação intencional pro protótipo (1 mapa, 1 Núcleo):
            // referência estática direta em vez de service locator.
            EnemyBase.CoreTarget = transform;
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            bool nowCritical = (current / max) <= criticalThreshold;
            if (nowCritical && !IsCritical)
            {
                IsCritical = true;
                OnCriticalEntered?.Invoke();
            }
            else if (!nowCritical && IsCritical)
            {
                IsCritical = false; // permite re-disparo se a Integridade regenerar acima do limiar
            }
        }
    }
}
