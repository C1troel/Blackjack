using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class ArmUp : BaseActiveGlobalEffect
        {
            public ArmUp(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
            {}

            public override void TryToActivate()
            {
                if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                var entities = GameManager.Instance.GetEntitiesList();

                foreach (var entity in entities)
                    EffectCardDealer.Instance.DealEffectCardOfType(entity, EffectCardType.BigAttackPack);
            }
        }
    }
}