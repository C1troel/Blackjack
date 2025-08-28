using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class Rage : BaseEffectCardLogic
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
                    }, TargetObjectsList);
                    break;

                case EntityType.Enemy:
                    target = HandleEnemyTargeting(entityInit);
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"Rage target: {target.GetEntityName}");
            RageEntity(target);
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
        
        private IEntity HandleEnemyTargeting(IEntity entityInit)
        {
            var targetEntities = TargetObjectsList.Cast<IEntity>().ToList();

            var damagedTargets = targetEntities
                .Where(entity => entity.GetEntityHp > 0 
                && entity.GetEntityHp < 20 
                && entity.GetCurrentPanel.GetEffectPanelInfo.Effect != PanelEffect.VIPClub)
                .ToList();

            if (damagedTargets.Count == 0)
                return entityInit;

            var player = damagedTargets.FirstOrDefault(e => e.GetEntityType == EntityType.Player);
            if (player != null)
                return player;

            return entityInit;
        }

        private void RageEntity(IEntity target)
        {
            var ragePassiveEffect = new PassiveEffects.Rage(EffectCardInfo.EffectsDuration);

            target.PassiveEffectHandler.TryToAddEffect(ragePassiveEffect);
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
            TargetObjectsList = entitiesInEffectiveCardRadius.Cast<IOutlinable>().ToList();
            return true;
        }
    }
}