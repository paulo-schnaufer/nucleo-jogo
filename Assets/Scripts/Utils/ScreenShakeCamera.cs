using UnityEngine;

namespace Nucleo
{
    public class ScreenShakeCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (ScreenShake.CurrentOffset != Vector3.zero)
            {
                transform.position += ScreenShake.CurrentOffset;
            }
        }
    }
}