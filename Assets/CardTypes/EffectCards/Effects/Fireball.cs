using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class Fireball : BaseEffectCardLogic
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
                    target = HandleEnemyTargeting();
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"Fireball target: {target.GetEntityName}");
            ThrowFireballToEntity(target, entityInit, onComplete);

            /*onComplete?.Invoke();*/
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            if (HandleEnemyTargeting() == null)
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

        private IEntity HandleEnemyTargeting()
        {
            var aliveTargets = TargetEnemiesList
                .Where(entity => entity.GetEntityHp > 0
                && entity.GetCurrentPanel.GetEffectPanelInfo.Effect != PanelEffect.VIPClub)
                .ToList();

            if (aliveTargets.Count == 0)
                return null;

            var player = aliveTargets.FirstOrDefault(e => e.GetEntityType == EntityType.Player);
            if (player != null)
                return player;

            return aliveTargets[Random.Range(0, aliveTargets.Count)];
        }

        private void ThrowFireballToEntity(IEntity target, IEntity entityInit, Action onComplete)
        {
            var projectilEffectCardInfo = EffectCardInfo as ProjectileEffectCardInfo;

            var fireballPrefab = projectilEffectCardInfo.ProjectilePrefab;

            var targetsPanel = target.GetCurrentPanel;
            Vector3 spawnPos = new Vector3(targetsPanel.transform.position.x + projectilEffectCardInfo.SpawnCordsOffset, targetsPanel.transform.position.y);

            GameObject fireballGO = GameManager.Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

            FireballProjectile fireballProjectile = fireballGO.GetComponent<FireballProjectile>();
            fireballProjectile.Initialize(onComplete, target, entityInit, targetsPanel, EffectCardInfo);
        }
    }
}