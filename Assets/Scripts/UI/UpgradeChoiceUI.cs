using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Nucleo.Upgrades;
using Nucleo.Controls;

namespace Nucleo.UI
{
    public class UpgradeChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private List<UpgradeCard> cards = new List<UpgradeCard>();

        [Header("Joystick (mobile)")]
        [Tooltip("Arraste aqui o GameObject 'Joystick_Background' (o que tem o script VirtualJoystick). Some enquanto a tela de upgrade está aberta.")]
        [SerializeField] private VirtualJoystick virtualJoystick;

        [System.Serializable]
        public class UpgradeCard
        {
            [Tooltip("Arraste o objeto PAI do card aqui (o fundo que contém tudo)")]
            public RectTransform cardRoot;
            
            public Button button;
            public Image icon;
            public TMP_Text title;
            public TMP_Text description;
        }

        private void Start()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnChoicesReady += HandleChoicesReady;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnChoicesReady -= HandleChoicesReady;
        }

        private void HandleChoicesReady(List<UpgradeData> choices)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (virtualJoystick != null) virtualJoystick.SetVisible(false);

            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card.button == null) continue;

                bool hasChoice = i < choices.Count;
                
                if (card.cardRoot != null) card.cardRoot.gameObject.SetActive(hasChoice);
                else card.button.gameObject.SetActive(hasChoice);

                if (!hasChoice) continue;

                UpgradeData data = choices[i];
                if (card.icon != null) card.icon.sprite = data.icon;
                if (card.title != null) card.title.text = data.displayName;
                if (card.description != null) card.description.text = data.description;

                card.button.onClick.RemoveAllListeners();
                card.button.onClick.AddListener(() => SelectAndClose(data));

                if (card.cardRoot != null)
                {
                    card.cardRoot.DOKill();
                    card.cardRoot.localScale = Vector3.zero;
                    
                    card.cardRoot.DOAnchorPosY(800f, 0.6f)
                        .From(true) 
                        .SetEase(Ease.OutBack)
                        .SetDelay(i * 0.15f)
                        .SetUpdate(true)
                        .OnStart(() => card.cardRoot.localScale = Vector3.one);
                }
            }
        }

        private void SelectAndClose(UpgradeData chosen)
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            bool stillPlaying = GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameManager.GameState.Playing;
            if (virtualJoystick != null && stillPlaying)
                virtualJoystick.SetVisible(true);

            UpgradeManager.Instance.ConfirmChoice(chosen);
        }
    }
}