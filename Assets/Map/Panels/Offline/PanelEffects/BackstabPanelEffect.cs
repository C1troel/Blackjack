using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BackstabPanelEffect : IPanelEffect
    {
        private const int DAMAGE = 20;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
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

        private IEntity HandleEnemyTargeting(IEntity entityInit)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);

            if (player.GetEntityHp == 0)
                return entityInit;

            return player;
        }
    }
}