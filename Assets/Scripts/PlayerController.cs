// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Movimento top-down 2D do jogador.
    ///
    /// ASSUNÇÃO PENDENTE DE CONFIRMAÇÃO: SCOPE_LOCK.md não define
    /// explicitamente 2D ou 3D. Este script assume 2D top-down (Rigidbody2D,
    /// Vector2) porque o STYLE_GUIDE.md é inteiramente sprite-based
    /// (Sprite Renderer, PPU, Pivot Bottom/Center, "bullet heaven"). Se a
    /// decisão real for 3D, este script (e EnemyBase/Projectile/XPOrb, que
    /// também usam Rigidbody2D) precisam ser reescritos.
    ///
    /// Usa Input Manager legado (Input.GetAxisRaw) em vez do pacote New
    /// Input System — decisão técnica pra zero setup extra no prazo. Exige
    /// Project Settings > Player > Active Input Handling = "Input Manager
    /// (Old)" ou "Both".
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private PlayerStats _stats;
        private Vector2 _moveInput;

        /// <summary>Exposto pra outros sistemas (ex.: mira de arma direcional futura).</summary>
        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<PlayerStats>();
            _rb.gravityScale = 0f; // top-down: sem gravidade
            _rb.freezeRotation = true;

            // Simplificação intencional pro protótipo (1 mapa, 1 jogador):
            // referência estática direta em vez de service locator.
            EnemyBase.PlayerTarget = transform;
        }

        private void Update()
        {
            // Leitura de input em Update (mais responsivo); movimento aplicado em FixedUpdate.
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(h, v).normalized;

            if (_moveInput.sqrMagnitude > 0.01f)
                FacingDirection = _moveInput;
        }

        private void FixedUpdate()
        {
            // Quando o jogo está pausado pra escolha de upgrade (Time.timeScale = 0),
            // o Unity já congela a física automaticamente — não precisa checar estado aqui.
            _rb.linearVelocity = _moveInput * _stats.MoveSpeed;
        }
    }
}
