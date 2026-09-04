// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Orbe de XP: fica parado até o jogador entrar no raio de coleta
    /// (PlayerStats.PickupRadius), então é atraído até ele. Reciclado via
    /// ObjectPool — não use Destroy() nele.
    /// </summary>
    [RequireComponent(typeof(PoolItem))]
    public class XPOrb : MonoBehaviour
    {
        [SerializeField] private float attractSpeed = 8f;
        [SerializeField] private float collectDistance = 0.2f;

        private int _value = 1;
        private Transform _player;
        private PlayerStats _playerStats;
        private bool _isAttracting;

        public void SetValue(int value) => _value = value;

        private void OnEnable()
        {
            _isAttracting = false;
            _player = EnemyBase.PlayerTarget;
            _playerStats = _player != null ? _player.GetComponent<PlayerStats>() : null;
        }

        private void Update()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);

            if (!_isAttracting && _playerStats != null && dist <= _playerStats.PickupRadius)
                _isAttracting = true;

            if (_isAttracting)
            {
                transform.position = Vector2.MoveTowards(transform.position, _player.position, attractSpeed * Time.deltaTime);
                if (dist <= collectDistance) Collect();
            }
        }

        private void Collect()
        {
            var progression = _player.GetComponent<PlayerProgression>();
            if (progression != null) progression.AddXP(_value);
            GetComponent<PoolItem>().ReturnToPool();
        }
    }
}
