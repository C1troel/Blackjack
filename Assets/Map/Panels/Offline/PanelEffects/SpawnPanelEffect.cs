using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class SpawnPanelEffect : IPanelEffect
    {
        private const int HEAL_AMOUNT = 20;

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            var gameManager = GameManager.Instance;

            gameManager.Heal(entity, HEAL_AMOUNT, true);

            if (entity.GetEntityType != EntityType.Player)
            {
                onComplete?.Invoke();
                yield break;
            }

            yield return null;

            var player = entity as BasePlayerController;
            gameManager.GetSavingMoneyController.StartSavingInteraction(savedMoney => OnMoneySave(savedMoney, player, onComplete));
        }

        private void OnMoneySave(int savedMoney, BasePlayerController player, Action onComplete)
        {
            if (savedMoney == 0)
            {
                Debug.Log("Player doesn`t have money or cancel saving");
                onComplete?.Invoke();
                return;
            }

            player.Pay(savedMoney, false);
            GameManager.Instance.SaveStealedMoney(savedMoney);
            onComplete?.Invoke();
        }
    }
}