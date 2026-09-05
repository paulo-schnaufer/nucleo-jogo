// NÚCLEO: Última Onda — UI (ver STATUS.md, bloco P1 "UI mínima")
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nucleo.UI
{
    /// <summary>
    /// HUD sempre visível: vida do jogador, Integridade do Núcleo, XP/nível
    /// e onda atual. Só consome eventos já públicos de Health/CoreIntegrity/
    /// PlayerProgression/EnemySpawner — nenhum script de gameplay foi
    /// alterado pra isso (ver DECISIONS.md sobre arquitetura oficial).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Fontes de dados (arrastar no Inspector)")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private PlayerProgression playerProgression;
        [Tooltip("Opcional — só pra exibir 'ONDA X'. Deixe vazio se não quiser o indicador.")]
        [SerializeField] private EnemySpawner spawner;

        [Header("UI — vida do jogador")]
        [SerializeField] private Image playerHealthFill;
        [SerializeField] private TMP_Text playerHealthLabel;

        [Header("UI — Integridade do Núcleo")]
        [SerializeField] private Image coreHealthFill;
        [SerializeField] private TMP_Text coreHealthLabel;

        [Header("UI — XP / nível")]
        [SerializeField] private Image xpFill;
        [SerializeField] private TMP_Text levelLabel;

        [Header("UI — onda (opcional)")]
        [SerializeField] private TMP_Text waveLabel;

        private void Awake()
        {
            // if (playerHealthFill != null) playerHealthFill.color = UITheme.CianoBase;
            // if (coreHealthFill != null) coreHealthFill.color = UITheme.CoreVioleta;
            // if (xpFill != null) xpFill.color = UITheme.CianoGlow;
        }

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
            if (coreIntegrity != null) coreIntegrity.GetComponent<Health>().OnHealthChanged += HandleCoreHealthChanged;            if (playerProgression != null)
            {
                playerProgression.OnXPChanged += HandleXPChanged;
                playerProgression.OnLevelUp += HandleLevelUp;
            }
            if (spawner != null) spawner.OnWaveStarted += HandleWaveStarted;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged -= HandlePlayerHealthChanged;
            if (coreIntegrity != null) coreIntegrity.GetComponent<Health>().OnHealthChanged -= HandleCoreHealthChanged;
            if (playerProgression != null)
            {
                playerProgression.OnXPChanged -= HandleXPChanged;
                playerProgression.OnLevelUp -= HandleLevelUp;
            }
            if (spawner != null) spawner.OnWaveStarted -= HandleWaveStarted;
        }

        private void Start()
        {
            // Estado inicial — os eventos acima só disparam em MUDANÇA, então
            // sem isso a barra ficaria vazia até o primeiro dano/XP.
            if (playerHealth != null) HandlePlayerHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
            if (coreIntegrity != null) 
            {
                Health coreHP = coreIntegrity.GetComponent<Health>();
                HandleCoreHealthChanged(coreHP.CurrentHP, coreHP.MaxHP);
            }
            if (playerProgression != null)
            {
                HandleXPChanged(playerProgression.CurrentXP, playerProgression.XPToNextLevel);
                HandleLevelUp(playerProgression.Level);
            }
        }

        private void HandlePlayerHealthChanged(float current, float max)
        {
            if (playerHealthFill != null) playerHealthFill.fillAmount = max > 0f ? current / max : 0f;
            if (playerHealthLabel != null) playerHealthLabel.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void HandleCoreHealthChanged(float current, float max)
        {
            if (coreHealthFill != null) coreHealthFill.fillAmount = max > 0f ? current / max : 0f;
            if (coreHealthLabel != null) coreHealthLabel.text = $"NÚCLEO {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void HandleXPChanged(int current, int toNext)
        {
            if (xpFill != null) xpFill.fillAmount = toNext > 0 ? (float)current / toNext : 0f;
        }

        private void HandleLevelUp(int newLevel)
        {
            if (levelLabel != null) levelLabel.text = $"NÍVEL {newLevel}";
        }

        private void HandleWaveStarted(int waveIndex)
        {
            if (waveLabel != null) waveLabel.text = $"ONDA {waveIndex + 1}";
        }
    }
}