using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// STUB — provável duplicata parcial de um PlayerStats/PlayerController já existente
    /// no projeto (não tive acesso a esse script). Antes de usar: se já existir, apenas
    /// copiar os 4 métodos Add* e os campos de bônus pra ele, e apagar este arquivo.
    /// Não sobrescrever nada relativo a movimentação via Input.GetAxisRaw (DECISIONS.md).
    ///
    /// Segue o padrão de referência estática simples (Instance) já usado em
    /// EnemyBase.PlayerTarget/CoreTarget (DECISIONS.md) — sem service locator.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Base")]
        public float baseMoveSpeed = 4f;
        public float baseDamageMultiplier = 1f;
        public float basePickupRadius = 1.5f;

        [Header("Runtime (somado pelos upgrades passivos — eles empilham, DECISIONS.md)")]
        [SerializeField] private float bonusSpeedPercent;
        [SerializeField] private float bonusDamagePercent;
        [SerializeField] private float regenPerSecond;
        [SerializeField] private float bonusPickupRadius;

        private float _regenAccumulator;

        public float CurrentMoveSpeed => baseMoveSpeed * (1f + bonusSpeedPercent);
        public float CurrentDamageMultiplier => baseDamageMultiplier * (1f + bonusDamagePercent);
        public float CurrentPickupRadius => basePickupRadius + bonusPickupRadius;

        private void Awake() => Instance = this;

        // --- Chamados pelo UpgradeManager ---
        public void AddSpeedMultiplier(float percent) => bonusSpeedPercent += percent;
        public void AddDamageMultiplier(float percent) => bonusDamagePercent += percent;
        public void AddPickupRadius(float units) => bonusPickupRadius += units;
        public void AddRegen(float hpPerSecond) => regenPerSecond += hpPerSecond;

        private void Update()
        {
            if (regenPerSecond <= 0f) return;

            // Acumula fração de HP pra funcionar direito com valores baixos (ex.: 0.4 HP/s).
            _regenAccumulator += regenPerSecond * Time.deltaTime;
            if (_regenAccumulator >= 1f)
            {
                int wholeHeal = Mathf.FloorToInt(_regenAccumulator);
                _regenAccumulator -= wholeHeal;
                // TODO: trocar pela chamada real de cura do sistema de vida do jogador
                // quando existir (ex.: healthComponent.Heal(wholeHeal)).
            }
        }
    }
}
