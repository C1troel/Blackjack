using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BadBeatPanelEffect : IPanelEffect
    {
        private const int DAMAGE = 20;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if ((entity.GetEntityType == EntityType.Enemy && (entity as BaseEnemy).enemyInfo.IsBoss))
            {
                onComplete?.Invoke();
                yield break;
            }

            GameManager.Instance.DealDamage(entity, DAMAGE,  false);

            yield return null;
            onComplete?.Invoke();
        }
    }
}