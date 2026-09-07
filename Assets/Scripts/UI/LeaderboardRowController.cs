using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nucleo.UI
{
    public class LeaderboardRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankLabel;
        [SerializeField] private TMP_Text initialsLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [Tooltip("Fundo da linha — usado pro destaque de 'você acabou de marcar isso'. Deixe com alpha 0 por padrão no prefab.")]
        [SerializeField] private Image rowBackground;
        [Tooltip("Label opcional tipo 'VOCÊ' — só aparece na linha recém-salva. Pode deixar vazio/null se não quiser usar.")]
        [SerializeField] private TMP_Text recentlySavedTag;

        [Header("Cores por posição")]
        [SerializeField] private Color corPadrao = Color.white;
        [Tooltip("1º lugar — tom próximo ao CORE-VIOLETA do STYLE_GUIDE.")]
        [SerializeField] private Color corOuro = new Color(0.68f, 0.42f, 1f);
        [SerializeField] private Color corPrata = new Color(0.75f, 0.75f, 0.78f);
        [SerializeField] private Color corBronze = new Color(0.8f, 0.5f, 0.3f);

        [Header("Destaque de recém-salvo")]
        [SerializeField] private Color corFundoRecemSalvo = new Color(0.68f, 0.42f, 1f, 0.25f);
        [SerializeField] private Color corFundoPadrao = new Color(1f, 1f, 1f, 0f);

        public void SetData(int rank, string initials, int score, bool isRecentlySaved = false)
        {
            if (rankLabel != null) rankLabel.text = $"#{rank}";
            if (initialsLabel != null) initialsLabel.text = initials;
            if (scoreLabel != null) scoreLabel.text = score.ToString();

            Color cor = rank switch
            {
                1 => corOuro,
                2 => corPrata,
                3 => corBronze,
                _ => corPadrao
            };
            if (rankLabel != null) rankLabel.color = cor;
            if (initialsLabel != null) initialsLabel.color = cor;
            if (scoreLabel != null) scoreLabel.color = cor;

            if (recentlySavedTag != null) recentlySavedTag.gameObject.SetActive(isRecentlySaved);
            if (rowBackground != null) rowBackground.color = isRecentlySaved ? corFundoRecemSalvo : corFundoPadrao;
        }

        public void PlayEnterAnimation(float delay, bool celebratory)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

            RectTransform rt = (RectTransform)transform;
            Vector2 originalPos = rt.anchoredPosition;

            cg.alpha = 0f;
            rt.anchoredPosition = originalPos + new Vector2(-20f, 0f);

            cg.DOFade(1f, 0.3f).SetDelay(delay).SetUpdate(true);
            rt.DOAnchorPos(originalPos, 0.3f).SetDelay(delay).SetEase(Ease.OutQuad).SetUpdate(true);

            if (celebratory)
            {
                DOVirtual.DelayedCall(delay + 0.3f, () =>
                    transform.DOPunchScale(Vector3.one * 0.08f, 0.5f, vibrato: 2, elasticity: 0.5f)
                             .SetLoops(3)
                             .SetUpdate(true)
                ).SetUpdate(true);
            }
        }
    }
}