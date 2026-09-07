using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using Nucleo.Controls;
using Nucleo.Player;

namespace Nucleo.UI
{
    public class NarrativeUIController : MonoBehaviour
    {
        [Header("Ranking (tela inicial)")]
        [SerializeField] private GameObject leaderboardPanel;
        [Tooltip("Usado só como mensagem de estado vazio ('Nenhum recorde ainda'). As linhas de fato são instanciadas via leaderboardRowPrefab.")]
        [SerializeField] private TMP_Text leaderboardText;
        [Tooltip("Prefab de uma linha do ranking — precisa ter o componente LeaderboardRowController.")]
        [SerializeField] private LeaderboardRowController leaderboardRowPrefab;
        [Tooltip("Container com Vertical Layout Group onde as linhas do ranking são instanciadas.")]
        [SerializeField] private Transform leaderboardRowsParent;

        [Header("Pontuação")]
        [Tooltip("Mesmo EnemySpawner usado no HUD — usado pra calcular a pontuação final.")]
        [SerializeField] private EnemySpawner spawner;
        [Tooltip("Nível do jogador ao fim da run — entra na fórmula de pontuação.")]
        [SerializeField] private PlayerProgression playerProgression;
        [Tooltip("Health do jogador — % de HP restante entra na fórmula de pontuação.")]
        [SerializeField] private Health playerHealth;
        [Tooltip("CoreIntegrity do Núcleo — % de Integridade restante entra na fórmula de pontuação.")]
        [SerializeField] private CoreIntegrity coreIntegrity;

        [Header("Pesos da Pontuação (calibração)")]
        [Tooltip("Pontos por wave alcançada. Deve ficar bem acima da soma máxima dos outros bônus, pra wave continuar sendo o fator dominante do ranking.")]
        [SerializeField] private int pontosPorWave = 1000;
        [Tooltip("Pontos por nível de jogador ao fim da run.")]
        [SerializeField] private int pontosPorNivel = 40;
        [Tooltip("Bônus máximo (com Núcleo a 100% de Integridade) pela defesa do objetivo principal.")]
        [SerializeField] private int bonusMaximoNucleo = 100;
        [Tooltip("Bônus máximo (com jogador a 100% de HP) pela sobrevivência pessoal.")]
        [SerializeField] private int bonusMaximoJogador = 50;

        [Header("Entrada de iniciais (top 10)")]
        [SerializeField] private GameObject initialsPanel;
        [SerializeField] private TMP_Text finalScoreLabel;
        [SerializeField] private TMP_InputField initialsInput;
        [SerializeField] private Button confirmInitialsButton;
        [Tooltip("4 caixinhas de texto (uma por letra) — a quantidade precisa bater com o characterLimit do initialsInput.")]
        [SerializeField] private TMP_Text[] initialsSlotLabels;
        [Tooltip("Fundo/contorno de cada caixinha, usado pra destacar qual slot está ativo no momento.")]
        [SerializeField] private Image[] initialsSlotBackgrounds;
        [SerializeField] private Color corSlotInativo = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color corSlotAtivo = new Color(0.68f, 0.42f, 1f, 0.35f);

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
        [Tooltip("Vinheta de perigo (Núcleo/HP do jogador) — suprimida automaticamente fora de gameplay, junto com o hudPanel.")]
        [SerializeField] private CoreIntegrityVignette coreIntegrityVignette;

        [Header("Joystick (mobile)")]
        [Tooltip("Arraste aqui o GameObject 'Joystick_Background' (o que tem o script VirtualJoystick). Só aparece na luta: some no ranking, na abertura (opcional), na digitação de nome e em vitória/derrota.")]
        [SerializeField] private VirtualJoystick virtualJoystick;

        [Header("Sincronização de Créditos")]
        [Tooltip("O AudioSource que está tocando a música do jogo")]
        [SerializeField] private AudioSource creditsMusic;
        [Tooltip("Arraste aqui o SEU NOVO ARQUIVO DE ÁUDIO (versão 0.9x)")]
        [SerializeField] private AudioClip finalCreditsClip;

