using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nucleo.Core;

namespace Nucleo
{
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

        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        public int CurrentWaveIndex { get; private set; } = -1;
        public bool IsSpawning { get; private set; }
        public int EnemiesAliveInWave { get; private set; }

        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCleared;
        public event System.Action OnAllWavesCleared;

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

            if (enemyGO == null)
            {
                Debug.LogError($"[EnemySpawner] ObjectPool.Get retornou null para o prefab '{prefab.name}' — checar se o pool sobrevive a reloads de cena corretamente.");
                return;
            }

            var health = enemyGO.GetComponent<Health>();
            if (health == null)
            {
                Debug.LogError($"[EnemySpawner] Inimigo instanciado sem componente Health: '{prefab.name}'.");
                return;
            }

            EnemiesAliveInWave++;

            void HandleDeath()
            {
                EnemiesAliveInWave--;
                health.OnDeath -= HandleDeath;
            }
            health.OnDeath += HandleDeath;
        }
    }
}