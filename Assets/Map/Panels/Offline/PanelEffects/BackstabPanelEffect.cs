using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class BackstabPanelEffect : IPanelEffect
    {
        private const int DAMAGE = 20;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if (!CheckForAvailableEntities(entity))
            {
                onComplete?.Invoke();
                yield break;
            }

            IEntity target = null;

            switch (entity.GetEntityType)
            {
                case EntityType.Player:
                    GameManager.Instance.StartChoosingTarget(choosed =>
                    {
                        target = choosed;
                    });
                    break;

                case EntityType.Enemy:
                    target = HandleEnemyTargeting(entity);
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"Backstab target: {target.GetEntityName}");

            if (target == entity)
                Debug.Log("Cannot target anyone for backstab...");
            else
                GameManager.Instance.DealDamage(target, DAMAGE);

            yield return null;
            onComplete?.Invoke();
        }
        private bool CheckForAvailableEntities(IEntity entityInit)
        {
            var allEntities = GameManager.Instance.GetEntitiesList().ToList();
            allEntities.Remove(entityInit);
            allEntities.RemoveAll(entity => entity.GetEntityHp == 0);

            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Ally);
                    break;

                case EntityType.Enemy:
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Enemy);
                    break;

                case EntityType.Ally:
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Ally);
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Player);
                    break;

                default:
                    break;
            }

            return allEntities.Count != 0;
        }

        private IEntity HandleEnemyTargeting(IEntity entityInit)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);

            if (player.GetEntityHp == 0)
                return entityInit;

            return player;
        }
    }
}