using UnityEngine;

namespace Nucleo
{
    [RequireComponent(typeof(PoolItem))]
    public class PooledDamageNumber : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private float riseDistance = 0.7f;
        [SerializeField] private TextMesh textMesh;

        private float _elapsed;
        private Vector3 _startPosition;
        private Vector3 _baseScale;
        private Color _baseColor;

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponentInChildren<TextMesh>();

            if (textMesh != null)
                _baseColor = textMesh.color;

            _baseScale = transform.localScale;
        }

        public void SetDamage(float damage)
        {
            if (textMesh != null)
            {
                textMesh.text = Mathf.RoundToInt(damage).ToString();
                textMesh.color = _baseColor;
            }

            _elapsed = 0f;
            _startPosition = transform.position;
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(_elapsed / lifetime);
            transform.position = _startPosition + Vector3.up * (riseDistance * normalized);

            float pop = normalized < 0.167f
                ? Mathf.Lerp(0.9f, 1.1f, normalized / 0.167f)
                : Mathf.Lerp(1.1f, 1f, Mathf.InverseLerp(0.167f, 0.25f, normalized));
            transform.localScale = _baseScale * pop;

            if (textMesh != null)
            {
                float fadeStart = 0.6f;
                Color color = _baseColor;
                color.a = normalized <= fadeStart
                    ? 1f
                    : 1f - Mathf.InverseLerp(fadeStart, 1f, normalized);
                textMesh.color = color;
            }

            if (_elapsed >= lifetime)
                GetComponent<PoolItem>().ReturnToPool();
        }

        private void OnDisable()
        {
            _elapsed = 0f;
            transform.localScale = _baseScale;
            if (textMesh != null)
                textMesh.color = _baseColor;
        }
    }
}
