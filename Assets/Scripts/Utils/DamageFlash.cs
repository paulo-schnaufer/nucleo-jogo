using System.Collections;
using UnityEngine;

namespace Nucleo
{
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private float duration = 0.09f;

        private SpriteRenderer _renderer;
        private Color _baseColor;
        private Coroutine _active;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
                _renderer = GetComponentInChildren<SpriteRenderer>();
            if (_renderer != null)
                _baseColor = _renderer.color;
        }

        public void Flash()
        {
            if (_renderer == null) return;
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _renderer.color = Color.white;
            yield return new WaitForSecondsRealtime(duration);
            _renderer.color = _baseColor;
            _active = null;
        }

        private void OnDisable()
        {
            if (_active != null) StopCoroutine(_active);
            if (_renderer != null) _renderer.color = _baseColor;
            _active = null;
        }
    }
}
