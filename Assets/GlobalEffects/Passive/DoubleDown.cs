using Singleplayer.PassiveEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class DoubleDown : BasePassiveGlobalEffect
    {
        public DoubleDown(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
        {}

        public DoubleDown(int turns) : base(turns)
        {}

        public override void HandlePassiveEffect(IEntity entityOwner)
        {
            TurnsRemaining--;
            Debug.Log($"{GetType().Name} remains active for {TurnsRemaining}");
        }
    }
}