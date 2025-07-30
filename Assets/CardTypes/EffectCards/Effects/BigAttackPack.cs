using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BigAttackPack : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            Debug.Log("Big attack pack activated...");
            yield return null;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            Debug.Log($"Entity {entityInit.GetEntityName} used big attack pack activated...");
        }
    }
}