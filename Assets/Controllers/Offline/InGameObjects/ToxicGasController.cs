using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class ToxicGasController : MonoBehaviour
    {
        private const float DAMAGE_MULT = 0.2f;
        private int remainingRounds = 0;

        public void Initialize(int roundDuration)
        {
            remainingRounds = roundDuration;
            TurnManager.Instance.OnNewRoundStarted += OnNewRoundStarted;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<IEntity>(out var entity)) return;

            ApplyDamage(entity);
        }

        private void ApplyDamage(IEntity entity)
        {
            int damage = (int)(entity.GetEntityMaxHp * DAMAGE_MULT);
            GameManager.Instance.DealDamage(entity, damage, false);
        }

        private void OnNewRoundStarted()
        {
            if (remainingRounds == 0)
                Destroy(gameObject);

            remainingRounds--;
        }
    }
}