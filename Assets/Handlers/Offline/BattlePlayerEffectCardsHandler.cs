using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class BattlePlayerEffectCardsHandler : MonoBehaviour
    {
        private BasePlayerController player;

        public void ShowPlayerBattleEffectCards(BasePlayerController player, bool forAtk)
        {
            gameObject.SetActive(true);
            this.player = player;

            List<BaseEffectCard> availableEffectCards = new List<BaseEffectCard>();

            if (forAtk)
                availableEffectCards = player.EffectCardsHandler.MoveEffectCardsByPurpose(EffectCardPurpose.BattleAttack);
            else
                availableEffectCards = player.EffectCardsHandler.MoveEffectCardsByPurpose(EffectCardPurpose.BattleDefense);

            foreach (var movedCard in availableEffectCards)
            {
                AddEffectCard(movedCard);
                movedCard.OutlineCard();
            }
        }

        public void HideAndReturnPlayerBattleEffectCards()
        {
            gameObject.SetActive(false);

            if (gameObject.transform.childCount == 0)
                return;

            foreach (var leftCard in GetComponentsInChildren<BaseEffectCard>())
            {
                leftCard.RemoveCardOutline();
                player.EffectCardsHandler.AddEffectCard(leftCard);
                OnCardMove(leftCard);
            }
        }

        private void AddEffectCard(BaseEffectCard addedCard)
        {
            addedCard.OnEffectCardUsed += OnEffectCardUsed;
            addedCard.transform.SetParent(transform);
        }

        private void OnCardMove(BaseEffectCard card)
        {
            card.OnEffectCardUsed -= OnEffectCardUsed;
        }

        private void RemoveEffectCard(BaseEffectCard card)
        {
            card.OnEffectCardUsed -= OnEffectCardUsed;
            Destroy(card.gameObject);
        }

        private void OnEffectCardUsed(BaseEffectCard usedEffectCard)
        {
            RemoveEffectCard(usedEffectCard);
        }
    }
}