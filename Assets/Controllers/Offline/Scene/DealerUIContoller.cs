using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class DealerUIContoller : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup cardSelectionContainer;
        [SerializeField] private GameObject preselectCardPrefab;
        [SerializeField] private GameObject background;

        private int preselectCardsAmount = 3;
        public bool isPlayerPreselect { get; private set; } = false;
        public static DealerUIContoller Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
        }

        public void StartDealerInteraction()
        {
            /*GameManager.Instance.TogglePlayersHUD(false);*/
            isPlayerPreselect = false;
            background.SetActive(true);

            SetupPreselectCards();
        }

        private void SetupPreselectCards()
        {
            List<Sprite> basicCardsList = new List<Sprite>();
            basicCardsList.AddRange(GameManager.Instance.BasicCardsList);

            System.Random random = new System.Random();
            basicCardsList = basicCardsList.OrderBy(x => random.Next()).ToList();

            List<Sprite> chosenPreselectSprites = basicCardsList.Take(preselectCardsAmount).ToList();

            foreach (var sprite in chosenPreselectSprites)
            {
                var preselectCardGO = Instantiate(preselectCardPrefab, cardSelectionContainer.transform);
                var preselectCard = preselectCardGO.GetComponent<PreselectCard>();
                preselectCard.SetupPreselectCard(sprite);
            }

            foreach (Transform card in cardSelectionContainer.transform)
            {
                var preselectCard = card.GetComponent<PreselectCard>();
                preselectCard.FlipCard();
                preselectCard.GetComponentInChildren<Button>().onClick.AddListener(() => OnPreselectCardClick(preselectCard));
            }
        }

        private void EndDealerInteraction()
        {
            background.SetActive(false);
            isPlayerPreselect = true;
            /*GameManager.Instance.TogglePlayersHUD(true);*/

            foreach (Transform card in cardSelectionContainer.transform)
                Destroy(card.gameObject);

        }

        private void OnPreselectCardClick(PreselectCard clickedCard)
        {
            Debug.Log($"Player preselect card: {clickedCard.GetPreselectCardSprite().name}");
            BlackjackManager.Instance.PreselectCardForNextGame(clickedCard.GetPreselectCardSprite());
            EndDealerInteraction();
        }

        public void OnPreselectSkipBtnClick()
        {
            Debug.Log("Player skip card preselection!");
            EndDealerInteraction();
        }

    }
}