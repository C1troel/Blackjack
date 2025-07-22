using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public abstract class BasePassiveGlobalEffect
        {
            public PassiveGlobalEffectInfo PassiveGlobalEffectInfo { get; private set; }
            public int TurnsRemaining { get; protected set; }

            public BasePassiveGlobalEffect(PassiveGlobalEffectInfo passiveGlobalEffectInfo)
            {
                this.PassiveGlobalEffectInfo = passiveGlobalEffectInfo;
                TurnsRemaining = passiveGlobalEffectInfo.TurnDuration;
            }

            public BasePassiveGlobalEffect(int turns, PassiveEffectType passiveEffectType)
            {
                TurnsRemaining = turns;
                PassiveGlobalEffectInfo = InfosLoadManager.Instance.GetPassiveGlobalEffectInfo(passiveEffectType);
            }

            public abstract void HandlePassiveEffect();

            public virtual void ApplyAsConditionalEffect()
            {
                Debug.Log("Conditional effect applying...");
            }

            public virtual void EndPassiveEffect(IEntity entityInit)
            {
                Debug.Log("End of passive effect...");
            }
        }

        public enum PassiveEffectType
        {
            TimeStop,
            Chronomaster
        }
    }
}