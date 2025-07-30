using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class Penalty : BaseActiveGlobalEffect
        {
            private const int EFFECT_CARDS_LOSE_AMOUNT = 1;
            public Penalty(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
            {}

            public override bool IsUsable()
            {
                return CooldownRounds <= 0;
            }

            public override void TryToActivate()
            {
                if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                var gameManager = GameManager.Instance;
                var entities = gameManager.GetEntitiesList();

                foreach (var entity in entities)
                {
                    switch (entity.GetEntityType)
                    {
                        case EntityType.Player:
                            var player = entity as BasePlayerController;
                            player.EffectCardsHandler.RemoveRandomEffectCards(EFFECT_CARDS_LOSE_AMOUNT);
                            break;

                        case EntityType.Enemy:
                            var enemy = entity as BaseEnemy;
                            enemy.EnemyEffectCardsHandler.RemoveRandomEffectCards(EFFECT_CARDS_LOSE_AMOUNT);
                            break;

                        case EntityType.Ally:
                            break;

                        default:
                            break;
                    }
                }
            }

            public override void OnNewTurnStart()
            {
                if (CooldownRounds <= 0)
                {
                    Debug.Log($"Ability {this} is already ready for activation");
                    return;
                }

                CooldownRounds--;
                Debug.Log($"{this} ability cooldown reset in {CooldownRounds} turns");

                if (CooldownRounds == 0)
                {
                    Debug.Log($"Ability {this} is now ready!");
                    OnGlobalEffectStateChange();
                }
            }
        }
    }
}
