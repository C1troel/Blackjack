using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class KindlyMirror : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            Debug.Log($"{GetType().Name} is being triggered");
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);
            var newRandomTarget = GetRandomEntity(entityInit);

            var currentProjectile = ProjectileManager.Instance.currentActiveProjectile;
            var entityOnComplete = currentProjectile.ReflectProjectile();
            var newTargetPanel = newRandomTarget?.GetCurrentPanel;
            var projectileEffectCardInfo = currentProjectile.effectCardInfo as ProjectileEffectCardInfo;

            onComplete?.Invoke();

            yield return new WaitUntil(() => ProjectileManager.Instance.currentActiveProjectile == null);

            if (newRandomTarget == null)
                yield break;

            Vector3 spawnPos = new(newTargetPanel.transform.position.x + projectileEffectCardInfo.SpawnCordsOffset, newTargetPanel.transform.position.y);
            var reflectedProjectileGO = ProjectileManager.Instantiate(projectileEffectCardInfo.ProjectilePrefab, spawnPos, Quaternion.identity);
            var reflectedProjectile = reflectedProjectileGO.GetComponent<IProjectile>();
            reflectedProjectile.Initialize(entityOnComplete, newRandomTarget, entityInit, newTargetPanel, projectileEffectCardInfo);
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

        private IEntity GetRandomEntity(IEntity entityInit)
        {
            var allEntities = GameManager.Instance.GetEntitiesList()
                .Where(entity => entity != null && entity != entityInit && entity.GetEntityHp > 0)
                .ToList();

            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                case EntityType.Ally:
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Player || entity.GetEntityType == EntityType.Ally);
                    break;

                case EntityType.Enemy:
                    allEntities.RemoveAll(entity => entity.GetEntityType == EntityType.Enemy);
                    break;
            }

            if (allEntities.Count == 0)
                return null;

            return allEntities[Random.Range(0, allEntities.Count)];
        }
    }
}