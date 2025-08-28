using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class BackstabKnife : BaseEffectCardLogic
    {
        private const int DAMAGE = 50;

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
                    }, TargetObjectsList);
                    break;

                case EntityType.Enemy:
                    target = HandleEnemyTargeting();
                    break;
            }

            yield return new WaitUntil(() => target != null);

            Debug.Log($"{GetType().Name} target: {target.GetEntityName}");
            BackstabEntity(target);

            onComplete?.Invoke();
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
            var targetEntities = TargetObjectsList.Cast<IEntity>().ToList();

            var aliveTargets = targetEntities
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

        private void BackstabEntity(IEntity target)
        {
            GameManager.Instance.DealDamage(target, DAMAGE, false, EffectCardInfo.EffectCardDmgType);
        }
    }
}