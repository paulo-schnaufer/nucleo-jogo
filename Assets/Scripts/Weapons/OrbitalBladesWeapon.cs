// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System.Collections.Generic;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Lâminas orbitais: um dos 3 upgrades de arma do SCOPE_LOCK.md (nome de
    /// exibição sugerido nesta sessão: "Buffer Circular" — estrutura de dados
    /// com N posições fixas dispostas em círculo, igual às lâminas aqui).
    ///
    /// Diferente de AutoTurretWeapon/FanShotWeapon: NÃO usa Projectile nem
    /// ObjectPool, porque as lâminas não são disparadas — elas existem o
    /// tempo todo, giram ao redor do jogador e causam dano por contato
    /// (Physics2D trigger), com um cooldown por par (lâmina, inimigo) pra não
    /// acumular múltiplos hits enquanto o collider fica sobreposto.
    ///
    /// Vira prefab referenciado em UpgradeData.weaponPrefab (mesmo fluxo das
    /// outras 2 armas), instanciado como filho de Player/WeaponSlots — nenhuma
    /// mudança em UpgradeManager/UpgradeData foi necessária pra isso funcionar.
    ///
    /// ATENÇÃO — bloqueio real, não decisão de arquitetura (ver STATUS.md
    /// desta sessão): este script assume a existência de
    /// Health.TakeDamage(float amount, GameObject source). Health.cs não
    /// estava na lista de leitura obrigatória desta tarefa (só PlayerStats,
    /// UpgradeManager, UpgradeData, AutoTurretWeapon) e nenhum desses 4
    /// arquivos chama esse método diretamente — quem causa dano ali é sempre
    /// Projectile, que também não lemos. CONFIRMAR a assinatura real de
    /// Health antes de compilar este arquivo.
    /// </summary>
    public class OrbitalBladesWeapon : MonoBehaviour
    {
        [Header("Órbita")]
        [SerializeField] private int bladeCount = 3;
        [SerializeField] private float orbitRadius = 1.8f;
        [SerializeField] private float rotationSpeedDegPerSec = 120f;

        [Header("Dano")]
        [SerializeField] private float baseDamage = 4f;
        [Tooltip("Tempo mínimo entre dois hits da MESMA lâmina no MESMO inimigo (evita dano repetido enquanto o collider fica sobreposto).")]
        [SerializeField] private float perEnemyHitCooldown = 0.5f;

        [Header("Visual/Física")]
        [Tooltip("Prefab pequeno com SpriteRenderer + Collider2D (Is Trigger = true) representando 1 lâmina.")]
        [SerializeField] private GameObject bladeVisualPrefab;
        [SerializeField] private LayerMask enemyLayer;

        private readonly List<Transform> _blades = new List<Transform>();

        // Chave composta (lâmina, inimigo): cooldown por PAR, não só por
        // inimigo — assim lâminas diferentes não "roubam" o cooldown umas das
        // outras quando várias se sobrepõem no mesmo alvo.
        private readonly Dictionary<(Transform blade, Health enemy), float> _lastHitTime =
            new Dictionary<(Transform, Health), float>();

        private PlayerStats _ownerStats;

        private void Awake()
        {
            // Mesma hierarquia das outras armas: instanciada como filha do jogador.
            _ownerStats = GetComponentInParent<PlayerStats>();
            SpawnBlades();
        }

        private void SpawnBlades()
        {
            if (bladeVisualPrefab == null || bladeCount <= 0) return;

            for (int i = 0; i < bladeCount; i++)
            {
                float angle = (360f / bladeCount) * i;
                Vector2 offset = RotateVector(Vector2.right * orbitRadius, angle);

                var bladeGO = Instantiate(bladeVisualPrefab, transform);
                bladeGO.transform.localPosition = offset;

                var contact = bladeGO.GetComponent<OrbitalBladeContact>();
                if (contact == null) contact = bladeGO.AddComponent<OrbitalBladeContact>();
                contact.Init(this);

                _blades.Add(bladeGO.transform);
            }
        }

        public void AddBlades(int amount)
        {
            bladeCount += amount;
            
            // Destrói os objetos das lâminas antigas
            foreach (Transform blade in _blades)
            {
                if (blade != null) Destroy(blade.gameObject);
            }
            _blades.Clear();
            
            // Recria as lâminas com a nova quantidade distribuída em 360 graus
            SpawnBlades();
        }
        private void Update()
        {
            // Mesma guarda das outras armas: painel de upgrade pausa via timeScale.
            if (Time.timeScale <= 0f) return;
            transform.Rotate(Vector3.forward, rotationSpeedDegPerSec * Time.deltaTime);
        }

        /// <summary>Chamado por OrbitalBladeContact quando a lâmina toca um Collider2D.</summary>
        public void HandleBladeHit(Transform blade, Collider2D other)
        {
            if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

            var health = other.GetComponent<Health>();
            if (health == null || health.IsDead) return;

            var key = (blade, health);
            if (_lastHitTime.TryGetValue(key, out float lastTime) && Time.time - lastTime < perEnemyHitCooldown)
                return;

            float dmgMultiplier = _ownerStats != null ? _ownerStats.DamageMultiplier : 1f;

            // TODO(verify): confirmar assinatura real do método de dano em Health.cs
            // antes de compilar (ver observação no cabeçalho deste arquivo e no
            // STATUS.md desta sessão).
            health.TakeDamage(baseDamage * dmgMultiplier, gameObject);

            _lastHitTime[key] = Time.time;
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }

    /// <summary>
    /// Componente minúsculo que fica em CADA lâmina filha — só repassa o
    /// trigger pro peso da lógica em OrbitalBladesWeapon (evita duplicar
    /// GetComponentInParent + lógica de cooldown em cada lâmina).
    /// </summary>
    public class OrbitalBladeContact : MonoBehaviour
    {
        private OrbitalBladesWeapon _owner;

        public void Init(OrbitalBladesWeapon owner) => _owner = owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            _owner?.HandleBladeHit(transform, other);
        }
    }
}
