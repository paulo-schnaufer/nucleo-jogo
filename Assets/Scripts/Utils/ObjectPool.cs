// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System.Collections.Generic;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Pool genérico de GameObjects, chaveado pelo prefab de origem.
    /// Qualquer sistema (Spawner, Torreta, drop de XP) pode pedir e devolver
    /// instâncias sem custo de Instantiate/Destroy em runtime.
    ///
    /// Singleton simples: acesse via ObjectPool.Instance. Coloque este script
    /// num GameObject vazio chamado "Managers" na cena (ver lista de
    /// GameObjects na resposta desta sessão).
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [System.Serializable]
        public struct PoolConfig
        {
            public GameObject prefab;
            public int initialSize;
        }

        [Tooltip("Pools pré-aquecidos na inicialização (recomendado pra inimigos, projéteis e orbes de XP). " +
                 "Pools não listados aqui são criados sob demanda na primeira chamada de Get().")]
        [SerializeField] private List<PoolConfig> prewarmPools = new List<PoolConfig>();

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _poolRoot = new GameObject("--- Pooled Objects (inativos) ---").transform;
            _poolRoot.SetParent(transform);

            foreach (var config in prewarmPools)
            {
                if (config.prefab == null) continue;
                var queue = GetOrCreateQueue(config.prefab);
                for (int i = 0; i < config.initialSize; i++)
                {
                    var obj = CreateNew(config.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(_poolRoot);
                    queue.Enqueue(obj);
                }
            }
        }

        private Queue<GameObject> GetOrCreateQueue(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }
            return queue;
        }

        private GameObject CreateNew(GameObject prefab)
        {
            var obj = Instantiate(prefab);
            var poolItem = obj.GetComponent<PoolItem>();
            if (poolItem == null) poolItem = obj.AddComponent<PoolItem>();
            poolItem.SourcePrefab = prefab;
            return obj;
        }

        /// <summary>Pega uma instância pronta pra usar, já posicionada e ativa.</summary>
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var queue = GetOrCreateQueue(prefab);
            GameObject obj = queue.Count > 0 ? queue.Dequeue() : CreateNew(prefab);

            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>Devolve a instância ao pool correto (via PoolItem.SourcePrefab). Chame isso em vez de Destroy().</summary>
        public void Return(GameObject instance)
        {
            var poolItem = instance.GetComponent<PoolItem>();
            if (poolItem == null || poolItem.SourcePrefab == null)
            {
                // Não veio de um pool conhecido: destrói de verdade pra não vazar memória.
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot);
            GetOrCreateQueue(poolItem.SourcePrefab).Enqueue(instance);
        }
    }
}
