using UnityEngine;

namespace Nucleo
{
    [RequireComponent(typeof(PoolItem))]
    public class PooledImpactEffect : MonoBehaviour
    {
        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponentInChildren<ParticleSystem>();
            if (_particles != null)
            {
                var main = _particles.main;
                main.useUnscaledTime = true;
            }
        }

        private void OnEnable()
        {
            _particles?.Play(true);
        }

        private void Update()
        {
            if (_particles != null && !_particles.IsAlive(true))
                GetComponent<PoolItem>().ReturnToPool();
        }

        private void OnDisable()
        {
            _particles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
