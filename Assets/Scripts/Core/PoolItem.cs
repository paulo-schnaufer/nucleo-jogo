using System.Collections.Generic;
using UnityEngine;

namespace Nucleo.Core
{
    public class PoolItem : MonoBehaviour
    {
        public GameObject SourcePrefab { get; set; }

        public void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}