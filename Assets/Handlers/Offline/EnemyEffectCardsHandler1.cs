using Singleplayer.PassiveEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class EnemyEffectCardsHandler
    {
        private int maxEffectCardsAmount;
        public List<IEffectCardLogic> effectCardsList { get; private set; } = new List<IEffectCardLogic>();
        private List<IEffectCardLogic> possibleEffectCardsForThatTurn = new List<IEffectCardLogic>();
        private BaseEnemy entityOwner;

        public EnemyEffectCardsHandler(IEntity managedEntity, int maxEntityEffectCardsAmount)
        {
            this.entityOwner = managedEntity as BaseEnemy;
            maxEffectCardsAmount = maxEntityEffectCardsAmount;
        }

        public bool TryToUseNextPossibleEffectCard()
        {
            if (possibleEffectCardsForThatTurn.Count == 0 || entityOwner.GetEntityLeftCards == 0)
                return false;

            var possibleEffectCard = possibleEffectCardsForThatTurn[0];
            possibleEffectCardsForThatTurn.Remove(possibleEffectCard);

            possibleEffectCard.TryToUseCard(isUsed =>
            {
                OnEnemyTryingToUseCard(isUsed, possibleEffectCard);
            }, entityOwner);

            return true;
        }

        public void AddEffectCard(IEffectCardLogic effectCard)
        {
            if (effectCardsList.Count >= maxEffectCardsAmount)
            {
                Debug.Log($"Enemy {entityOwner.GetEntityName} already have max effect cards on hands");
                return;
            }

            effectCardsList.Add(effectCard);
        }

        public List<IEffectCardLogic> GetCounterCards(EffectCardDmgType effectCardDmgType)
        {
            List<IEffectCardLogic> counterCards = new List<IEffectCardLogic>();

            foreach (var card in effectCardsList)
            {
                var vulnerabilities = card.EffectCardInfo.Vulnerabilities;

                var strength = CombatInteractionEvaluator.Evaluate(vulnerabilities, effectCardDmgType);

                if (strength == CombatInteractionEvaluator.InteractionStrength.Strong)
                {
                    counterCards.Add(card);
                }
            }

            return counterCards;
        }

        public List<IEffectCardLogic> GetCounterCards(PassiveEffectType passiveEffectType)
        {
            List<IEffectCardLogic> counterCards = new List<IEffectCardLogic>();

            foreach (var card in effectCardsList)
            {
                var vulnerabilities = card.EffectCardInfo.Vulnerabilities;

                var strength = CombatInteractionEvaluator.Evaluate(vulnerabilities, passiveEffectType);

                if (strength == CombatInteractionEvaluator.InteractionStrength.Strong)
                {
                    counterCards.Add(card);
                }
            }

            return counterCards;
        }

        public void RemoveRandomEffectCards(int amount)
        {
            if (effectCardsList.Count == 0 || amount <= 0)
                return;

            int removeCount = Mathf.Min(amount, effectCardsList.Count);

            var randomCardsToRemove = effectCardsList
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(removeCount)
                .ToList();

            foreach (var card in randomCardsToRemove)
                RemoveEffectCard(card);
        }

        public void RemoveEffectCard(IEffectCardLogic effectCard) => effectCardsList.Remove(effectCard);

        private void OnEnemyTryingToUseCard(bool isUsed, IEffectCardLogic possibleEffectCard)
        {
            if (isUsed)
            {
                entityOwner.DecreaseEffectCardsUsages();
                effectCardsList.Remove(possibleEffectCard);
            }

            RemoveEffectCard(possibleEffectCard);
            entityOwner.ProcessEnemyTurn();
        }

        public void OnNewTurnStart()
        {
            foreach (var card in effectCardsList)
            {
                if (card.CheckIfCanBeUsed(entityOwner))
                    possibleEffectCardsForThatTurn.Add(card);
            }
        }
    }
}