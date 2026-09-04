// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Marcador colocado automaticamente pelo ObjectPool em toda instância
    /// criada por ele. Guarda a referência do prefab de origem, pra saber a
    /// qual fila devolver o objeto quando ReturnToPool() for chamado.
    ///
    /// Não precisa adicionar este script manualmente nos prefabs — scripts
    /// como EnemyBase, Projectile e XPOrb já o exigem via [RequireComponent],
    /// então o Editor adiciona sozinho ao anexar aquele script.
    /// </summary>
    public class PoolItem : MonoBehaviour
    {
        public GameObject SourcePrefab { get; set; }

        /// <summary>Atalho pra qualquer script devolver este objeto ao pool sem guardar referência ao prefab.</summary>
        public void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
