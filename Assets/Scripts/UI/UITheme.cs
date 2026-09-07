using UnityEngine;

namespace Nucleo.UI
{
    public static class UITheme
    {
        public static readonly Color CianoBase = Hex("#00E5FF");
        public static readonly Color CianoGlow = Hex("#8FF9FF");
        public static readonly Color MagentaBase = Hex("#FF167A");
        public static readonly Color CoreVioleta = Hex("#B45CFF");
        public static readonly Color Branco = Hex("#F5F7FA");
        public static readonly Color CinzaUI = Hex("#8B93AC");

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}