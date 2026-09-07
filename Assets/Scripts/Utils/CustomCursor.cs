using UnityEngine;

namespace Nucleo.UI
{
    public class CustomCursor : MonoBehaviour
    {
        [Tooltip("Textura do cursor. Import Settings: Texture Type = Cursor (ou Sprite/2D com Read/Write ativado).")]
        [SerializeField] private Texture2D cursorTexture;

        [Tooltip("Ponto da textura que corresponde à 'ponta' do cursor (em pixels, a partir do canto superior esquerdo). Pra uma seta, geralmente (0,0). Pra uma mira/crosshair, o centro da textura.")]
        [SerializeField] private Vector2 hotspot = Vector2.zero;

        [Tooltip("ForceSoftware força o Unity a desenhar o cursor por conta própria (funciona igual em qualquer plataforma/resolução, mas tem custo de performance mínimo). Auto deixa o SO renderizar quando possível.")]
        [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

        private void Start()
        {
            ApplyCursor();
        }

        private void ApplyCursor()
        {
            if (cursorTexture == null) return;
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        }

        private void OnDestroy()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}