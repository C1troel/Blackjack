using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class Mirror : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            Debug.Log($"{GetType().Name} is being triggered");
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            var currentProjectile = ProjectileManager.Instance.currentActiveProjectile;
            var entityOnComplete = currentProjectile.ReflectProjectile();
            var newTargetPanel = currentProjectile.entityOwner.GetCurrentPanel;
            var projectileEffectCardInfo = currentProjectile.effectCardInfo as ProjectileEffectCardInfo;

            Vector3 spawnPos = new(newTargetPanel.transform.position.x + projectileEffectCardInfo.SpawnCordsOffset, newTargetPanel.transform.position.y);

            onComplete?.Invoke();

            yield return new WaitUntil(() => ProjectileManager.Instance.currentActiveProjectile == null);

            var reflectedProjectileGO = ProjectileManager.Instantiate(projectileEffectCardInfo.ProjectilePrefab, spawnPos, Quaternion.identity);
            var reflectedProjectile = reflectedProjectileGO.GetComponent<IProjectile>();
            reflectedProjectile.Initialize(entityOnComplete, currentProjectile.entityOwner, entityInit, newTargetPanel, projectileEffectCardInfo);
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
    }
}