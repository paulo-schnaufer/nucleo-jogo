using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Ponto único de verdade sobre quais upgrades o jogador tem e em que nível.
    /// Responsável por:
    ///   1) Sortear até 3 opções por level-up, excluindo upgrades já no nível máximo.
    ///   2) Aplicar o upgrade escolhido (instanciar/upar arma, ou somar passiva).
    ///
    /// NÃO cuida de UI nem de pausa — isso fica pra tela de level-up (fora do escopo
    /// desta tarefa). A UI deve: pausar via Time.timeScale = 0 (já decidido em
    /// DECISIONS.md), chamar RollOptions(3), mostrar os cards, chamar ApplyUpgrade()
    /// com a escolha, e então Time.timeScale = 1.
    ///
    /// Padrão de referência estática simples (Instance), coerente com
    /// EnemyBase.PlayerTarget/CoreTarget (DECISIONS.md) — sem service locator,
    /// aceitável pro escopo de protótipo de 1 mapa/1 jogador.
    ///
    /// ASSUNÇÕES DE INTEGRAÇÃO (não tive acesso ao restante do código do repo —
    /// conferir antes de usar):
    /// - Existe (ou vai existir) um PlayerStats com Instance estático e os métodos
    ///   AddSpeedMultiplier / AddDamageMultiplier / AddRegen / AddPickupRadius
    ///   (ver PlayerStats.cs deste pacote — é um stub, pode já existir versão real).
    /// - Os prefabs de arma implementam IUpgradableWeapon.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Tooltip("Os 7 upgrades do MVP. Arraste os 7 ScriptableObjects aqui no Inspector.")]
        [SerializeField] private List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();

        [Tooltip("Transform filho do Player onde as armas são instanciadas. " +
                 "Se vazio, usa o transform de PlayerStats.Instance.")]
        [SerializeField] private Transform weaponAnchor;

        private readonly Dictionary<UpgradeDefinition, int> _currentLevels = new Dictionary<UpgradeDefinition, int>();
        private readonly Dictionary<UpgradeId, IUpgradableWeapon> _activeWeapons = new Dictionary<UpgradeId, IUpgradableWeapon>();

        /// <summary>Disparado quando as opções são sorteadas para a tela de level-up.</summary>
        public event Action<List<UpgradeDefinition>> OnOptionsRolled;

        /// <summary>Disparado depois que um upgrade é efetivamente aplicado.</summary>
        public event Action<UpgradeDefinition, int> OnUpgradeApplied;

        /// <summary>
        /// Disparado se não sobrar nenhum upgrade disponível (todos no máximo).
        /// A UI deve tratar esse evento pulando a tela de escolha e resumindo o jogo.
        /// </summary>
        public event Action OnNoUpgradesAvailable;

        private void Awake()
        {
            Instance = this;
            foreach (var upgrade in allUpgrades)
                _currentLevels[upgrade] = 0;
        }

        public int GetCurrentLevel(UpgradeDefinition upgrade)
            => upgrade != null && _currentLevels.TryGetValue(upgrade, out var level) ? level : 0;

        public bool IsMaxed(UpgradeDefinition upgrade)
            => upgrade != null && GetCurrentLevel(upgrade) >= upgrade.MaxLevel;

        /// <summary>
        /// Sorteia até <paramref name="count"/> upgrades DISTINTOS, excluindo qualquer
        /// upgrade já no nível máximo. Se sobrar menos que <paramref name="count"/>
        /// disponíveis, retorna o que houver (inclusive lista vazia).
        /// </summary>
        public List<UpgradeDefinition> RollOptions(int count = 3)
        {
            var available = allUpgrades.Where(u => u != null && !IsMaxed(u)).ToList();

            // Fisher-Yates parcial — evita viés de Ordenar-por-Random e é O(n).
            for (int i = available.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (available[i], available[j]) = (available[j], available[i]);
            }

            var result = available.Take(Mathf.Min(count, available.Count)).ToList();

            if (result.Count == 0)
                OnNoUpgradesAvailable?.Invoke();
            else
                OnOptionsRolled?.Invoke(result);

            return result;
        }

        /// <summary>Aplica o upgrade escolhido pelo jogador na tela de level-up.</summary>
        public void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null || IsMaxed(upgrade))
            {
                Debug.LogWarning($"[UpgradeManager] Upgrade inválido ou já no máximo: {upgrade?.displayName}");
                return;
            }

            int newLevel = GetCurrentLevel(upgrade) + 1;
            _currentLevels[upgrade] = newLevel;
            UpgradeLevelData data = upgrade.levels[newLevel - 1];

            if (upgrade.category == UpgradeCategory.Weapon)
                ApplyWeapon(upgrade, newLevel, data);
            else
                ApplyPassive(upgrade, data);

            OnUpgradeApplied?.Invoke(upgrade, newLevel);
        }

        private void ApplyWeapon(UpgradeDefinition upgrade, int newLevel, UpgradeLevelData data)
        {
            if (!_activeWeapons.TryGetValue(upgrade.id, out var weapon))
            {
                if (upgrade.weaponPrefab == null)
                {
                    Debug.LogError($"[UpgradeManager] {upgrade.displayName} é Weapon mas não tem weaponPrefab configurado.");
                    return;
                }

                Transform parent = weaponAnchor != null
                    ? weaponAnchor
                    : (PlayerStats.Instance != null ? PlayerStats.Instance.transform : null);

                var instanceGO = Instantiate(upgrade.weaponPrefab, parent);
                weapon = instanceGO.GetComponent<IUpgradableWeapon>();

                if (weapon == null)
                {
                    Debug.LogError($"[UpgradeManager] Prefab de {upgrade.displayName} não implementa IUpgradableWeapon.");
                    return;
                }

                _activeWeapons[upgrade.id] = weapon;
            }

            weapon.ApplyLevel(newLevel, data);
        }

        private void ApplyPassive(UpgradeDefinition upgrade, UpgradeLevelData data)
        {
            if (PlayerStats.Instance == null)
            {
                Debug.LogError("[UpgradeManager] PlayerStats.Instance não encontrado — passiva não aplicada.");
                return;
            }

            switch (upgrade.id)
            {
                case UpgradeId.Overclock:
                    PlayerStats.Instance.AddSpeedMultiplier(data.primaryValue);
                    break;
                case UpgradeId.CriticalExploit:
                    PlayerStats.Instance.AddDamageMultiplier(data.primaryValue);
                    break;
                case UpgradeId.Redundancy:
                    PlayerStats.Instance.AddRegen(data.primaryValue);
                    break;
                case UpgradeId.Cache:
                    PlayerStats.Instance.AddPickupRadius(data.primaryValue);
                    break;
                default:
                    Debug.LogWarning($"[UpgradeManager] Upgrade passivo sem case de aplicação: {upgrade.id}");
                    break;
            }
        }
    }
}
