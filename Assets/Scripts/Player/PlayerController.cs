using UnityEngine;
using Nucleo.Controls;
using Nucleo.Enemies;

namespace Nucleo.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Opcional. Deixe vazio em builds sem controle touch — o script funciona normalmente só com teclado.")]
        [SerializeField] private VirtualJoystick _joystick;

        [Header("Suavização de Movimento (Game Feel)")]
        [Tooltip("Quão rápido o jogador atinge a velocidade máxima ou para. Valores menores = mais 'escorregadio'.")]
        [SerializeField] private float acceleration = 18f;
        [Tooltip("Quão rápido o personagem vira de frente para a direção que está andando.")]
        [SerializeField] private float rotationSpeed = 20f;

        private Rigidbody2D _rb;
        private PlayerStats _stats;
        private Vector2 _moveInput;

        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<PlayerStats>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            EnemyBase.PlayerTarget = transform;
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 keyboardInput = new Vector2(h, v);

            Vector2 joystickInput = (_joystick != null && _joystick.isActiveAndEnabled)
                ? _joystick.Direction
                : Vector2.zero;

            _moveInput = Vector2.ClampMagnitude(keyboardInput + joystickInput, 1f);

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                FacingDirection = _moveInput;
                
                transform.up = Vector3.Slerp(transform.up, _moveInput, rotationSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = _moveInput * _stats.MoveSpeed;
            
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }
}