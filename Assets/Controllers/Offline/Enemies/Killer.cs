using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class Killer : BaseEnemy
    {
        private const int EFFECT_CARD_COUNTER_RESET_VALUE = 4;
        private int effectCardCounter;
        public override void SetupEnemy(EnemyInfo enemyInfo)
        {
            base.SetupEnemy(enemyInfo);

            RestoreEffectCards();
        }

        public override void OnNewTurnStart()
        {
            effectCardCounter++;

            if (effectCardCounter == EFFECT_CARD_COUNTER_RESET_VALUE)
            {
                effectCardCounter = 0;

                RestoreEffectCards();
            }

            base.OnNewTurnStart();
        }

        private void RestoreEffectCards()
        {
            foreach (var effectCard in EnemyEffectCardsHandler.effectCardsList.ToList())
                EnemyEffectCardsHandler.RemoveEffectCard(effectCard);

            for (int i = 0; i < 2; i++)
                EffectCardDealer.Instance.DealRandomEffectCard(this);
        }
    }
}