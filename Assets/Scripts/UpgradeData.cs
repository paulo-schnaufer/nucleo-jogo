// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using UnityEngine;

namespace Nucleo
{
    public enum UpgradeCategory { Weapon, Passive }
    public enum PassiveType { MoveSpeed, Damage, Regen, PickupRadius }

    /// <summary>
    /// Definição de um upgrade (dos 7 do SCOPE_LOCK.md: 3 armas — torreta
    /// automática, lâminas orbitais, tiro em leque — + 4 passivas —
    /// velocidade, dano, regeneração, raio de coleta).
    ///
    /// Crie um asset por upgrade em Assets > Create > Núcleo > Upgrade Data.
    /// displayName/description aqui são placeholder técnico — nomenclatura
    /// final com metáfora de computação é responsabilidade de
    /// narrativa/UI (SCOPE_LOCK.md, item "desejável"), não deste script.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Núcleo/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public UpgradeCategory category;

        [Header("Se category == Weapon")]
        [Tooltip("Prefab do GameObject de arma a ser instanciado como filho de PlayerController > WeaponSlots.")]
        public GameObject weaponPrefab;

        [Header("Se category == Passive")]
        public PassiveType passiveType;
        public float passiveAmountPerPick = 1f;

        [Header("Regras de oferta — PENDENTE DE CONFIRMAÇÃO (ver resposta desta sessão)")]
        [Tooltip("Se true, só pode ser escolhido uma vez (padrão usado aqui pras 3 armas). Passivas ficam false, pra empilhar em picks futuros.")]
        public bool oneTimeOnly;
    }
}
