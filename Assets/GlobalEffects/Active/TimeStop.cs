using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class TimeStop : BaseActiveGlobalEffect
        {
            private const string TIME_STOP_BYPASS_LAYER = "ColorObject";

            public TimeStop(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) 
                : base(activeGlobalEffectInfo, entityOwner)
            {}

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

                int passiveEffectsDuration = this.ActiveGlobalEffectInfo.TurnDuration;
                var timeStopPassiveEffect = new PassiveEffects.TimeStop(passiveEffectsDuration, PassiveEffectType.TimeStop);
                var chronoMasterPassiveEffect = new PassiveEffects.Chronomaster(passiveEffectsDuration, PassiveEffectType.Chronomaster);

                entityOwner.PassiveEffectHandler.AddEffect(timeStopPassiveEffect);
                entityOwner.PassiveEffectHandler.AddEffect(chronoMasterPassiveEffect);

                var entityMono = entityOwner as MonoBehaviour;
                entityMono.gameObject.layer = LayerMask.NameToLayer(TIME_STOP_BYPASS_LAYER);

                if (entityOwner.Animator.speed == 0)
                    entityMono.StartCoroutine(entityOwner.ResumeAnimationSmoothly(2));

                GlobalEffectsManager.Instance.StopTime(entityOwner);
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