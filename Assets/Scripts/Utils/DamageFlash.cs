using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nucleo
{
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private float duration = 0.06f;
        [Tooltip("Escala temporária durante o impacto (1.15 = 15% maior).")]
        [SerializeField] private float popScale = 1.15f;

        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<Material> _originalMaterials = new List<Material>();
        private static Material _whiteFlashMaterial;
        private Coroutine _active;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;

            if (_whiteFlashMaterial == null)
            {
                Shader flashShader = Shader.Find("GUI/Text Shader");
                if (flashShader != null)
                    _whiteFlashMaterial = new Material(flashShader);
            }
            FetchRenderers();
        }

        private void FetchRenderers()
        {
            _renderers.Clear();
            _originalMaterials.Clear();

            var found = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in found)
            {
                _renderers.Add(r);
                _originalMaterials.Add(r.sharedMaterial);
            }
        }

        public void Flash()
        {
            if (_renderers.Count == 0) FetchRenderers();
            if (_renderers.Count == 0 || _whiteFlashMaterial == null) return;

            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            transform.localScale = _baseScale * popScale;

            for (int i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].material = _whiteFlashMaterial;
            }

            yield return new WaitForSecondsRealtime(duration);

            ResetMaterials();
        }

        private void ResetMaterials()
        {
            transform.localScale = _baseScale;

            for (int i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].material = _originalMaterials[i];
            }
            _active = null;
        }

        private void OnDisable()
        {
            if (_active != null) StopCoroutine(_active);
            ResetMaterials();
        }
    }
}