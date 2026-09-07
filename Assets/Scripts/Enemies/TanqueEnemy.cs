using UnityEngine;

namespace Nucleo.Enemies
{
    public class TanqueEnemy : EnemyBase
    {
        [Header("Tanque - Memory Leak")]
        [SerializeField] private float leakGrowthPerSecond = 0.6f;
        [SerializeField] private float contactDamageCap = 24f;

        private float _baseContactDamage;
        private float _aliveTime;

        protected override void Awake()
        {
            base.Awake();
            _baseContactDamage = contactDamage;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _aliveTime = 0f;
            contactDamage = _baseContactDamage;
        }

        protected override void Update()
        {
            base.Update();
            _aliveTime += Time.deltaTime;
            contactDamage = Mathf.Min(_baseContactDamage + leakGrowthPerSecond * _aliveTime, contactDamageCap);
        }
    }
}