        [Header("Créditos — Skip")]
        [SerializeField] private Button skipCreditsButtonVictory;
        [SerializeField] private Button skipCreditsButtonDefeat;
        [Tooltip("Tecla alternativa pro skip, além dos botões na tela.")]
        [SerializeField] private KeyCode skipCreditsKey = KeyCode.Escape;

        private bool _isRollingCredits;
        private Coroutine _creditsRoutine;
        private TMP_Text _activeMainText;
        private TMP_Text _activeThanksLabel;
        private Image _activeBlackFade;
        private Button _activeSkipButton;

        private int _highestWave;
        private int _pendingScore;
        private int _finalScore;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            if (spawner != null)
                spawner.OnWaveStarted += HandleWaveStartedForScore;

            if (openingText != null) openingText.text = openingLines;

            RenderLeaderboard();

            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(true);
                PlayPanelOpenAnimation(leaderboardPanel);
            }
            if (openingPanel != null) openingPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (coreIntegrityVignette != null) coreIntegrityVignette.SetSuppressed(true);
            if (initialsPanel != null) initialsPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            if (virtualJoystick != null) virtualJoystick.SetVisible(false);

            if (confirmInitialsButton != null)
            {
                confirmInitialsButton.onClick.RemoveAllListeners();
                confirmInitialsButton.onClick.AddListener(ConfirmInitials);
            }

            if (skipCreditsButtonVictory != null)
            {
                skipCreditsButtonVictory.onClick.RemoveAllListeners();
                skipCreditsButtonVictory.onClick.AddListener(SkipCredits);
                skipCreditsButtonVictory.gameObject.SetActive(false);
            }
            if (skipCreditsButtonDefeat != null)
            {
                skipCreditsButtonDefeat.onClick.RemoveAllListeners();
                skipCreditsButtonDefeat.onClick.AddListener(SkipCredits);
                skipCreditsButtonDefeat.gameObject.SetActive(false);
            }

            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

