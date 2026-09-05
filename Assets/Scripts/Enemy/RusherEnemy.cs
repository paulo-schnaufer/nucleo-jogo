// NÚCLEO: Última Onda — IA de Inimigos (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Rusher — reskin "loop infinito".
    /// Sem overrides: o comportamento padrão de EnemyBase (persegue o alvo mais próximo
    /// entre jogador e Núcleo por distância ao quadrado, dano por contato via trigger) já É
    /// exatamente esse padrão — ver o comentário de classe em EnemyBase.cs, que cita
    /// literalmente o rusher/loop infinito como o caso já coberto pela base.
    /// Esta classe existe só pra dar um tipo de prefab próprio (organização e futura tuning
    /// específica), não pra adicionar lógica.
    /// Tuning no prefab: moveSpeed alto, Health.maxHP baixo, contactDamage moderado.
    /// </summary>
    public class RusherEnemy : EnemyBase
    {
        [SerializeField] AudioClip hitClip;
        [SerializeField] float pitchMin = 1.15f, pitchMax = 1.35f; // por tipo de inimigo
        AudioSource src;

        void PlayHit() {
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.PlayOneShot(hitClip);
        }
    }
}
