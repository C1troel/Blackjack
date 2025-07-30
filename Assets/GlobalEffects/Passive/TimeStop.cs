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

            public TimeStop(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
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