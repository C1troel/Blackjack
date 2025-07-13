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
                    PanelEffectsManager.Instance.StartChoosingTarget(choosed =>
                    {
                        target = choosed;
                    });
                    break;

                case EntityType.Enemy:
                    target = GameManager.Instance.GetEntityWithType(EntityType.Player);
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"Backstab target: {target.GetEntityName}");
            GameManager.Instance.DealDamage(target, DAMAGE);

            yield return new WaitForSeconds(0.5f);
            onComplete?.Invoke();
        }
    }
}