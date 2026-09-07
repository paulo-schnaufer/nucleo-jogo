using System;
using UnityEngine;

namespace Nucleo.Player
{
    public class PlayerProgression : MonoBehaviour
    {
        [Header("Curva de XP: xpParaProximoNivel = base * mult^(nivel-1)")]
        [SerializeField] private int baseXPToLevel = 5;
        [SerializeField] private float xpCurveMultiplier = 1.35f;

        public int Level { get; private set; } = 1;
        public int CurrentXP { get; private set; }
        public int XPToNextLevel { get; private set; }

        public event Action<int, int> OnXPChanged;
        public event Action<int> OnLevelUp;

        private void Awake()
        {
            XPToNextLevel = baseXPToLevel;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            CurrentXP += amount;

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