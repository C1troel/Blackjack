using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class ToxicGasController : MonoBehaviour, IMapObject
    {
        private const float DAMAGE_MULT = 0.2f;
        private int remainingRounds = 0;
        private PanelScript currentPanel;

        public void Initialize(int roundDuration)
        {
            remainingRounds = roundDuration;
            TurnManager.Instance.OnNewRoundStarted += OnNewRoundStarted;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PanelScript>(out var panel)) return;

            currentPanel = panel;
        }

        private void ApplyDamage(IEntity entity)
        {
            int damage = (int)(entity.GetEntityMaxHp * DAMAGE_MULT);
            GameManager.Instance.DealDamage(entity, damage, false);
        }

        private void OnNewRoundStarted()
        {
            if (remainingRounds == 0)
            {
                currentPanel.TryToRemoveMapObject(gameObject);
                Destroy(gameObject);
            }

            remainingRounds--;
        }

        public void OnEntityStay(Action onCompleteCallback, IEntity stayedEntity)
        {
            ApplyDamage(stayedEntity);
            onCompleteCallback?.Invoke();
        }
    }
}