using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class CasinoPanelEffect : IPanelEffect
    {
        private const int HEAL_AMOUNT = 20;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            GameManager.Instance.Heal(entity, HEAL_AMOUNT, true);

            if (entity.GetEntityType != EntityType.Player)
            {
                onComplete?.Invoke();
                yield break;
            }

            var blackjackManager = BlackjackManager.Instance;
            Debug.Log("Panel starting blackjack game");
            blackjackManager.StartBlackjack();

            yield return new WaitUntil(() => !blackjackManager.isBlackjackGameRunning);

            onComplete?.Invoke();
        }
    }
}