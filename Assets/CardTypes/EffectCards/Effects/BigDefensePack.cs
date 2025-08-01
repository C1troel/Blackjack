using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BigDefensePack : BaseEffectCardLogic
    {
        private const int ADDING_CARDS_AMOUNT = 3;

        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            Debug.Log("BigDefensePack being used");
            BattleManager.Instance.AddAdditionalCards(ADDING_CARDS_AMOUNT, false);
            yield return null;
            onComplete?.Invoke();
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            Debug.Log($"Entity {entityInit.GetEntityName} used big defense pack activated...");
        }
    }
}