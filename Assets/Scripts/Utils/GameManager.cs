// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;
using System.Collections.Generic;

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
        [Header("Feedback opcional")]
        [SerializeField] private GameObject enemyHitEffectPrefab;
        [SerializeField] private GameObject enemyDeathEffectPrefab;
        [SerializeField] private GameObject bossHitEffectPrefab;
        [SerializeField] private GameObject coreDamageEffectPrefab;
        [SerializeField] private GameObject bossDamageNumberPrefab;

        private const int FeedbackCapacity = 20;
        private readonly List<GameObject> _activeEffects = new List<GameObject>();
        private readonly List<GameObject> _activeDamageNumbers = new List<GameObject>();

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
            Health.AnyDamaged += HandleAnyDamaged;
            Health.AnyDeath += HandleAnyDeath;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDeath;
            if (coreIntegrity != null) coreIntegrity.Health.OnDeath -= HandleCoreDestroyed;
            if (spawner != null) spawner.OnAllWavesCleared -= HandleVictory;
            Health.AnyDamaged -= HandleAnyDamaged;
            Health.AnyDeath -= HandleAnyDeath;
        }

        private void HandleAnyDamaged(Health target, float amount, GameObject source)
        {
            if (CurrentState != GameState.Playing) return;

            if (target.GetComponent<CoreIntegrity>() != null)
            {
                HitStop.Trigger(this, 80f);
                ScreenShake.Trigger(this, 0.15f, 0.25f);
                SpawnEffect(coreDamageEffectPrefab, target.transform.position);
                return;
            }

            var boss = target.GetComponent<BossEnemy>();
            if (boss != null)
            {
                HitStop.Trigger(this, 60f);
                ScreenShake.Trigger(this, 0.08f, 0.15f);
                if (!target.IsDead)
                {
                    SpawnEffect(bossHitEffectPrefab, target.transform.position);
                    SpawnDamageNumber(target, amount);
                }
            }
            else if (target.GetComponent<EnemyBase>() != null && !target.IsDead)
            {
                SpawnEffect(enemyHitEffectPrefab, target.transform.position);
            }
        }

        private void HandleAnyDeath(Health target)
        {
            if (CurrentState != GameState.Playing) return;

            if (target.GetComponent<BossEnemy>() != null)
            {
                HitStop.Trigger(this, 180f, true);
                ScreenShake.Trigger(this, 0.35f, 0.6f);
                SpawnEffect(bossHitEffectPrefab, target.transform.position);
            }
            else if (target.GetComponent<EnemyBase>() != null)
            {
                HitStop.Trigger(this, 40f, true);
                ScreenShake.Trigger(this, 0.05f, 0.12f);
                SpawnEffect(enemyDeathEffectPrefab, target.transform.position);
            }
        }

        private void SpawnEffect(GameObject prefab, Vector3 position)
        {
            if (prefab == null || ObjectPool.Instance == null) return;

            CleanupInactive(_activeEffects);
            if (_activeEffects.Count >= FeedbackCapacity)
            {
                ObjectPool.Instance.Return(_activeEffects[0]);
                _activeEffects.RemoveAt(0);
            }

            GameObject effect = ObjectPool.Instance.Get(prefab, position, Quaternion.identity);
            if (effect.GetComponent<PooledImpactEffect>() == null)
                effect.AddComponent<PooledImpactEffect>();
            _activeEffects.Add(effect);
        }

        private void SpawnDamageNumber(Health target, float amount)
        {
            if (bossDamageNumberPrefab == null || ObjectPool.Instance == null) return;

            CleanupInactive(_activeDamageNumbers);
            if (_activeDamageNumbers.Count >= FeedbackCapacity)
            {
                ObjectPool.Instance.Return(_activeDamageNumbers[0]);
                _activeDamageNumbers.RemoveAt(0);
            }

            Vector2 offset = Random.insideUnitCircle * 0.1f;
            GameObject number = ObjectPool.Instance.Get(
                bossDamageNumberPrefab,
                target.transform.position + (Vector3)offset,
                Quaternion.identity
            );
            number.GetComponent<PooledDamageNumber>()?.SetDamage(amount);
            _activeDamageNumbers.Add(number);
        }

        private static void CleanupInactive(List<GameObject> objects)
        {
            objects.RemoveAll(item => item == null || !item.activeInHierarchy);
        }

        private void HandlePlayerDeath() => EndGame(GameState.GameOver);
        private void HandleCoreDestroyed() => EndGame(GameState.GameOver);
        private void HandleVictory() => EndGame(GameState.Victory);

        private void EndGame(GameState state)
        {
            if (CurrentState != GameState.Playing) return; // evita disparar 2x
            CurrentState = state;
            HitStop.NotifyExternalPause();
            Time.timeScale = 0f;
            OnGameStateChanged?.Invoke(state);
        }
    }
}
