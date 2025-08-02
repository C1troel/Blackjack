using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class PoisonFlask : BaseEffectCardLogic
    {
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
                    target = gameManager.GetEntityWithType(EntityType.Player);
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"Fireball target: {target.GetEntityName}");
            ThrowPoisonFlaskToEntity(target, onComplete);

            /*onComplete?.Invoke();*/
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

        private void ThrowPoisonFlaskToEntity(IEntity target, Action onComplete)
        {
            var projectilEffectCardInfo = EffectCardInfo as ProjectilelEffectCardInfo;

            var poisonFlaskPrefab = projectilEffectCardInfo.ProjectilePrefab;

            var targetsPanel = target.GetCurrentPanel;
            Vector3 spawnPos = new Vector3(targetsPanel.transform.position.x + spawnCordsOffset, targetsPanel.transform.position.y);

            GameObject posionFlaskGO = GameManager.Instantiate(poisonFlaskPrefab, spawnPos, Quaternion.identity);
            
            PoisonFlaskProjectile fireballProjectile = posionFlaskGO.GetComponent<PoisonFlaskProjectile>();
            fireballProjectile.Initialize(onComplete, target, targetsPanel, EffectCardInfo);
        }
    }
}