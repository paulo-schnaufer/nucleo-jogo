// NÚCLEO: Última Onda — UI
using UnityEngine;
using DG.Tweening; 
using UnityEngine.UI;
using TMPro;

namespace Nucleo.UI
{
    public class NarrativeUIController : MonoBehaviour
    {
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
        [SerializeField] private TMP_Text victoryThanksLabel; 
        [SerializeField] private Image victoryBlackFade;      
        [TextArea(5, 10)]
        [SerializeField] private string victoryAndCreditsLines = "...";

        [Header("Derrota / Créditos")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TMP_Text defeatText;
        [SerializeField] private TMP_Text defeatThanksLabel; 
        [SerializeField] private Image defeatBlackFade;      
        [TextArea(5, 10)]
        [SerializeField] private string defeatAndCreditsLines = "...";

        [Header("HUD")]
        [SerializeField] private GameObject hudPanel;

        [Header("Sincronização de Créditos")]
        [Tooltip("O AudioSource que está tocando a música do jogo")]
        [SerializeField] private AudioSource creditsMusic;
        [Tooltip("Arraste aqui o SEU NOVO ARQUIVO DE ÁUDIO (versão 0.9x)")]
        [SerializeField] private AudioClip finalCreditsClip; 

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
                
                RollCredits(victoryText, victoryThanksLabel, victoryBlackFade); 
            }
            else if (state == GameManager.GameState.GameOver)
            {
                if (defeatText != null) defeatText.text = defeatAndCreditsLines;
                if (defeatPanel != null) defeatPanel.SetActive(true);
                if (hudPanel != null) hudPanel.SetActive(false);
                
                RollCredits(defeatText, defeatThanksLabel, defeatBlackFade);
            }
        }

        private void RollCredits(TMP_Text mainText, TMP_Text thanksLabel, Image blackScreenFade)
        {
            if (mainText == null || creditsMusic == null || finalCreditsClip == null) return;

            // NOVO: Código atualizado para a versão mais recente da Unity sem dar aviso amarelo
            AudioSource[] todasAsFontesDeAudio = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
            foreach (AudioSource audio in todasAsFontesDeAudio)
            {
                if (audio != creditsMusic) audio.Stop();
            }

            creditsMusic.clip = finalCreditsClip;
            creditsMusic.loop = false;
            creditsMusic.Play();

            float duration = finalCreditsClip.length;
            float alturaReal = mainText.preferredHeight + 800f;
            
            RectTransform mainRect = mainText.rectTransform;
            float startY = mainRect.anchoredPosition.y;

            float distanciaTotal = alturaReal - startY;
            float velocidade = distanciaTotal / duration;

            mainRect.DOAnchorPosY(alturaReal, duration)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);

            if (thanksLabel != null)
            {
                RectTransform thanksRect = thanksLabel.rectTransform;
                
                // NOVO: Removemos a divisão por 2. Agora subtraímos a altura INTEIRA do texto principal
                // Se ainda ficar sobreposto, você pode aumentar esse "- 300f" para "- 600f"
                float thanksStartY = startY - mainText.preferredHeight - 300f; 
                thanksRect.anchoredPosition = new Vector2(thanksRect.anchoredPosition.x, thanksStartY);

                float distanciaThanks = 0 - thanksStartY;
                float thanksDuration = distanciaThanks / velocidade;

                thanksRect.DOAnchorPosY(0, thanksDuration)
                          .SetEase(Ease.Linear)
                          .SetUpdate(true);
            }

            if (blackScreenFade != null)
            {
                blackScreenFade.color = new Color(0, 0, 0, 0);
                blackScreenFade.gameObject.SetActive(true);

                float tempoFade = 5f;
                float tempoEspera = duration - tempoFade;

                blackScreenFade.DOFade(1f, tempoFade)
                               .SetDelay(tempoEspera)
                               .SetUpdate(true);
            }
        }
    }
}