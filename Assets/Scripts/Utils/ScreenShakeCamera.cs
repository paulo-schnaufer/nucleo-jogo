using UnityEngine;

namespace Nucleo
{
    public class ScreenShakeCamera : MonoBehaviour
    {
        private Vector3 _basePosition;

        private void Awake()
        {
            _basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            transform.localPosition = _basePosition + ScreenShake.CurrentOffset;
        }
    }
}
