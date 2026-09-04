using UnityEngine;
using System.Collections;

public class MaterialColorLerp : MonoBehaviour
{
    private Renderer myRenderer;
    private Material targetMaterial;

    // Cores definidas no enunciado
    public Color corInicial = Color.magenta;
    public Color corFinal = Color.white;
    
    // Velocidade da transição
    public float velocidadeDefasagem = 5f; 

    void Start()
    {
        // Pega o componente Renderer do objeto
        myRenderer = GetComponent<Renderer>();
        
        // .material cria uma cópia única para este objeto não afetar os outros
        targetMaterial = myRenderer.material; 
    }

    // Chame esta função quando o inimigo morrer
    public void IniciarMorte()
    {
        StartCoroutine(TransicionarCor());
    }

    IEnumerator TransicionarCor()
    {
        float progresso = 0f;

        // Se o seu shader usa Particles/Unlit, o nome da propriedade de cor padrão é "_Color"
        // Transiciona suavemente até chegar no Branco puro
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
        // Boa prática: limpa o material clonado da memória ao destruir o objeto
        if (targetMaterial != null)
        {
            Destroy(targetMaterial);
        }
    }
}
