using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // Importação correta para o Cinemachine novo

namespace Nucleo
{
    public class CameraZoom : MonoBehaviour
    {
        public static CameraZoom Instance { get; private set; }

        [Header("Configuração")]
        [Tooltip("Arraste a sua Cinemachine Camera aqui")]
        [SerializeField] private CinemachineCamera vcam; // Componente atualizado
        
        private float _defaultSize;
        private Coroutine _zoomRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (vcam != null)
            {
                // No Cinemachine 3, usamos Lens em vez de m_Lens
                _defaultSize = vcam.Lens.OrthographicSize; 
            }
        }

        public void SetZoom(float targetSize, float duration = 1.2f)
        {
            if (vcam == null) return;

            if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
            _zoomRoutine = StartCoroutine(ZoomRoutine(targetSize, duration));
        }

        public void ResetZoom(float duration = 1.2f)
        {
            SetZoom(_defaultSize, duration);
        }

        private IEnumerator ZoomRoutine(float targetSize, float duration)
        {
            float startSize = vcam.Lens.OrthographicSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                vcam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
                yield return null;
            }

            vcam.Lens.OrthographicSize = targetSize;
            _zoomRoutine = null;
        }
    }
}