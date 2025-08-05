using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class Regroup : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            IgnoreDirection(entityInit);
            onComplete?.Invoke();
            yield break;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }

        private void IgnoreDirection(IEntity entityInit)
        {
            entityInit.IgnoreDirectionOnce = true;
        }
    }
}