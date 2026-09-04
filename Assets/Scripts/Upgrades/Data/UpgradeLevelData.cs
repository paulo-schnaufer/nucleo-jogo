using System;
using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Dados de UM nível de um upgrade. Genérica de propósito — os 7 upgrades do MVP
    /// cabem nesta mesma estrutura, o significado de primaryValue/secondaryValue muda
    /// por upgrade. Isso evita 7 classes de dados diferentes num prazo de 2 dias.
    ///
    /// Tabela de uso (ver também comentário no topo de cada script de upgrade):
    /// - DaemonTurret:      primary = cadência de tiro (s)      | secondary = dano
    /// - RoundRobinBlades:  primary = nº de lâminas             | secondary = dano por lâmina
    /// - ForkShot:          primary = nº de projéteis do leque  | secondary = dano por projétil
    /// - Overclock:         primary = % bônus de velocidade     | secondary = não usado
    /// - CriticalExploit:   primary = % bônus de dano           | secondary = não usado
    /// - Redundancy:        primary = HP regenerado por segundo | secondary = não usado
    /// - Cache:             primary = unidades de raio de coleta| secondary = não usado
    /// </summary>
    [Serializable]
    public class UpgradeLevelData
    {
        [TextArea(1, 2)]
        [Tooltip("Texto curto mostrado no card de upgrade na UI, ex.: '+15% de dano'.")]
        public string description;

        [Tooltip("Valor principal do nível — ver tabela de uso no topo deste arquivo.")]
        public float primaryValue;

        [Tooltip("Valor secundário opcional — só usado pelas 3 armas (dano do projétil/lâmina).")]
        public float secondaryValue;
    }
}
