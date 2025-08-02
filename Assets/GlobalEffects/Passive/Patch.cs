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
            {
                GameManager.Instance.OnEntityDamageDeal += TryToDecreaseIncomingDamage;
            }

            public Patch(int turns) : base(turns)
            {
                GameManager.Instance.OnEntityDamageDeal += TryToDecreaseIncomingDamage;
            }

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log("Entity is protected by patch");
            }

            public void TryToDecreaseIncomingDamage(ref int damage, IEntity damagedEntity)
            {
                if (damagedEntity != entityOwner)
                    return;

                Debug.Log($"{GetType().Name} passive effect is triggered");
                damage -= DAMAGE_REDUCTION;
                TurnsRemaining = 0;
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                GameManager.Instance.OnEntityDamageDeal -= TryToDecreaseIncomingDamage;
                base.EndPassiveEffect(entityInit);
            }
        }
    }
}