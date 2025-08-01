using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class PlayerEffectCardsHandler : MonoBehaviour
    {
        [SerializeField] private int maxEffectCardsAmount;
        private List<BaseEffectCard> playerEffectCardsList = new List<BaseEffectCard>();
        private BasePlayerController player;

        private void Start()
        {
            StartCoroutine(AvaitAndCachePlayer());
        }

        private IEnumerator AvaitAndCachePlayer()
        {
            var gameManager = GameManager.Instance;
            yield return new WaitUntil(() => gameManager.GetEntitiesList().Count != 0);

            player = gameManager.GetEntityWithType(EntityType.Player) as BasePlayerController;
        }

        public void OnNewTurnStart()
        {
            CheckAllCardsUsability();
        }

        public void EnableAndOutlineCounterCards(List<IEffectCardLogic> counterCardsLogic)
        {
            var counterCards = playerEffectCardsList
                .Where(card => counterCardsLogic.Contains(card.EffectCardLogic))
                .ToList();

            foreach (var card in counterCards)
            {
                card.OutlineCard();
                card.EffectCardLogic.ToggleMarkAsCounterCard(true);
            }
        }

        public void DisableAndRemoveOutlineOfCounterCards()
        {
            foreach (var card in playerEffectCardsList)
            {
                card.EffectCardLogic.ToggleMarkAsCounterCard(false);
                card.RemoveCardOutline();
            }
        }

        public List<IEffectCardLogic> GetCounterCards(EffectCardDmgType effectCardDmgType)
        {
            List<IEffectCardLogic> counterCards = new List<IEffectCardLogic>();

            foreach (var card in playerEffectCardsList)
            {
                var vulnerabilities = card.EffectCardLogic.EffectCardInfo.Vulnerabilities;

                var strength = CombatInteractionEvaluator.Evaluate(vulnerabilities, effectCardDmgType);

                if (strength == CombatInteractionEvaluator.InteractionStrength.Strong)
                {
                    counterCards.Add(card.EffectCardLogic);
                }
            }

            return counterCards;
        }

        public List<IEffectCardLogic> GetCounterCards(PassiveEffectType passiveEffectType)
        {
            List<IEffectCardLogic> counterCards = new List<IEffectCardLogic>();

            foreach (var card in playerEffectCardsList)
            {
                var vulnerabilities = card.EffectCardLogic.EffectCardInfo.Vulnerabilities;

                var strength = CombatInteractionEvaluator.Evaluate(vulnerabilities, passiveEffectType);

                if (strength == CombatInteractionEvaluator.InteractionStrength.Strong)
                {
                    counterCards.Add(card.EffectCardLogic);
                }
            }

            return counterCards;
        }

        public void OnTurnEnd()
        {
            RemoveCardsOutline();
        }

        private void RemoveCardsOutline()
        {
            foreach (var card in playerEffectCardsList)
                card.RemoveCardOutline();
        }

        private void CheckAllCardsUsability()
        {
            foreach (var card in playerEffectCardsList)
                card.CheckIfCanBeUsed(player);
        }

        public void AddEffectCard(BaseEffectCard addedCard)
        {
            if (playerEffectCardsList.Count >= maxEffectCardsAmount)
            {
                Debug.Log($"Player already have max effect cards in hand");
                return;
            }

            playerEffectCardsList.Add(addedCard);

            addedCard.OnEffectCardUsed += OnEffectCardUsed;
            addedCard.transform.SetParent(transform);
        }

        public void RemoveRandomEffectCards(int amount)
        {
            if (playerEffectCardsList.Count == 0 || amount <= 0)
                return;

            int removeCount = Mathf.Min(amount, playerEffectCardsList.Count);

            var randomCardsToRemove = playerEffectCardsList
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(removeCount)
                .ToList();

            foreach (var card in randomCardsToRemove)
                RemoveEffectCard(card);
        }

        public List<BaseEffectCard> MoveEffectCardsByPurpose(EffectCardPurpose effectCardPurpose)
        {
            var matchedCards = playerEffectCardsList.FindAll(effectCard => 
            effectCard.EffectCardLogic.EffectCardInfo.EffectCardPurposes.Contains(effectCardPurpose));

            foreach (var movedCard in matchedCards)
                RemoveFromList(movedCard);

            return matchedCards;
        }

        public void RemoveFromList(BaseEffectCard card)
        {
            playerEffectCardsList.Remove(card);
            card.OnEffectCardUsed -= OnEffectCardUsed;
        }

        public void RemoveEffectCard(BaseEffectCard card)
        {
            playerEffectCardsList.Remove(card);
            card.OnEffectCardUsed -= OnEffectCardUsed;
            Destroy(card.gameObject);
        }

        private void OnEffectCardUsed(BaseEffectCard usedEffectCard)
        {
            RemoveEffectCard(usedEffectCard);
        }
    }
}