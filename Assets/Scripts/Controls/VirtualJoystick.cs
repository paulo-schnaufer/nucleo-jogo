using UnityEngine;
using UnityEngine.EventSystems;

namespace Nucleo.Controls
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _handleRange = 100f; 

        [Header("Juice")]
        [SerializeField, Range(0f, 0.5f)] private float _deadZone = 0.15f; 
        [SerializeField] private float _returnSpeed = 20f; 
        [SerializeField] private float _pressedScale = 1.1f; 
        [SerializeField] private bool _vibrateOnPress = true;

        public Vector2 Direction { get; private set; } = Vector2.zero;

        private bool _gameplayVisible = false;
        private bool _isReturning = false;

        private void Awake()
        {
            ApplyVisibility();
        }

        public void SetVisible(bool visible)
        {
            _gameplayVisible = visible;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            bool shouldShow = Application.isMobilePlatform && _gameplayVisible;

            if (!shouldShow)
            {
                Direction = Vector2.zero;
                _isReturning = false;
                if (_handle != null) _handle.anchoredPosition = Vector2.zero;
                if (_background != null) _background.localScale = Vector3.one;
            }

            gameObject.SetActive(shouldShow);
        }

        private void Update()
        {
            if (!_isReturning) return;

            _handle.anchoredPosition = Vector2.Lerp(
                _handle.anchoredPosition, Vector2.zero, Time.deltaTime * _returnSpeed);

            if (_handle.anchoredPosition.sqrMagnitude < 0.1f)
            {
                _handle.anchoredPosition = Vector2.zero;
                _isReturning = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isReturning = false;

            if (_background != null) _background.localScale = Vector3.one * _pressedScale;

            if (_vibrateOnPress && Application.isMobilePlatform)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                #endif
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, eventData.pressEventCamera, out localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, _handleRange);
            _handle.anchoredPosition = clamped;

            float normalizedMagnitude = clamped.magnitude / _handleRange;
            Direction = normalizedMagnitude < _deadZone ? Vector2.zero : clamped / _handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            if (_background != null) _background.localScale = Vector3.one;
            _isReturning = true;
        }
    }
}