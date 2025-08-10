using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Split : BasePassiveGlobalEffect
        {
            public Split(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public Split(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                TurnsRemaining--;
                Debug.Log($"{GetType().Name} remains active for {TurnsRemaining}");
            }
        }
    }
}