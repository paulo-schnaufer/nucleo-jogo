using System.Collections.Generic;
using UnityEngine;

namespace Nucleo.Core
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [System.Serializable]
        public struct PoolConfig
        {
            public GameObject prefab;
            public int initialSize;
        }

        [Tooltip("Pools pré-aquecidos na inicialização (recomendado pra inimigos, projéteis e orbes de XP). Pools não listados aqui são criados sob demanda na primeira chamada de Get().")]
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

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var queue = GetOrCreateQueue(prefab);
            GameObject obj = queue.Count > 0 ? queue.Dequeue() : CreateNew(prefab);

            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject instance)
        {
            var poolItem = instance.GetComponent<PoolItem>();
            if (poolItem == null || poolItem.SourcePrefab == null)
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot);
            GetOrCreateQueue(poolItem.SourcePrefab).Enqueue(instance);
        }
    }
}