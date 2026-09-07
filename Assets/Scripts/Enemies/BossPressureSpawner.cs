using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nucleo.Core;

namespace Nucleo.Enemies
{
    public class BossPressureSpawner : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Health do boss. O spawner para de spawnar assim que esse componente morrer.")]
        [SerializeField] private Health bossHealth;
        [Tooltip("Transform do boss, usado só pra não spawnar dentro do raio do overflow.")]
        [SerializeField] private Transform bossTransform;

        [Header("Inimigos de Pressão")]
        [SerializeField] private List<GameObject> pressureEnemyPrefabs = new List<GameObject>();
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        [Header("Cadência")]
        [Tooltip("Segundos de calmaria antes do primeiro spawn, pra dar tempo do jogador processar a entrada do boss.")]
        [SerializeField] private float initialDelay = 4f;
        [SerializeField] private float spawnInterval = 5f;
        [Tooltip("Intervalo usado a partir do momento em que o boss entra em fúria.")]
        [SerializeField] private float enragedSpawnInterval = 2.5f;
        [SerializeField] private int maxAlive = 3;

        [Header("Segurança")]
        [Tooltip("Distância mínima do boss pra um ponto de spawn ser considerado válido — evita spawn dentro do overflow.")]
        [SerializeField] private float minDistanceFromBoss = 4.5f;

        private bool _enraged;
        private int _aliveCount;
        private Coroutine _routine;

        private void OnEnable()
        {
            _enraged = false;
            _aliveCount = 0;

            if (bossHealth != null)
                bossHealth.OnDeath += StopSpawning;

            _routine = StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            if (bossHealth != null)
                bossHealth.OnDeath -= StopSpawning;

            if (_routine != null) StopCoroutine(_routine);
        }

        public void NotifyEnrage()
        {
            _enraged = true;
        }

        private void StopSpawning()
        {
            if (_routine != null) StopCoroutine(_routine);
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                float interval = _enraged ? enragedSpawnInterval : spawnInterval;

                if (_aliveCount < maxAlive)
                    TrySpawnOne();

                yield return new WaitForSeconds(interval);
            }
        }

        private void TrySpawnOne()
        {
            if (pressureEnemyPrefabs.Count == 0 || spawnPoints.Count == 0 || ObjectPool.Instance == null) return;

            Transform point = PickValidSpawnPoint();
            if (point == null) return;

            var prefab = pressureEnemyPrefabs[Random.Range(0, pressureEnemyPrefabs.Count)];
            var enemyGO = ObjectPool.Instance.Get(prefab, point.position, Quaternion.identity);
            if (enemyGO == null) return;

            var health = enemyGO.GetComponent<Health>();
            if (health == null) return;

            _aliveCount++;

            void HandleDeath()
            {
                _aliveCount--;
                health.OnDeath -= HandleDeath;
            }
            health.OnDeath += HandleDeath;
        }

        private Transform PickValidSpawnPoint()
        {
            for (int attempt = 0; attempt < spawnPoints.Count; attempt++)
            {
                Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Count)];
                if (bossTransform == null) return candidate;

                float dist = Vector2.Distance(candidate.position, bossTransform.position);
                if (dist >= minDistanceFromBoss) return candidate;
            }
            return null;
        }
    }
}