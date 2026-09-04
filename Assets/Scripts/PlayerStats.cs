// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Stats do jogador que podem ser modificados por upgrades passivos
    /// (velocidade, dano, regeneração, raio de coleta — SCOPE_LOCK.md).
    /// Centraliza os bônus pra qualquer sistema (movimento, armas, XPOrb)
    /// ler o valor final em vez de duplicar lógica de upgrade em cada script.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base (antes de upgrades)")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseDamageMultiplier = 1f;
        [SerializeField] private float baseRegenPerSecond = 0f;
        [SerializeField] private float basePickupRadius = 1.5f;

        public float MoveSpeed { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float RegenPerSecond { get; private set; }
        public float PickupRadius { get; private set; }

        private float _moveSpeedBonus;
        private float _damageBonus;
        private float _regenBonus;
        private float _pickupRadiusBonus;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            RecalculateStats();
        }

        private void Update()
        {
            if (RegenPerSecond > 0f && !_health.IsDead)
                _health.Heal(RegenPerSecond * Time.deltaTime);
        }

        // Chamados pelo UpgradeManager ao aplicar um upgrade passivo escolhido.
        public void AddMoveSpeed(float amount) { _moveSpeedBonus += amount; RecalculateStats(); }
        public void AddDamageMultiplier(float amount) { _damageBonus += amount; RecalculateStats(); }
        public void AddRegen(float amount) { _regenBonus += amount; RecalculateStats(); }
        public void AddPickupRadius(float amount) { _pickupRadiusBonus += amount; RecalculateStats(); }

        private void RecalculateStats()
        {
            MoveSpeed = baseMoveSpeed + _moveSpeedBonus;
            DamageMultiplier = baseDamageMultiplier + _damageBonus;
            RegenPerSecond = baseRegenPerSecond + _regenBonus;
            PickupRadius = basePickupRadius + _pickupRadiusBonus;
        }
    }
}
