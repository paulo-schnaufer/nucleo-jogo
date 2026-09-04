namespace Nucleo.Upgrades
{
    /// <summary>
    /// Contrato que todo script de arma-upgrade (DaemonTurret, RoundRobinBlades, ForkShot)
    /// precisa implementar para que o UpgradeManager consiga aplicar níveis de forma
    /// genérica, sem um switch-case por arma.
    /// </summary>
    public interface IUpgradableWeapon
    {
        /// <summary>
        /// Chamado pelo UpgradeManager toda vez que a arma sobe de nível — inclusive no
        /// nível 1, no momento em que a arma é desbloqueada e instanciada.
        /// </summary>
        void ApplyLevel(int level, UpgradeLevelData data);
    }
}
