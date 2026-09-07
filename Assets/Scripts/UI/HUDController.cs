using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Nucleo.Player;

namespace Nucleo.UI
{
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

        private Color originalPlayerColor;
        private Color originalCoreColor;
        private float lastPlayerHP = -1f;
        private float lastCoreHP = -1f;

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
            if (coreIntegrity != null) coreIntegrity.GetComponent<Health>().OnHealthChanged += HandleCoreHealthChanged;            
            if (playerProgression != null)
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
            if (playerHealthFill != null) originalPlayerColor = playerHealthFill.color;
            if (coreHealthFill != null) originalCoreColor = coreHealthFill.color;

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
            bool isDamage = lastPlayerHP > 0 && current < lastPlayerHP;
            lastPlayerHP = current;

            if (playerHealthFill != null) 
            {
                float targetFill = max > 0f ? current / max : 0f;
                
                playerHealthFill.DOKill();
                playerHealthFill.color = originalPlayerColor; 
                
                if (isDamage) 
                {
                    Sequence dmgSeq = DOTween.Sequence();
                    dmgSeq.Append(playerHealthFill.DOColor(Color.white, 0.1f)); 
                    dmgSeq.Append(playerHealthFill.DOColor(originalPlayerColor, 0.2f)); 
                }
                
                playerHealthFill.DOFillAmount(targetFill, 0.3f).SetEase(Ease.OutCubic);
            }
            
            if (playerHealthLabel != null) 
            {
                playerHealthLabel.text = $"HP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
                
                playerHealthLabel.DOKill(); 
                playerHealthLabel.transform.DOKill();
                
                if (isDamage)
                {
                    playerHealthLabel.transform.localScale = Vector3.one; 
                    playerHealthLabel.color = Color.white; 
                    
                    Sequence textSeq = DOTween.Sequence();
                    textSeq.Append(playerHealthLabel.DOColor(Color.red, 0.1f));
                    textSeq.Append(playerHealthLabel.DOColor(Color.white, 0.2f));

                    playerHealthLabel.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 15, 1);
                }
                else
                {
                    playerHealthLabel.transform.localScale = Vector3.one;
                    playerHealthLabel.color = Color.white;
                }
            }
        }

        private void HandleCoreHealthChanged(float current, float max)
        {
            bool isDamage = lastCoreHP > 0 && current < lastCoreHP;
            lastCoreHP = current;

            if (coreHealthFill != null) 
            {
                float targetFill = max > 0f ? current / max : 0f;
                
                coreHealthFill.DOKill();
                coreHealthFill.color = originalCoreColor;
                
                if (isDamage)
                {
                    Sequence dmgSeq = DOTween.Sequence();
                    dmgSeq.Append(coreHealthFill.DOColor(Color.white, 0.1f));
                    dmgSeq.Append(coreHealthFill.DOColor(originalCoreColor, 0.2f));
                }
                
                coreHealthFill.DOFillAmount(targetFill, 0.3f).SetEase(Ease.OutCubic);
            }
            
            if (coreHealthLabel != null) 
            {
                coreHealthLabel.text = $"CHP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
                
                coreHealthLabel.DOKill();
                coreHealthLabel.transform.DOKill();
                
                if (isDamage)
                {
                    coreHealthLabel.transform.localScale = Vector3.one;
                    coreHealthLabel.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
                }
                else
                {
                    coreHealthLabel.transform.localScale = Vector3.one;
                }
            }
        }

        private void HandleXPChanged(int current, int toNext)
        {
            if (xpFill != null) 
            {
                float targetFill = toNext > 0 ? (float)current / toNext : 0f;
                
                if (targetFill == 0) xpFill.fillAmount = 0f;
                else xpFill.DOFillAmount(targetFill, 0.4f).SetEase(Ease.OutBack);
            }
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