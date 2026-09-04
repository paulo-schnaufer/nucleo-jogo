// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Spawner de ondas com object pooling desde o início. 6 ondas
    /// (SCOPE_LOCK.md), onda 6 = boss com padrão telegrafado. Cada onda
    /// define quais tipos de inimigo spawnam, quantos, e o intervalo entre
    /// spawns; spawna em pontos aleatórios de uma lista de Transforms
    /// (posicionados manualmente no Editor, fora da visão da câmera).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class WaveEntry
        {
            public GameObject enemyPrefab;
            public int count;
        }

        [System.Serializable]
        public class WaveData
        {
            public string waveName;
            public List<WaveEntry> entries = new List<WaveEntry>();
            [Tooltip("Segundos entre cada spawn individual dentro da onda.")]
            public float spawnInterval = 0.5f;
            [Tooltip("Segundos de calmaria antes desta onda começar.")]
            public float delayBeforeWave = 3f;
        }

        [Tooltip("6 entradas, na ordem — a última é a onda do boss (SCOPE_LOCK.md).")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        public int CurrentWaveIndex { get; private set; } = -1;
        public bool IsSpawning { get; private set; }
        public int EnemiesAliveInWave { get; private set; }

        public event System.Action<int> OnWaveStarted;   // índice da onda (0-based)
        public event System.Action<int> OnWaveCleared;    // índice da onda concluída
        public event System.Action OnAllWavesCleared;      // vitória (onda 6/boss derrotado)

        private void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < waves.Count; i++)
            {
                CurrentWaveIndex = i;
                yield return new WaitForSeconds(waves[i].delayBeforeWave);

                OnWaveStarted?.Invoke(i);
                yield return StartCoroutine(SpawnWave(waves[i]));

                // Espera todos os inimigos desta onda morrerem antes de seguir.
                yield return new WaitUntil(() => EnemiesAliveInWave <= 0);
                OnWaveCleared?.Invoke(i);
            }

            OnAllWavesCleared?.Invoke();
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            IsSpawning = true;

            foreach (var entry in wave.entries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyPrefab);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }

            IsSpawning = false;
        }

        private void SpawnEnemy(GameObject prefab)
        {
            if (prefab == null || spawnPoints.Count == 0 || ObjectPool.Instance == null) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var enemyGO = ObjectPool.Instance.Get(prefab, point.position, Quaternion.identity);

            EnemiesAliveInWave++;
            var health = enemyGO.GetComponent<Health>();
            if (health == null) return;

            // Assina uma vez por spawn; a própria closure se desinscreve ao disparar,
            // então não conta duas vezes se o inimigo for reciclado e morrer de novo depois.
            void HandleDeath()
            {
                EnemiesAliveInWave--;
                health.OnDeath -= HandleDeath;
            }
            health.OnDeath += HandleDeath;
        }
    }
}
