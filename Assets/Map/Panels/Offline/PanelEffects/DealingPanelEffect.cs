using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class DealingPanelEffect : IPanelEffect
    {
        private const int CARDS_DEAL_AMOUNT = 2;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            var effectCardsDealer = EffectCardDealer.Instance;

            for (int i = 0; i < CARDS_DEAL_AMOUNT; i++)
            {
                effectCardsDealer.DealRandomEffectCard(entity);
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}