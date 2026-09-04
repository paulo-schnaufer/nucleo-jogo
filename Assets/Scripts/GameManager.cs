// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Estado geral da partida: liga fim de jogo (morte do jogador OU
    /// Integridade do Núcleo zerada) e vitória (onda 6/boss derrotado).
    ///
    /// DECISÃO DE DESIGN PENDENTE DE CONFIRMAÇÃO: este script assume que
    /// Núcleo a zero = Game Over, igual à morte do jogador. SCOPE_LOCK.md só
    /// confirma que a Integridade afeta o TEXTO do fechamento ("fechamento
    /// com variante conforme integridade restante"), não confirma que ela é
    /// uma condição de derrota separada. Ver aviso na resposta desta sessão.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Health playerHealth;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private EnemySpawner spawner;

        public enum GameState { Playing, GameOver, Victory }
        public GameState CurrentState { get; private set; } = GameState.Playing;

        public event System.Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnDeath += HandlePlayerDeath;
            if (coreIntegrity != null) coreIntegrity.Health.OnDeath += HandleCoreDestroyed;
            if (spawner != null) spawner.OnAllWavesCleared += HandleVictory;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDeath;
            if (coreIntegrity != null) coreIntegrity.Health.OnDeath -= HandleCoreDestroyed;
            if (spawner != null) spawner.OnAllWavesCleared -= HandleVictory;
        }

        private void HandlePlayerDeath() => EndGame(GameState.GameOver);
        private void HandleCoreDestroyed() => EndGame(GameState.GameOver);
        private void HandleVictory() => EndGame(GameState.Victory);

        private void EndGame(GameState state)
        {
            if (CurrentState != GameState.Playing) return; // evita disparar 2x
            CurrentState = state;
            Time.timeScale = 0f;
            OnGameStateChanged?.Invoke(state);
        }
    }
}
