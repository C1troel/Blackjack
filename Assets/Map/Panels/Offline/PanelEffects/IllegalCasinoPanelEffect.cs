using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class IllegalCasinoPanelEffect : IPanelEffect
    {
        public IEnumerator Execute(IEntity entityInit, Action onComplete)
        {
            if (entityInit.GetEntityType != EntityType.Player)
            {
                onComplete?.Invoke();
                yield break;
            }

            var blackjackManager = BlackjackManager.Instance;
            Debug.Log("Panel starting blackjack game");
            blackjackManager.StartBlackjack();

            yield return new WaitUntil(() => !blackjackManager.IsBlackjackGameRunning);

            Debug.Log("Illegal casino panel effect ends");
            onComplete?.Invoke();
        }
    }
}