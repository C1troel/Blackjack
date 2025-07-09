using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer {
    public class SmallMedicine : BaseEffectCardLogic
    {
        private const int HEAL_AMOUNT = 20;

        public override void ApplyEffect(IEntity entityInit = null)
        {
            if (entityInit == null)
            {
                var player = GameManager.Instance.GetEntityWithType(EntityType.Player);

                Debug.Log($"Health before small heal: {player.GetEntityHp}");
                SmallHeal(player);
                Debug.Log($"Health after small heal: {player.GetEntityHp}");
            }
            else
            {
                Debug.Log($"Health before small heal: {entityInit.GetEntityHp} of entity with name: {entityInit.GetEntityName}");
                SmallHeal(entityInit);
                Debug.Log($"Health after small heal: {entityInit.GetEntityHp} of entity with name: {entityInit.GetEntityName}");
            }
        }

        private void SmallHeal(IEntity entityInit)
        {
            GameManager.Instance.Heal(entityInit, HEAL_AMOUNT, true);
        }
    }
}