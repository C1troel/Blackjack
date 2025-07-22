using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class PayoffPanelEffect : IPanelEffect
    {
        private List<int> rewards = new List<int>()
        {
            2, 4, 6, 8, 10
        };

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if (entity.GetEntityType != EntityType.Player)
            {
                onComplete?.Invoke();
                yield break;
            }

            var player = entity as BasePlayerController;
            int randomPredefinedReward = rewards[Random.Range(0, rewards.Count)];

            player.GainMoney(randomPredefinedReward, true);

            var dealerUIController = DealerUIContoller.Instance;
            dealerUIController.StartDealerInteraction();

            yield return new WaitUntil(() => dealerUIController.isPlayerPreselect);

            onComplete?.Invoke();
        }

    }
}