// NÚCLEO: Última Onda — Core Prototype Architecture (ver STATUS.md)
using System;
using UnityEngine;

namespace Nucleo
{
    /// <summary>
    /// XP e nível do jogador. Curva de XP necessário simples (exponencial
    /// leve), ajustável no Inspector. Ao subir de nível, dispara OnLevelUp —
    /// quem escuta (UpgradeManager) é responsável por pausar o jogo e
    /// mostrar as opções de upgrade.
    /// </summary>
    public class PlayerProgression : MonoBehaviour
    {
        [Header("Curva de XP: xpParaProximoNivel = base * mult^(nivel-1)")]
        [SerializeField] private int baseXPToLevel = 5;
        [SerializeField] private float xpCurveMultiplier = 1.35f;

        public int Level { get; private set; } = 1;
        public int CurrentXP { get; private set; }
        public int XPToNextLevel { get; private set; }

        /// <summary>current, toNext — pra UI de barra de XP.</summary>
        public event Action<int, int> OnXPChanged;
        /// <summary>novo nível.</summary>
        public event Action<int> OnLevelUp;

        private void Awake()
        {
            XPToNextLevel = baseXPToLevel;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            CurrentXP += amount;
            // "while" (não "if") de propósito: cobre o caso de ganhar XP
            // suficiente pra subir 2+ níveis de uma vez (ex.: matar o boss).
            while (CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                LevelUp();
            }
            OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
        }

        private void LevelUp()
        {
            Level++;
            XPToNextLevel = Mathf.RoundToInt(baseXPToLevel * Mathf.Pow(xpCurveMultiplier, Level - 1));
            OnLevelUp?.Invoke(Level);
        }
    }
}
