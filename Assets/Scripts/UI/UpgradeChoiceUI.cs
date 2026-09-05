using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Obrigatório para as animações

namespace Nucleo.UI
{
    public class UpgradeChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private List<UpgradeCard> cards = new List<UpgradeCard>();

        [System.Serializable]
        public class UpgradeCard
        {
            [Tooltip("Arraste o objeto PAI do card aqui (o fundo que contém tudo)")]
            public RectTransform cardRoot; // <- NOVA VARIÁVEL AQUI
            
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
            // 1. ATIVA O PAINEL PRIMEIRO para a Unity organizar a tela
            if (panelRoot != null) panelRoot.SetActive(true);

            // 2. FORÇA O LAYOUT A ATUALIZAR (Garante que a Unity saiba a posição final exata das cartas)
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
                    // 3. MATA qualquer animação presa de level-ups anteriores
                    card.cardRoot.DOKill();
                    
                    // ESCONDE a carta imediatamente para não "piscar" na tela durante o delay
                    card.cardRoot.localScale = Vector3.zero;
                    
                    // Inicia a queda suave
                    card.cardRoot.DOAnchorPosY(800f, 0.6f)
                        .From(true) 
                        .SetEase(Ease.OutBack)
                        .SetDelay(i * 0.15f)
                        .SetUpdate(true)
                        .OnStart(() => card.cardRoot.localScale = Vector3.one); // REVELA a carta só na hora de despencar
                }
            }
        }

        private void SelectAndClose(UpgradeData chosen)
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            UpgradeManager.Instance.ConfirmChoice(chosen);
        }
    }
}