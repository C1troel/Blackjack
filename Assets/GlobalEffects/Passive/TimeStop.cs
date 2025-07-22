using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class TimeStop : BasePassiveGlobalEffect
        {
            public TimeStop(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public TimeStop(int turns, PassiveEffectType passiveEffectType) : base(turns, passiveEffectType)
            {}

            public override void HandlePassiveEffect()
            {
                TurnsRemaining--;
                Debug.Log($"Keep stopped time remaining {TurnsRemaining} turns");
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                GlobalEffectsManager.Instance.TryToResumeTime(entityInit);
            }
        }
    }
}