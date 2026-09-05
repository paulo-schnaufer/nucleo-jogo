// NÚCLEO: Última Onda — UI (ver STATUS.md, bloco P1 "UI mínima")
using UnityEngine;

namespace Nucleo.UI
{
    /// <summary>
    /// Cores da paleta fixa do STYLE_GUIDE.md, centralizadas aqui pra nenhum
    /// script de UI hardcodar hex solto (evita a paleta divergir por script).
    /// Ver STYLE_GUIDE.md seção 1 pra origem/uso de cada cor.
    /// </summary>
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