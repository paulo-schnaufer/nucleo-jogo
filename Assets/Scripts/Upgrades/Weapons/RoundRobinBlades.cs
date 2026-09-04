using System.Collections.Generic;
using UnityEngine;

namespace Nucleo.Upgrades
{
    /// <summary>
    /// Lâminas orbitais — metáfora "Round-Robin": mesmo nome do algoritmo de
    /// escalonamento que dá uma fatia de tempo pra cada processo em turnos cíclicos;
    /// aqui, cada lâmina ocupa seu turno/posição girando ao redor do jogador.
    ///
    /// Uso dos dados de nível: primaryValue = número de lâminas ativas,
    /// secondaryValue = dano por contato de cada lâmina.
    ///
    /// ASSUNÇÃO: prefab de lâmina tem um BladeHit (deste pacote) com Collider2D trigger.
    /// </summary>
    public class RoundRobinBlades : MonoBehaviour, IUpgradableWeapon
    {
        [SerializeField] private GameObject bladePrefab;
        [SerializeField] private float orbitRadius = 1.4f;
        [SerializeField] private float angularSpeedDegPerSec = 90f;
        [SerializeField] private int initialBladeCount = 1;
        [SerializeField] private float initialDamage = 10f;

        private readonly List<Transform> _blades = new List<Transform>();
        private float _damage;

        private void Awake()
        {
            ApplyLevel(1, new UpgradeLevelData
            {
                primaryValue = initialBladeCount,
                secondaryValue = initialDamage
            });
        }

        public void ApplyLevel(int level, UpgradeLevelData data)
        {
            int desiredCount = Mathf.RoundToInt(data.primaryValue);
            _damage = data.secondaryValue;

            SetBladeCount(desiredCount);
            ApplyDamageToAllBlades();
        }

        private void SetBladeCount(int desiredCount)
        {
            if (bladePrefab == null) return;

            while (_blades.Count < desiredCount)
            {
                var blade = Instantiate(bladePrefab, transform.position, Quaternion.identity, transform);
                _blades.Add(blade.transform);
            }
            // Não remove lâminas se desiredCount cair — não deveria acontecer, já que
            // upgrades só sobem de nível, nunca descem.
        }

        private void ApplyDamageToAllBlades()
        {
            foreach (var blade in _blades)
            {
                var bladeHit = blade.GetComponent<BladeHit>();
                if (bladeHit != null) bladeHit.SetDamage(_damage);
            }
        }

        private void Update()
        {
            if (_blades.Count == 0) return;

            float step = 360f / _blades.Count;
            float baseAngle = Time.time * angularSpeedDegPerSec;

            for (int i = 0; i < _blades.Count; i++)
            {
                float angleRad = (baseAngle + step * i) * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitRadius;
                _blades[i].position = (Vector2)transform.position + offset;
            }
        }
    }
}
