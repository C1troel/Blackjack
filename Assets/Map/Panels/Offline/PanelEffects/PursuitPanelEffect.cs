using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class PursuitPanelEffect : IPanelEffect
    {
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

            Debug.Log($"Pursuit target: {target.GetEntityName}");

            if (target == entity)
                Debug.Log("Cannot target anyone for pursuit...");
            else
            {
                entity.EnableAttacking();
                ((MonoBehaviour)entity).gameObject.transform.position = ((MonoBehaviour)target).transform.position;
                yield return null;

                var battleManager = BattleManager.Instance;
                battleManager.TryToStartBattle(entity, target);
                yield return new WaitUntil(() => !battleManager.isBattleActive);
            }

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