using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    public class BigSwing : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            IEntity target = null;
            var gameManager = GameManager.Instance;

            entityInit ??= gameManager.GetEntityWithType(EntityType.Player);

            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                    gameManager.StartChoosingTarget(choosed =>
                    {
                        target = choosed;
                    }, TargetEnemiesList);
                    break;

                case EntityType.Enemy:
                    target = entityInit;
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"{GetType().Name} target: {target.GetEntityName}");
            GiveDoubleDownEffect(target);
            onComplete?.Invoke();
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

        private void GiveDoubleDownEffect(IEntity target)
        {
            var doublePassiveEffect = new DoubleDown(EffectCardInfo.EffectsDuration);

            target.PassiveEffectHandler.TryToAddEffect(doublePassiveEffect);
        }

        public override bool CheckIfCanBeUsed(IEntity entityOwner)
        {
            if (EffectCardInfo.EffectiveDistanceInPanels == 0 &&
                EffectCardInfo.EffectCardPurposes.Any(purpose => purpose == EffectCardPurpose.Action))
            {
                CanUse = true;
                return true;
            }
            else if (EffectCardInfo.EffectiveDistanceInPanels == 0)
            {
                CanUse = false;
                return false;
            }

            var entitiesInEffectiveCardRadius = MapManager.FindEntitiesAtDistance(entityOwner.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels);

            if (entitiesInEffectiveCardRadius.Count == 0)
            {
                CanUse = false;
                return false;
            }

            CanUse = true;
            TargetEnemiesList = entitiesInEffectiveCardRadius;
            return true;
        }
    }
}