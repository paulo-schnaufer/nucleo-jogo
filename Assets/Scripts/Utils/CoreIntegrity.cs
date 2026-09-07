using UnityEngine;
using Nucleo.Enemies;

namespace Nucleo
{
    [RequireComponent(typeof(Health))]
    public class CoreIntegrity : MonoBehaviour
    {
        [Range(0f, 1f)]
        [Tooltip("Fração de HP restante abaixo da qual a Integridade é considerada crítica (gatilho de telegraph âmbar).")]
        [SerializeField] private float criticalThreshold = 0.25f;

        public Health Health { get; private set; }
        public bool IsCritical { get; private set; }

        public event System.Action OnCriticalEntered;

        private void Awake()
        {
            Health = GetComponent<Health>();
            Health.OnHealthChanged += HandleHealthChanged;

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
                IsCritical = false; 
            }
        }
    }
}