using System.Collections;
using UnityEngine;

namespace Nucleo
{
    public static class HitStop
    {
        private const float DebounceSeconds = 0.06f;
        private static float _lastTrigger = -999f;
        private static Coroutine _active;
        private static MonoBehaviour _runner;
        private static bool _externalPause;

        public static void Trigger(MonoBehaviour runner, float milliseconds, bool bypassDebounce = false)
        {
            if (runner == null || _externalPause ||
                (!bypassDebounce && Time.unscaledTime - _lastTrigger < DebounceSeconds))
                return;

            _lastTrigger = Time.unscaledTime;
            if (_active != null && _runner != null)
                _runner.StopCoroutine(_active);

            _runner = runner;
            _active = runner.StartCoroutine(Routine(milliseconds / 1000f));
        }

        public static void NotifyExternalPause()
        {
            _externalPause = true;
        }

        public static void NotifyExternalResume()
        {
            _externalPause = false;
        }

        private static IEnumerator Routine(float seconds)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(seconds);

            _active = null;
            if (!_externalPause && (GameManager.Instance == null ||
                                    GameManager.Instance.CurrentState == GameManager.GameState.Playing))
                Time.timeScale = 1f;
        }
    }
}
