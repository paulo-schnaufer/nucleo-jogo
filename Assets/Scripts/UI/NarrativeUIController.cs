// NÚCLEO: Última Onda — UI
using UnityEngine;
using DG.Tweening; 
using TMPro;

namespace Nucleo.UI
{
    public class NarrativeUIController : MonoBehaviour
    {
        // ... (MANTENHA TODOS OS SEUS [Headers] DE ABERTURA, VITÓRIA, DERROTA E HUD INTACTOS AQUI) ...
        [Header("Abertura")]
        [SerializeField] private GameObject openingPanel;
        [SerializeField] private TMP_Text openingText;
        [TextArea(3, 6)]
        [SerializeField]
        private string openingLines =
            "Estação Ômega. Núcleo instável. Contenção em colapso.\n" +
            "Processos corrompidos convergem para o núcleo a cada ciclo.\n" +
            "Você é o último protocolo de defesa ativo — não deixe o núcleo cair.";

        [Header("Vitória / Créditos")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryText;
        [TextArea(5, 10)]
        [SerializeField]
        private string victoryAndCreditsLines = "Onda neutralizada... (seu texto original aqui)";

        [Header("Derrota / Créditos")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TMP_Text defeatText;
        [TextArea(5, 10)]
        [SerializeField] 
        private string defeatAndCreditsLines = "Núcleo comprometido... (seu texto original aqui)";

        [Header("HUD")]
        [SerializeField] private GameObject hudPanel;

        [Header("Sincronização de Créditos")]
        [Tooltip("O AudioSource que está tocando a música do jogo")]
        [SerializeField] private AudioSource creditsMusic;
        
        [Tooltip("Arraste aqui o SEU NOVO ARQUIVO DE ÁUDIO (versão 0.9x)")]
        [SerializeField] private AudioClip finalCreditsClip; 
        
        [Tooltip("Até qual posição Y (altura) o texto deve subir?")]
        [SerializeField] private float finalScrollY = 1500f;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            if (openingText != null) openingText.text = openingLines;
            if (openingPanel != null) openingPanel.SetActive(true);
            if (hudPanel != null) hudPanel.SetActive(false);
            Time.timeScale = 0f;
            
            if (openingPanel != null)
            {
                openingPanel.SetActive(true);
                CanvasGroup cg = openingPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = openingPanel.AddComponent<CanvasGroup>();
                
                cg.alpha = 0f;
                cg.DOFade(1f, 1.5f).SetUpdate(true); 
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            if (openingPanel != null && openingPanel.activeSelf && Input.anyKeyDown)
                DismissOpeningPanel();
        }

        public void DismissOpeningPanel()
        {
            if (openingPanel != null) openingPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);
            Time.timeScale = 1f;
        }

        private void HandleGameStateChanged(GameManager.GameState state)
        {
            if (state == GameManager.GameState.Victory)
            {
                if (victoryText != null) victoryText.text = victoryAndCreditsLines;
                if (victoryPanel != null) victoryPanel.SetActive(true);
                if (hudPanel != null) hudPanel.SetActive(false);
                
                // Passe o victoryText direto!
                RollCredits(victoryText); 
            }
            else if (state == GameManager.GameState.GameOver)
            {
                if (defeatText != null) defeatText.text = defeatAndCreditsLines;
                if (defeatPanel != null) defeatPanel.SetActive(true);
                if (hudPanel != null) hudPanel.SetActive(false);
                
                // Passe o defeatText direto!
                RollCredits(defeatText);
            }
        }

        // Agora recebemos o TMP_Text para saber a altura exata das letras!
        private void RollCredits(TMP_Text textComponent)
        {
            if (textComponent == null || creditsMusic == null || finalCreditsClip == null) return;

            AudioSource[] todasAsFontesDeAudio = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in todasAsFontesDeAudio)
            {
                if (audio != creditsMusic) audio.Stop();
            }

            creditsMusic.clip = finalCreditsClip;
            creditsMusic.loop = false;
            creditsMusic.Play();

            float duration = finalCreditsClip.length;
            
            // A MÁGICA: preferredHeight pega o tamanho real do texto gerado!
            // + 800f garante que a última linha passe do centro e saia da tela
            float alturaReal = textComponent.preferredHeight + 800f;

            // Pega o RectTransform do texto para mover
            RectTransform textRect = textComponent.rectTransform;

            textRect.DOAnchorPosY(alturaReal, duration)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);
        }
    }
}