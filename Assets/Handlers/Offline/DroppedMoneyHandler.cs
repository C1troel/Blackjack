using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class DroppedMoneyHandler : MonoBehaviour, IMapObject, IOutlinable
    {
        private PanelScript currentPanel;
        private int moneyAmount;

        private SpriteRenderer spriteRenderer;
        private Material defaultSpriteMaterial;
        private Material outlineSpriteMaterial;

        public event Action<bool> OnOutlineChanged;

        public void ManageDroppedMoney(int moneyAmount)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultSpriteMaterial = spriteRenderer.material;
            outlineSpriteMaterial = EffectCardDealer.Instance.GetEffectCardOutlineMaterial; // заглушка, щоб була хоч якась обводка
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

        public void SetOutline()
        {
            spriteRenderer.material = outlineSpriteMaterial;
        }

        public void RemoveOutline()
        {
            spriteRenderer.material = defaultSpriteMaterial;
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PanelScript>(out var panel)) return;

            currentPanel = panel;
        }
    }
}