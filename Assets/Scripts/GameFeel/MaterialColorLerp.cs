using System.Collections;
using UnityEngine;

namespace Nucleo.GameFeel
{
    public class MaterialColorLerp : MonoBehaviour
    {
        private Renderer myRenderer;
        private Material targetMaterial;

        public Color corInicial = Color.magenta;
        public Color corFinal = Color.white;
        public float velocidadeDefasagem = 5f; 

        void Start()
        {
            myRenderer = GetComponent<Renderer>();
            targetMaterial = myRenderer.material; 
        }

        public void IniciarMorte()
        {
            StartCoroutine(TransicionarCor());
        }

        IEnumerator TransicionarCor()
        {
            float progresso = 0f;

            while (progresso < 1f)
            {
                progresso += Time.deltaTime * velocidadeDefasagem;
                Color novaCor = Color.Lerp(corInicial, corFinal, progresso);
                
                targetMaterial.SetColor("_Color", novaCor);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            if (targetMaterial != null)
            {
                Destroy(targetMaterial);
            }
        }
    }
}
