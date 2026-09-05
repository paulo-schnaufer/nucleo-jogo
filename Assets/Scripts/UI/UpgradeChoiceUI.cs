// NÚCLEO: Última Onda — UI (ver STATUS.md, bloco P1 "UI mínima")
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nucleo.UI
{
    /// <summary>
    /// Painel de escolha de upgrade (3 cards). Só escuta
    /// UpgradeManager.OnChoicesReady e chama ConfirmChoice no clique — a
    /// pausa e o sorteio já existem em UpgradeManager, não duplicados aqui
    /// (ver DECISIONS.md).
    /// </summary>
    public class UpgradeChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private List<UpgradeCard> cards = new List<UpgradeCard>();

        [System.Serializable]
        public class UpgradeCard
        {
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
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card.button == null) continue;

                bool hasChoice = i < choices.Count;
                card.button.gameObject.SetActive(hasChoice);
                if (!hasChoice) continue;

                UpgradeData data = choices[i];
                if (card.icon != null) card.icon.sprite = data.icon;
                if (card.title != null) card.title.text = data.displayName;
                if (card.description != null) card.description.text = data.description;

                card.button.onClick.RemoveAllListeners();
                card.button.onClick.AddListener(() => SelectAndClose(data));
            }

            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void SelectAndClose(UpgradeData chosen)
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            UpgradeManager.Instance.ConfirmChoice(chosen);
            // Se houver escolha pendente (level-up duplo), UpgradeManager
            // dispara OnChoicesReady de novo sozinho — nada a fazer aqui.
        }
    }
}