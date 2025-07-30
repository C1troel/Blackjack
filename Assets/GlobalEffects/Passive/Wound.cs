using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Wound : BasePassiveGlobalEffect
        {
            private const int ADDITIONAL_DAMAGE = 20;

            public Wound(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public Wound(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log("Entity is still being wounded");
            }

            public void IncreaseDamage(ref int initialDamage) => initialDamage += ADDITIONAL_DAMAGE;
        }
    }
}
