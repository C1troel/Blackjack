using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Plague : BasePassiveGlobalEffect
        {
            const int DAMAGE_AMOUNT = 10; // 10
            public Plague(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public Plague(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                GameManager.Instance.DealDamage(entityOwner, DAMAGE_AMOUNT, false);

                var closeEntities = MapManager
                    .FindEntitiesAtDistance(entityOwner.GetCurrentPanel, 5)
                    .Where(e => e != entityOwner);

                if (closeEntities.Any(entity => entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Plague)))
                    return;

                TurnsRemaining--;
            }
        }
    }
}
