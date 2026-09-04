// Enums de apoio ao sistema de upgrades.
// Convenção do projeto (ver DECISIONS.md: EnemyBase.PlayerTarget/CoreTarget, ObjectPool,
// PoolItem): identificadores de código em inglês; textos exibidos ao jogador
// (nomes/descrições) ficam em PT-BR, dentro dos ScriptableObjects (UpgradeDefinition).

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Categoria do upgrade — define a regra de aquisição já travada em DECISIONS.md:
    /// "upgrades passivos empilham, armas são únicas".
    /// </summary>
    public enum UpgradeCategory
    {
        Weapon,
        Passive
    }

    /// <summary>
    /// Identificador estável de cada um dos 7 upgrades do MVP (SCOPE_LOCK.md).
    /// NÃO adicionar Corrente Elétrica aqui — está fora do MVP (stretch goal,
    /// só entra se sobrar tempo real após o boss testado). Se algum dia entrar,
    /// é uma reabertura de escopo e precisa passar pelo Backlog do SCOPE_LOCK.md.
    /// </summary>
    public enum UpgradeId
    {
        // --- Armas (3) ---
        DaemonTurret,       // Torreta automática — "Daemon"
        RoundRobinBlades,   // Lâminas orbitais — "Round-Robin"
        ForkShot,           // Tiro em leque — "Fork()"

        // --- Passivas (4) ---
        Overclock,          // Velocidade de movimento
        CriticalExploit,    // Dano
        Redundancy,         // Regeneração de vida
        Cache               // Raio de coleta de XP
    }
}