            if (spawner != null)
                spawner.OnWaveStarted -= HandleWaveStartedForScore;
        }

        private void Update()
        {
            if (leaderboardPanel != null && leaderboardPanel.activeSelf && Input.anyKeyDown)
            {
                leaderboardPanel.SetActive(false);
                ShowOpeningPanel();
                return;
            }

            if (openingPanel != null && openingPanel.activeSelf && Input.anyKeyDown)
            {
                DismissOpeningPanel();
                return;
            }

            if (initialsPanel != null && initialsPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
                ConfirmInitials();
            
            if (_isRollingCredits && Input.GetKeyDown(skipCreditsKey))
                SkipCredits();  
        }

        private void ShowOpeningPanel()
        {
            if (openingPanel == null) return;

            openingPanel.SetActive(true);
            CanvasGroup cg = openingPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = openingPanel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 1.5f).SetUpdate(true);
        }

        public void DismissOpeningPanel()
        {
            if (openingPanel != null) openingPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);
            if (coreIntegrityVignette != null) coreIntegrityVignette.SetSuppressed(false);

            if (virtualJoystick != null) virtualJoystick.SetVisible(true);

            Time.timeScale = 1f;
        }

        private void PlayPanelOpenAnimation(GameObject panel, bool celebratory = false)
        {
            if (panel == null) return;

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f).SetUpdate(true);

            Transform t = panel.transform;
            t.localScale = Vector3.one * (celebratory ? 0.8f : 0.96f);
            t.DOScale(1f, celebratory ? 0.4f : 0.2f)
             .SetEase(celebratory ? Ease.OutBack : Ease.OutQuad)
             .SetUpdate(true);
        }

        private void PlayPanelCloseAnimation(GameObject panel, float duration, TweenCallback onComplete = null)
        {
            if (panel == null || !panel.activeSelf)
            {
                onComplete?.Invoke();
                return;
            }

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            cg.DOKill();
            cg.DOFade(0f, duration).SetUpdate(true).OnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void HandleGameStateChanged(GameManager.GameState state)
        {
            if (state == GameManager.GameState.Victory || state == GameManager.GameState.GameOver)
            {
                _finalScore = GetFinalScore();
                if (coreIntegrityVignette != null) coreIntegrityVignette.SetSuppressed(true);

                if (virtualJoystick != null) virtualJoystick.SetVisible(false);
            }

            if (state == GameManager.GameState.Victory)
            {
                if (victoryText != null) victoryText.text = victoryAndCreditsLines;
                if (victoryPanel != null)
                {
                    victoryPanel.SetActive(true);
                    PlayPanelOpenAnimation(victoryPanel, celebratory: true);
                }
                if (hudPanel != null) hudPanel.SetActive(false);
                    _creditsRoutine = StartCoroutine(RollCreditsThenFinish(victoryText, victoryThanksLabel, victoryBlackFade, skipCreditsButtonVictory));            }
            
            else if (state == GameManager.GameState.GameOver)
            {
                if (defeatText != null) defeatText.text = defeatAndCreditsLines;
                if (defeatPanel != null)
                {
                    defeatPanel.SetActive(true);
                    PlayPanelOpenAnimation(defeatPanel);
                }
                if (hudPanel != null) hudPanel.SetActive(false);
                    _creditsRoutine = StartCoroutine(RollCreditsThenFinish(defeatText, defeatThanksLabel, defeatBlackFade, skipCreditsButtonDefeat));            }
        }

        private IEnumerator RollCreditsThenFinish(TMP_Text mainText, TMP_Text thanksLabel, Image blackScreenFade, Button skipButton)
        {
            _isRollingCredits = true;
            _activeMainText = mainText;
            _activeThanksLabel = thanksLabel;
            _activeBlackFade = blackScreenFade;
            _activeSkipButton = skipButton;

            if (skipButton != null) skipButton.gameObject.SetActive(true);

            RollCredits(mainText, thanksLabel, blackScreenFade);

            float wait = finalCreditsClip != null ? finalCreditsClip.length : 3f;
            yield return new WaitForSecondsRealtime(wait);

            FinishCreditsSequence();
        }

        public void SkipCredits()
        {
            if (!_isRollingCredits) return;

            if (_creditsRoutine != null) StopCoroutine(_creditsRoutine);
            FinishCreditsSequence();
        }

        private void FinishCreditsSequence()
        {
            _isRollingCredits = false;
            if (_activeSkipButton != null) _activeSkipButton.gameObject.SetActive(false);

            _activeMainText?.rectTransform.DOKill();
            _activeThanksLabel?.rectTransform.DOKill();
            _activeBlackFade?.DOKill();
            if (creditsMusic != null) creditsMusic.Stop();

            FinishRun();
        }

        private void HandleWaveStartedForScore(int waveIndex)
        {
            _highestWave = waveIndex + 1;
        }

        private int GetFinalScore()
        {
            int wave = _highestWave;
            int level = playerProgression != null ? playerProgression.Level : 1;

            float corePercent = (coreIntegrity != null && coreIntegrity.Health != null && coreIntegrity.Health.MaxHP > 0f)
                ? coreIntegrity.Health.CurrentHP / coreIntegrity.Health.MaxHP
                : 0f;

            float playerPercent = (playerHealth != null && playerHealth.MaxHP > 0f)
                ? playerHealth.CurrentHP / playerHealth.MaxHP
                : 0f;

            int waveScore = wave * pontosPorWave;
            int levelScore = level * pontosPorNivel;
            int coreBonus = Mathf.RoundToInt(corePercent * bonusMaximoNucleo);
            int playerBonus = Mathf.RoundToInt(playerPercent * bonusMaximoJogador);

            return waveScore + levelScore + coreBonus + playerBonus;
        }

        private void FinishRun()
        {
            int finalScore = _finalScore;
            bool qualifiesForRanking = HighScoreManager.QualifiesForTopScores(finalScore);

            GameObject endGamePanel =
                (victoryPanel != null && victoryPanel.activeSelf) ? victoryPanel :
                (defeatPanel != null && defeatPanel.activeSelf) ? defeatPanel :
                null;

            PlayPanelCloseAnimation(endGamePanel, 0.35f, () =>
            {
                if (qualifiesForRanking)
                    ShowInitialsPanel(finalScore);
                else
                    ResetToStart();
            });
        }

        private void RenderLeaderboard()
        {
            if (leaderboardRowsParent != null)
            {
                for (int i = leaderboardRowsParent.childCount - 1; i >= 0; i--)
                {
                    Transform oldRow = leaderboardRowsParent.GetChild(i);
                    oldRow.SetParent(null);
                    Destroy(oldRow.gameObject);
                }
            }

            var scores = HighScoreManager.LoadScores();
            bool hasScores = scores.Count > 0;

            if (leaderboardText != null)
            {
                leaderboardText.gameObject.SetActive(!hasScores);
                if (!hasScores)
                    leaderboardText.text = "Nenhum recorde ainda.\nSeja o primeiro a marcar um!";
            }

            if (!hasScores || leaderboardRowPrefab == null || leaderboardRowsParent == null) return;

            int highlightRank = HighScoreManager.ConsumeLastSavedRank();
            var spawnedRows = new List<(LeaderboardRowController row, int index, bool isRecentlySaved)>();

            for (int i = 0; i < scores.Count; i++)
            {
                LeaderboardRowController row = Instantiate(leaderboardRowPrefab, leaderboardRowsParent);
                bool isRecentlySaved = (i + 1) == highlightRank;
                row.SetData(i + 1, scores[i].initials, scores[i].score, isRecentlySaved);
                spawnedRows.Add((row, i, isRecentlySaved));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardRowsParent as RectTransform);

            foreach (var (row, index, isRecentlySaved) in spawnedRows)
                row.PlayEnterAnimation(index * 0.06f, celebratory: isRecentlySaved);
        }

        private void ShowInitialsPanel(int finalScore)
        {
            _pendingScore = finalScore;

            if (virtualJoystick != null) virtualJoystick.SetVisible(false);

            if (finalScoreLabel != null) finalScoreLabel.text = $"NOVO RECORDE: {finalScore}";

            if (initialsInput != null)
            {
                initialsInput.text = "";
                initialsInput.characterLimit = 4;
                initialsInput.onValueChanged.RemoveAllListeners();
                initialsInput.onValueChanged.AddListener(HandleInitialsInputChanged);
                initialsInput.ActivateInputField();
            }

            RefreshInitialsSlots("");

            if (confirmInitialsButton != null)
                confirmInitialsButton.interactable = false;

            if (initialsPanel != null)
            {
                initialsPanel.SetActive(true);
                PlayPanelOpenAnimation(initialsPanel, celebratory: true);
            }
        }

        private void HandleInitialsInputChanged(string value)
        {
            if (initialsInput == null) return;

            string upper = value.ToUpperInvariant();
            if (upper != value)
            {
                int caret = initialsInput.caretPosition;
                initialsInput.text = upper; 
                initialsInput.caretPosition = caret;
                return;
            }

            RefreshInitialsSlots(upper);

            if (confirmInitialsButton != null)
                confirmInitialsButton.interactable = upper.Length >= initialsInput.characterLimit;
        }

        private void RefreshInitialsSlots(string typed)
        {
            if (initialsSlotLabels == null) return;

            for (int i = 0; i < initialsSlotLabels.Length; i++)
            {
                if (initialsSlotLabels[i] == null) continue;

                bool filled = i < typed.Length;
                bool wasEmpty = string.IsNullOrEmpty(initialsSlotLabels[i].text);
                initialsSlotLabels[i].text = filled ? typed[i].ToString() : "";

                if (filled && wasEmpty)
                    initialsSlotLabels[i].transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, vibrato: 6, elasticity: 0.6f);

                if (initialsSlotBackgrounds != null && i < initialsSlotBackgrounds.Length && initialsSlotBackgrounds[i] != null)
                {
                    bool isActiveSlot = i == typed.Length;
                    initialsSlotBackgrounds[i].color = isActiveSlot ? corSlotAtivo : corSlotInativo;
                }
            }
        }

        public void ConfirmInitials()
        {
            string initials = (initialsInput != null && !string.IsNullOrWhiteSpace(initialsInput.text))
                ? initialsInput.text
                : "???";

            HighScoreManager.SaveScore(initials, _pendingScore);

            if (initialsPanel != null) initialsPanel.SetActive(false);
            ResetToStart();
        }

        private void ResetToStart()
        {
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        private void RollCredits(TMP_Text mainText, TMP_Text thanksLabel, Image blackScreenFade)
        {
            if (mainText == null || creditsMusic == null || finalCreditsClip == null) return;

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