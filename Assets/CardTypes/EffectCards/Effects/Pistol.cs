using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class Pistol : BaseEffectCardLogic
    {
        private const int DAMAGE = 20;
        private float spawnCordsOffset = -60f;

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

            Debug.Log($"Pistol bullet target: {target.GetEntityName}");
            ShootEntity(target, entityInit, onComplete);

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

        private void ShootEntity(IEntity target, IEntity entityInit, Action onComplete)
        {
            var projectilEffectCardInfo = EffectCardInfo as ProjectileEffectCardInfo;

            var pistolBulletPrefab = projectilEffectCardInfo.ProjectilePrefab;

            var targetsPanel = target.GetCurrentPanel;
            Vector3 spawnPos = new Vector3(targetsPanel.transform.position.x + spawnCordsOffset, targetsPanel.transform.position.y);

            GameObject pistolBulletGO = GameManager.Instantiate(pistolBulletPrefab, spawnPos, Quaternion.identity);

            PistolBulletProjectile pistolBulletProjectile = pistolBulletGO.GetComponent<PistolBulletProjectile>();
            pistolBulletProjectile.Initialize(onComplete, target, entityInit, targetsPanel, DAMAGE, EffectCardInfo);
        }
    }
}