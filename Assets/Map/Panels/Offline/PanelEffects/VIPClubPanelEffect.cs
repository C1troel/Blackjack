using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class VIPClubPanelEffect : IPanelEffect
    {
        private const int HEAL_AMOUNT = 30;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            GameManager.Instance.Heal(entity, HEAL_AMOUNT, true);
            yield return null;
            onComplete?.Invoke();
        }
    }
}