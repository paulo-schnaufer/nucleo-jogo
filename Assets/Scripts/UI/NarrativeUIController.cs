// NÚCLEO: Última Onda — UI (ver STATUS.md, bloco P1 "UI mínima")
using UnityEngine;
using TMPro;

namespace Nucleo.UI
{
    /// <summary>
    /// Telas de abertura, vitória e derrota. Escopo P1: texto fixo, sem
    /// variantes por Integridade nem easter egg por tipo de inimigo — isso é
    /// P2 (ver STATUS.md). Abertura pausa com o mesmo padrão já usado em
    /// UpgradeManager/GameManager (Time.timeScale = 0).
    /// </summary>
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

        [Header("Vitória")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryText;
        [TextArea(2, 4)]
        [SerializeField]
        private string victoryLine = "Onda neutralizada. Núcleo estável. Estação Ômega seguirá operacional.";

        [Header("Derrota")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TMP_Text defeatText;
        [TextArea(2, 4)]
        [SerializeField]
        private string defeatLine = "Núcleo comprometido. Contenção perdida.\nEstação Ômega — fora do ar.";

        [Header("HUD")]
        [SerializeField] private GameObject hudPanel; // Arraste o GameObject "HUD" aqui no Inspector

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            if (openingText != null) openingText.text = openingLines;
            if (openingPanel != null) openingPanel.SetActive(true);
            if (hudPanel != null) hudPanel.SetActive(false);
            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            // Dismiss "pega e joga": qualquer tecla/clique fecha a abertura,
            // sem precisar de botão dedicado. Input legado (ver DECISIONS.md).
            if (openingPanel != null && openingPanel.activeSelf && Input.anyKeyDown)
                DismissOpeningPanel();
        }

        /// <summary>Também pode ser ligado num OnClick de botão, se preferir.</summary>
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
                if (victoryText != null) victoryText.text = victoryLine;
                if (victoryPanel != null) victoryPanel.SetActive(true);
                if (hudPanel != null) hudPanel.SetActive(false);
            }
            else if (state == GameManager.GameState.GameOver)
            {
                if (defeatText != null) defeatText.text = defeatLine;
                if (defeatPanel != null) defeatPanel.SetActive(true);
                if (hudPanel != null) hudPanel.SetActive(false);
            }
            // GameManager já coloca Time.timeScale = 0 antes de disparar este
            // evento (ver GameManager.EndGame) — não repetir aqui.
        }
    }
}