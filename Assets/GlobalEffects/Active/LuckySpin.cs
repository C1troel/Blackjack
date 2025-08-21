using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;
using System;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class LuckySpin : BaseActiveGlobalEffect
        {
            private readonly List<Action> effects;

            public LuckySpin(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) 
                : base(activeGlobalEffectInfo, entityOwner)
            {
                effects = new List<Action>
                {
                    () => // Patch
                    {
                        Debug.Log($"{GetType().Name} adds Patch effect");
                        entityOwner.PassiveEffectHandler.TryToAddEffect(new Patch(1));
                    },
                    () => // DoubleDown
                    {
                        Debug.Log($"{GetType().Name} adds DoubleDown effect");
                        entityOwner.PassiveEffectHandler.TryToAddEffect(new DoubleDown(1));
                    },
                    () => // Split
                    {
                        Debug.Log($"{GetType().Name} adds Split effect");
                        entityOwner.PassiveEffectHandler.TryToAddEffect(new Split(1));
                    },
                    () => // Rage
                    {
                        Debug.Log($"{GetType().Name} adds Rage effect");
                        entityOwner.PassiveEffectHandler.TryToAddEffect(new PassiveEffects.Rage(1));
                    },
                    () => // Event panel
                    {
                        Debug.Log($"{GetType().Name} triggers Event panel");
                        new EventPanelEffect().Execute(entityOwner, null);
                    }
                };
            }

            public override void TryToActivate()
            {
                if (entityOwner == null)
                {
                    Debug.Log($"Can`t trigger {this} cuz entity owner is null");
                    return;
                }
                else if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                GiveRandomEffect();
                OnGlobalEffectStateChange();
            }

            private void GiveRandomEffect()
            {
                int effectId = Random.Range(0, effects.Count);
                effects[effectId].Invoke();
            }
        }
    }
}