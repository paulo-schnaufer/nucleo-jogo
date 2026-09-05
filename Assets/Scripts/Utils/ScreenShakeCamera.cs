using UnityEngine;

namespace Nucleo
{
    // Executa imediatamente APÓS o CinemachineBrain atualizar a câmera no LateUpdate
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