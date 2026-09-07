using UnityEngine;
using Nucleo.Core;

namespace Nucleo.Enemies
{
    public class AtiradorEnemy : EnemyBase
    {
        [Header("Atirador - DDoS")]
        [SerializeField] private float minRange = 3.5f;
        [SerializeField] private float maxRange = 6f;
        [SerializeField] private float burstCooldown = 2.2f;
        [SerializeField] private int projectilesPerBurst = 3;
        [SerializeField] private float burstSpreadAngle = 20f;
        [SerializeField] private float projectileDamage = 5f;
        [SerializeField] private GameObject projectilePrefab;

        private float _burstTimer;

        protected override void OnEnable()
        {
            base.OnEnable();
            _burstTimer = Random.Range(0f, burstCooldown);
        }

        protected override Transform PickNearestTarget()
        {
            return CoreTarget;
        }

        protected override void FixedUpdate()
        {
            if (_currentTarget == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(_rb.position, _currentTarget.position);
            Vector2 dirToTarget = ((Vector2)_currentTarget.position - _rb.position).normalized;
            transform.up = dirToTarget;

            if (dist > maxRange)
                _rb.linearVelocity = dirToTarget * moveSpeed;
            else if (dist < minRange)
                _rb.linearVelocity = -dirToTarget * moveSpeed;
            else
                _rb.linearVelocity = Vector2.zero;

            _burstTimer -= Time.fixedDeltaTime;
            if (_burstTimer <= 0f && dist <= maxRange)
            {
                _burstTimer = burstCooldown;
                FireBurst(_currentTarget.position);
            }
        }

        private void FireBurst(Vector2 aimPoint)
        {
            Vector2 baseDir = (aimPoint - _rb.position).normalized;
            float startAngle = -burstSpreadAngle * 0.5f;
            float step = projectilesPerBurst > 1 ? burstSpreadAngle / (projectilesPerBurst - 1) : 0f;

            for (int i = 0; i < projectilesPerBurst; i++)
            {
                float angle = startAngle + step * i;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;

                GameObject go = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                if (go.TryGetComponent(out EnemyProjectile proj))
                    proj.Launch(dir, projectileDamage);
            }
        }
    }
}