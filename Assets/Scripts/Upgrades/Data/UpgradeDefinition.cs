using System.Collections.Generic;
using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// ScriptableObject que define um upgrade completo (metadados + progressão de níveis).
    /// Criar 1 asset por upgrade (7 no total) em Assets/Data/Upgrades/.
    ///
    /// Ícone deve seguir STYLE_GUIDE.md seção 2c: canvas-fonte 64x64 (HUD pode reduzir
    /// pra 48x48), sem contorno (seção 3), paleta travada de 13 cores.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Nucleo/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Identidade")]
        public UpgradeId id;
        public UpgradeCategory category;

        [Header("Exibição (texto final em PT-BR)")]
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Progressão")]
        [Tooltip("Índice 0 = nível 1. O TAMANHO da lista define o nível máximo do upgrade — " +
                 "é o que o UpgradeManager usa pra excluir o upgrade do sorteio quando maximizado.")]
        public List<UpgradeLevelData> levels = new List<UpgradeLevelData>();

        [Header("Somente para Category = Weapon")]
        [Tooltip("Prefab instanciado como filho do jogador na primeira vez que o upgrade é " +
                 "escolhido. Precisa ter um componente que implemente IUpgradableWeapon.")]
        public GameObject weaponPrefab;

        public int MaxLevel => levels.Count;
    }
}
