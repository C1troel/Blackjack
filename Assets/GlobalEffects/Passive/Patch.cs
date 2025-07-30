using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Patch : BasePassiveGlobalEffect
        {
            private const int DAMAGE_REDUCTION = 20;
            public Patch(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public Patch(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log("Entity is protected by patch");
            }

            public void NulifyDamage(ref int initialDamage) => initialDamage -= DAMAGE_REDUCTION;
        }
    }
}