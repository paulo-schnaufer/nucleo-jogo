// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// Orquestra o momento de level-up: pausa o jogo (Time.timeScale = 0),
    /// sorteia 3 upgrades válidos entre os 7 do SCOPE_LOCK.md, espera a UI
    /// avisar qual foi escolhido (via ConfirmChoice) e aplica o efeito.
    ///
    /// Usa uma fila de pendências (_pendingChoices) em vez de um bool: se o
    /// jogador ganhar XP suficiente pra subir 2 níveis no mesmo frame (ex.:
    /// matando o boss), PlayerProgression dispara OnLevelUp duas vezes
    /// seguidas — sem a fila, a segunda escolha seria perdida em silêncio.
    ///
    /// Este script NÃO desenha UI — só a lógica. A UI (painel de 3 cards)
    /// fica pra outra tarefa/sessão; ela só precisa:
    /// 1) escutar OnChoicesReady pra montar os cards;
    /// 2) chamar UpgradeManager.Instance.ConfirmChoice(upgrade) no clique.
    /// </summary>
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

        /// <summary>Avisa a UI pra montar os cards com esta lista (normalmente 3 itens).</summary>
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
            if (_pendingChoices == 1) OpenNextChoice(); // não havia pendência: abre agora
            // Se já havia uma pendência, esta entra na fila e OpenNextChoice()
            // é chamado de novo dentro de ConfirmChoice() quando a atual fechar.
        }

        private void OpenNextChoice()
        {
            var pool = allUpgrades.Where(u => !(u.oneTimeOnly && _oneTimeAlreadyPicked.Contains(u))).ToList();
            if (pool.Count == 0)
            {
                // Nada mais pra oferecer (ex.: as 3 armas já foram pegas e não sobrou passiva configurada).
                _pendingChoices = Mathf.Max(0, _pendingChoices - 1);
                return;
            }

            int choiceCount = Mathf.Min(3, pool.Count);
            var choices = pool.OrderBy(_ => Random.value).Take(choiceCount).ToList();

            HitStop.NotifyExternalPause();
            Time.timeScale = 0f;
            OnChoicesReady?.Invoke(choices);
        }

        /// <summary>Chamado pela UI quando o jogador clica em um dos cards.</summary>
        public void ConfirmChoice(UpgradeData chosen)
        {
            if (chosen == null) return;

            ApplyUpgrade(chosen);
            if (chosen.oneTimeOnly) _oneTimeAlreadyPicked.Add(chosen);

            _pendingChoices = Mathf.Max(0, _pendingChoices - 1);
            if (_pendingChoices > 0)
                OpenNextChoice(); // ainda há level-up pendente: mostra o próximo painel, mantém pausado
            else
            {
                HitStop.NotifyExternalResume();
                Time.timeScale = 1f;
            }
        }

        private void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade.category == UpgradeCategory.Weapon)
            {
                if (upgrade.weaponPrefab != null && weaponSlotsRoot != null)
                {
                    // O Unity adiciona "(Clone)" ao nome quando instancia. Vamos procurar se já existe.
                    Transform existingWeapon = weaponSlotsRoot.Find(upgrade.weaponPrefab.name + "(Clone)");
                    
                    if (existingWeapon != null)
                    {
                        // Se já temos a arma, tentamos acumular os atributos
                        var orbital = existingWeapon.GetComponent<OrbitalBladesWeapon>();
                        if (orbital != null) 
                        {
                            orbital.AddBlades(1); // Adiciona +1 lâmina à formação atual
                        }
                    }
                    else
                    {
                        // Primeira vez pegando a arma, criamos do zero
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
