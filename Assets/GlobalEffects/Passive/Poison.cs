using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Poison : BasePassiveGlobalEffect
        {
            const int DAMAGE_AMOUNT = 20;
            public Poison(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            { }

            public Poison(int turns) : base(turns)
            { }

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log($"{GetType().Name} passive effect is being triggered");
                GameManager.Instance.DealDamage(entityOwner, DAMAGE_AMOUNT, false);

                TurnsRemaining--;
            }
        }
    }
}