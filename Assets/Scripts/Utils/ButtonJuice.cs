using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nucleo.UI
{
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