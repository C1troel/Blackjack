using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Singleplayer
{
    public class DroppedMoneyHandler : MonoBehaviour
    {
        private IEntity moneyOwner;
        private int moneyAmount;
        public void ManageDroppedMoney(IEntity moneyOwner, int moneyAmount)
        {
            this.moneyOwner = moneyOwner;
            this.moneyAmount = moneyAmount;
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<IEntity>(out var entity)) return;

            if (entity.GetEntityHp == 0)
                return;

            switch (entity.GetEntityType)
            {
                case EntityType.Player:
                    var player = entity as BasePlayerController;
                    player.GainMoney(moneyAmount, false);
                    break;

                case EntityType.Enemy:
                    var enemy = entity as BaseEnemy;
                    enemy.PickUpMoney(moneyAmount);
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            Destroy(gameObject);
        }
    }
}