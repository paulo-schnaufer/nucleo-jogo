using System.Collections;
using UnityEngine;

namespace Nucleo
{
    public static class ScreenShake
    {
        public static Vector3 CurrentOffset { get; private set; }

        private static Coroutine _active;
        private static MonoBehaviour _runner;
        private static float _activeAmplitude;
        private static float _activeDuration;

        public static void Trigger(MonoBehaviour runner, float amplitude, float duration)
        {
            if (runner == null || amplitude <= 0f || duration <= 0f)
                return;

            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<ScreenShakeCamera>() == null)
                mainCamera.gameObject.AddComponent<ScreenShakeCamera>();

            // Lendo a variável _active aqui para pará-la (isso remove o warning CS0414)
            if (_active != null && _runner != null)
                _runner.StopCoroutine(_active);

            _runner = runner;
            _activeAmplitude = Mathf.Max(_activeAmplitude, amplitude);
            _activeDuration = Mathf.Max(_activeDuration, duration);
            
            // Atribuindo valor à variável _active aqui
            _active = runner.StartCoroutine(Routine());
        }

        private static IEnumerator Routine()
        {
            float amplitude = _activeAmplitude;
            float duration = _activeDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float falloff = 1f - normalized * normalized;
                CurrentOffset = (Vector3)(Random.insideUnitCircle * amplitude * falloff);
                yield return null;
            }

            CurrentOffset = Vector3.zero;
            _activeAmplitude = 0f;
            _activeDuration = 0f;
            _active = null;
        }
    }
}