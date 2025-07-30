using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class ShoppingController : MonoBehaviour
    {
        private const int CARD_PRICE_IN_MONEY = 3;
        private const int CARD_PRICE_IN_CHIPS = 3;
        private const int DEFAULT_CARDS_AMOUNT = 3;

        [SerializeField] private GridLayoutGroup sellableCardsContainer;
        [SerializeField] private GameObject sellableCardPrefab;
        [SerializeField] private GameObject background;

        private int sellableCardsAmount = 3;

        public bool isPlayerShopping { get; private set; } = false;

        public void StartShopping()
        {
            isPlayerShopping = true;
            background.SetActive(true);

            SetupSellabletCards();
        }

        private void SetupSellabletCards()
        {
            for (int i = 0; i < sellableCardsAmount; i++)
            {
                var randomEffectCard = EffectCardDealer.Instance.GetRandomEffectCardInfo();

                var sellableCardGO = Instantiate(sellableCardPrefab, sellableCardsContainer.transform);
                var sellableCard = sellableCardGO.GetComponent<SellableCard>();
                sellableCard.SetupSellableCard(randomEffectCard.EffectCardSprite, randomEffectCard.EffectCardType);
            }

            foreach (Transform card in sellableCardsContainer.transform)
            {
                var sellableCard = card.GetComponent<SellableCard>();
                sellableCard.FlipCard();
                sellableCard.GetComponentInChildren<Button>().onClick.AddListener(() => OnSellableCardClick(sellableCard));
            }
        }

        private void OnSellableCardClick(SellableCard clickedCard)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            if (player.GetEntityMoney < CARD_PRICE_IN_MONEY && player.GetEntityChips < CARD_PRICE_IN_CHIPS)
            {
                Debug.Log($"Player doesn`t have enough money: {player.GetEntityMoney} or chips: {player.GetEntityChips} to buy card: {clickedCard.GetSellableEffectCard()}");
                return;
            }

            var purchasedCard = clickedCard.GetSellableEffectCard();

            if (player.GetEntityChips >= CARD_PRICE_IN_CHIPS)
                player.Pay(CARD_PRICE_IN_CHIPS, true);
            else
                player.Pay(CARD_PRICE_IN_MONEY, false);

            EffectCardDealer.Instance.DealEffectCardOfType(player, purchasedCard);

            Destroy(clickedCard.gameObject);
        }

        private void EndShopping()
        {
            background.SetActive(false);
            isPlayerShopping = false;

            foreach (Transform card in sellableCardsContainer.transform)
                Destroy(card.gameObject);
        }

        public void OnShopExitBtnClick()
        {
            EndShopping();
        }

        public void RaiseSellableCardsAmount(int amount) => sellableCardsAmount = DEFAULT_CARDS_AMOUNT + amount;
    }
}