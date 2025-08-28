using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class GoldenMagnet : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            var gameManager = GameManager.Instance;
            entityInit ??= gameManager.GetEntityWithType(EntityType.Player);

            /*var availableDroppedMoney = MapManager.FindObjectsOfTypeAroundPanel<DroppedMoneyHandler>
                (entityInit.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels);*/

            var availableDroppedMoney = TargetObjectsList.Cast<DroppedMoneyHandler>().ToList();

            foreach (var droppedMoney in availableDroppedMoney)
            {
                droppedMoney.OnEntityStay(null, entityInit);
                yield return null;
            }

            onComplete?.Invoke();
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            if (TargetObjectsList.Count == 0)
            {
                onComplete?.Invoke(false);
                return;
            }

            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }

        public override bool CheckIfCanBeUsed(IEntity entityOwner)
        {
            var availableDroppedMoney = MapManager.FindObjectsOfTypeAroundPanel<DroppedMoneyHandler>
                (entityOwner.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels);

            CanUse = availableDroppedMoney.Count > 0;
            TargetObjectsList = availableDroppedMoney.Cast<IOutlinable>().ToList();
            return availableDroppedMoney.Count > 0;
        }
    }
}