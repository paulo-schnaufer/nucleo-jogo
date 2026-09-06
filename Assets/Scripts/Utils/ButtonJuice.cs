// NÚCLEO: Última Onda — UI
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nucleo.UI
{
    /// <summary>
    /// Micro-interação genérica pra qualquer botão de UI: cresce levemente no hover
    /// e dá um "punch" de escala no clique — mesmo princípio já usado no
    /// RefreshInitialsSlots do NarrativeUIController. Só adicionar esse componente
    /// no mesmo GameObject do Button, sem precisar mexer em código nenhum.
    /// Se o botão tiver um Selectable (Button, Toggle, etc.) e estiver
    /// interactable = false, o hover é ignorado — não faz sentido "convidar" o
    /// jogador a clicar num botão desativado.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Hover")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("Clique (punch)")]
        [SerializeField] private float clickPunchStrength = 0.15f;
        [SerializeField] private float clickPunchDuration = 0.25f;
        [SerializeField] private int clickPunchVibrato = 8;
        [SerializeField] private float clickPunchElasticity = 0.6f;

        private Vector3 _baseScale;
        private Tween _hoverTween;
        private Selectable _selectable;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            // Evita que o botão fique "preso" numa escala esticada se o painel
            // dele for fechado no meio de uma animação de hover/punch.
            _hoverTween?.Kill();
            transform.localScale = _baseScale;
        }

        private bool IsInteractable => _selectable == null || _selectable.interactable;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable) return;

            _hoverTween?.Kill();
            _hoverTween = transform.DOScale(_baseScale * hoverScale, hoverDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverTween?.Kill();
            _hoverTween = transform.DOScale(_baseScale, hoverDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable) return;

            transform.DOPunchScale(Vector3.one * clickPunchStrength, clickPunchDuration, clickPunchVibrato, clickPunchElasticity)
                .SetUpdate(true);
        }
    }
}
