using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nucleo.Player;
using Nucleo.Combat;
using Nucleo.GameFeel;

namespace Nucleo.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();
        [SerializeField] private PlayerProgression playerProgression;
        [SerializeField] private PlayerStats playerStats;
        [Tooltip("Transform filho do jogador onde prefabs de arma escolhidos são instanciados (ex.: Player/WeaponSlots).")]
        [SerializeField] private Transform weaponSlotsRoot;

        private readonly HashSet<UpgradeData> _oneTimeAlreadyPicked = new HashSet<UpgradeData>();
        private int _pendingChoices;

        public event System.Action<List<UpgradeData>> OnChoicesReady;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (playerProgression != null) playerProgression.OnLevelUp += HandleLevelUp;
        }

        private void OnDisable()
        {
            if (playerProgression != null) playerProgression.OnLevelUp -= HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            _pendingChoices++;
            if (_pendingChoices == 1) OpenNextChoice(); 
        }

        private void OpenNextChoice()
        {
            var pool = allUpgrades.Where(u => !(u.oneTimeOnly && _oneTimeAlreadyPicked.Contains(u))).ToList();
            if (pool.Count == 0)
            {
                _pendingChoices = Mathf.Max(0, _pendingChoices - 1);
                return;
            }

            int choiceCount = Mathf.Min(3, pool.Count);
            var choices = pool.OrderBy(_ => Random.value).Take(choiceCount).ToList();

            HitStop.NotifyExternalPause();
            Time.timeScale = 0f;
            OnChoicesReady?.Invoke(choices);
        }

        public void ConfirmChoice(UpgradeData chosen)
        {
            if (chosen == null) return;

            ApplyUpgrade(chosen);
            if (chosen.oneTimeOnly) _oneTimeAlreadyPicked.Add(chosen);

            _pendingChoices = Mathf.Max(0, _pendingChoices - 1);
            if (_pendingChoices > 0)
                OpenNextChoice(); 
            else
            {
                HitStop.NotifyExternalResume();
                Time.timeScale = 1f;
            }
        }

        private void ApplyUpgrade(UpgradeData upgrade)
        {
            Debug.Log($"[Upgrade] Aplicando {upgrade.displayName} ({upgrade.category}/{upgrade.passiveType})");
            if (upgrade.category == UpgradeCategory.Weapon)
            {
                if (upgrade.weaponPrefab != null && weaponSlotsRoot != null)
                {
                    Transform existingWeapon = weaponSlotsRoot.Find(upgrade.weaponPrefab.name + "(Clone)");
                    
                    if (existingWeapon != null)
                    {
                        var orbital = existingWeapon.GetComponent<OrbitalBladesWeapon>();
                        if (orbital != null) 
                        {
                            orbital.AddBlades(1); 
                        }
                    }
                    else
                    {
                        Instantiate(upgrade.weaponPrefab, weaponSlotsRoot);
                    }
                }
                return;
            }

            switch (upgrade.passiveType)
            {
                case PassiveType.MoveSpeed: playerStats.AddMoveSpeed(upgrade.passiveAmountPerPick); break;
                case PassiveType.Damage: playerStats.AddDamageMultiplier(upgrade.passiveAmountPerPick); break;
                case PassiveType.Regen: playerStats.AddRegen(upgrade.passiveAmountPerPick); break;
                case PassiveType.PickupRadius: playerStats.AddPickupRadius(upgrade.passiveAmountPerPick); break;
            }
        }
    }
}