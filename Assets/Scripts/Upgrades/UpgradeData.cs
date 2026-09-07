using UnityEngine;

namespace Nucleo.Upgrades
{
    public enum UpgradeCategory { Weapon, Passive }
    public enum PassiveType { MoveSpeed, Damage, Regen, PickupRadius }

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

        [Header("Regras de oferta")]
        [Tooltip("Se true, só pode ser escolhido uma vez. Passivas ficam false, pra empilhar em picks futuros.")]
        public bool oneTimeOnly;
    }
}