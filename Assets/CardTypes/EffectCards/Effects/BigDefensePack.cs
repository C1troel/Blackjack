using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BigDefensePack : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            Debug.Log("Big defense pack activated...");
            yield return null;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            Debug.Log($"Entity {entityInit.GetEntityName} used big defense pack activated...");
        }
    }
}