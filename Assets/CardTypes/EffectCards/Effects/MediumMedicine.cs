using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class MediumMedicine : BaseEffectCardLogic
    {
        private const int HEAL_AMOUNT = 40;

        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            Heal(entityInit);
            onComplete?.Invoke();
            yield break;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            if (entityInit.GetEntityHp > (entityInit.GetEntityMaxHp - HEAL_AMOUNT))
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

        private void Heal(IEntity entityInit)
        {
            Debug.Log($"Health before small heal: {entityInit.GetEntityHp} of entity with name: {entityInit.GetEntityName}");
            GameManager.Instance.Heal(entityInit, HEAL_AMOUNT, true);
            Debug.Log($"Health after small heal: {entityInit.GetEntityHp} of entity with name: {entityInit.GetEntityName}");
        }
    }
}