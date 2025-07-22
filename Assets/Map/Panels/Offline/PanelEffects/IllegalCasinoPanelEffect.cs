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
            var blackjackManager = BlackjackManager.Instance;
            Debug.Log("Panel starting blackjack game");
            blackjackManager.StartBlackjack();

            yield return new WaitUntil(() => !blackjackManager.isBlackjackGameRunning);

            Debug.Log("Illegal casino panel effect ends");
            onComplete?.Invoke();
        }
    }
}