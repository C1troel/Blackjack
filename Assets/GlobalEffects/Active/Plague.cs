using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class Plague : BaseActiveGlobalEffect
        {
            public Plague(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
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

                int effectDuration = ActiveGlobalEffectInfo.TurnDuration;

                foreach (var entity in entities)
                {
                    var plaguePassiveEffect = new PassiveEffects.Plague(effectDuration);
                    entity.PassiveEffectHandler.TryToAddEffect(plaguePassiveEffect);
                }

                OnGlobalEffectStateChange();
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