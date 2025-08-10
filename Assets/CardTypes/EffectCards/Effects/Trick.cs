using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class Trick : BaseEffectCardLogic
    {
        private const int LEFT_CARDS_RAISE_AMOUNT = 2;

        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            var gameManager = GameManager.Instance;

            entityInit ??= gameManager.GetEntityWithType(EntityType.Player);

            entityInit.RaiseLeftCards(LEFT_CARDS_RAISE_AMOUNT);

            onComplete?.Invoke();
            yield break;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            var enemy = entityInit as BaseEnemy;

            int usableCount = enemy.EnemyEffectCardsHandler.effectCardsList
                .Count(card => card.CanUse && card != this);

            if ((entityInit.GetEntityLeftCards + LEFT_CARDS_RAISE_AMOUNT) < usableCount)
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
    }
}