using System.Collections;
using UnityEngine;

namespace Nucleo.GameFeel
{
    [DefaultExecutionOrder(10000)]
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