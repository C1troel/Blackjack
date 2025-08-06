using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Singleplayer
{
    public class DroppedMoneyHandler : MonoBehaviour, IMapObject
    {
        private PanelScript currentPanel;
        private int moneyAmount;
        public void ManageDroppedMoney(int moneyAmount)
        {
            this.moneyAmount = moneyAmount;
        }

        public void OnEntityStay(Action onCompleteCallback, IEntity stayedEntity)
        {
            if (stayedEntity.GetEntityHp == 0)
                return;

            switch (stayedEntity.GetEntityType)
            {
                case EntityType.Player:
                    var player = stayedEntity as BasePlayerController;
                    player.GainMoney(moneyAmount, false);
                    break;

                case EntityType.Enemy:
                    var enemy = stayedEntity as BaseEnemy;
                    enemy.PickUpMoney(moneyAmount);
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            onCompleteCallback?.Invoke();

            currentPanel.TryToRemoveMapObject(gameObject);
            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PanelScript>(out var panel)) return;

            currentPanel = panel;
        }
    }
}