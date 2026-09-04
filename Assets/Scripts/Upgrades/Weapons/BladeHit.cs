using System.Collections.Generic;
using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Componente de dano por contato para as lâminas orbitais (RoundRobinBlades).
    /// Fica no prefab da lâmina; usa um Collider2D marcado como Trigger.
    ///
    /// Usa EnemyBase.ApplyDamage para manter a origem do dano identificável.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BladeHit : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float hitCooldown = 0.5f; // evita dano a cada frame no mesmo inimigo

        private float _damage;
        private readonly Dictionary<Collider2D, float> _lastHitTime = new Dictionary<Collider2D, float>();

        public void SetDamage(float damage) => _damage = damage;

        private void OnTriggerStay2D(Collider2D other)
        {
            if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

            if (_lastHitTime.TryGetValue(other, out float lastTime) && Time.time - lastTime < hitCooldown)
                return;

            _lastHitTime[other] = Time.time;
            other.GetComponent<EnemyBase>()?.ApplyDamage(_damage, gameObject);
        }
    }
}
