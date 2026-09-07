using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nucleo.UI
{
    public class CoreIntegrityVignette : MonoBehaviour
    {
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private Health playerHealth;
        [SerializeField] private Image vignetteImage;

        [Tooltip("Cor da vinheta (o alpha é controlado à parte, não precisa mexer aqui).")]
        [SerializeField] private Color vignetteColor = new Color(0.8f, 0.05f, 0.05f);

        [Tooltip("Alpha máximo da vinheta com Integridade a 0%. Evita usar 1 cheio pra não tampar o jogo — 0.5~0.6 já lê bem como alerta.")]
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.55f;

        [Tooltip("Acima desse % de Integridade a vinheta fica em 0 — só aparece quando o perigo é real, sem 'sujar' a tela o jogo inteiro.")]
        [SerializeField, Range(0f, 1f)] private float startThreshold = 0.5f;

        [Tooltip("Abaixo desse % a vinheta entra em modo crítico e passa a pulsar (fade in/out em loop) em vez de só ficar estática.")]
        [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.2f;

        [SerializeField] private float smoothDuration = 0.4f;
        [SerializeField] private float pulseDuration = 0.6f;

        private Tween _tween;
        private bool _isPulsing;
        private bool _suppressed;

        public void SetSuppressed(bool suppressed)
        {
            _suppressed = suppressed;
            if (!suppressed || vignetteImage == null) return;

            _tween?.Kill();
            _isPulsing = false;
            _tween = vignetteImage.DOFade(0f, smoothDuration).SetUpdate(true);
        }

        private void Awake()
        {
            if (vignetteImage != null)
                vignetteImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
        }

        private void Update()
        {
            if (_suppressed || vignetteImage == null) return;

            float percent = Mathf.Min(GetPercent(coreIntegrity?.Health), GetPercent(playerHealth));
            bool shouldPulse = percent > 0f && percent <= criticalThreshold;

            if (shouldPulse != _isPulsing)
                SetPulsing(shouldPulse);

            if (!shouldPulse)
            {
                float targetAlpha = percent >= startThreshold
                    ? 0f
                    : Mathf.Lerp(maxAlpha, 0f, Mathf.InverseLerp(0f, startThreshold, percent));

                _tween?.Kill();
                _tween = vignetteImage.DOFade(targetAlpha, smoothDuration).SetUpdate(true);
            }
        }

        private static float GetPercent(Health health)
        {
            if (health == null || health.MaxHP <= 0f) return 1f;
            return health.CurrentHP / health.MaxHP;
        }

        private void SetPulsing(bool pulsing)
        {
            _isPulsing = pulsing;
            _tween?.Kill();

            if (pulsing)
            {
                _tween = vignetteImage.DOFade(maxAlpha, pulseDuration)
                    .SetUpdate(true)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }
    }
}