// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Tanque — reskin "memory leak".
    /// Usa o comportamento padrão de EnemyBase (persegue o alvo mais próximo entre jogador e
    /// Núcleo, dano por contato) — não há regra própria de alvo pra esse tipo em DECISIONS.md,
    /// então não sobrescrevo PickNearestTarget/FixedUpdate. A diferenciação vem de tuning no
    /// prefab (moveSpeed baixo, Health.maxHP alto) e de um efeito próprio, só deste subtipo:
    /// o dano de contato cresce enquanto o inimigo está vivo, até um teto — como um
    /// vazamento de memória que só aumenta com o tempo. Usa o campo protected contactDamage
    /// que já existe na base (não cria campo novo lá), resetado a cada volta do pool.
    /// </summary>
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
            _baseContactDamage = contactDamage; // guarda o valor configurado no prefab
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _aliveTime = 0f;
            contactDamage = _baseContactDamage;
        }

        protected override void Update()
        {
            base.Update(); // mantém a escolha padrão de alvo (mais próximo entre jogador/Núcleo)

            _aliveTime += Time.deltaTime;
            contactDamage = Mathf.Min(_baseContactDamage + leakGrowthPerSecond * _aliveTime, contactDamageCap);
        }
    }
}
