using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Wound : BasePassiveGlobalEffect
        {
            private const int DAMAGE_PENALTY = 20;

            public Wound(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {
                GameManager.Instance.OnEntityDamageDeal += TryToIncreaseIncomingDamage;
            }

            public Wound(int turns) : base(turns)
            {
                GameManager.Instance.OnEntityDamageDeal += TryToIncreaseIncomingDamage;
            }

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log("Entity is still being wounded");
            }

            private void TryToIncreaseIncomingDamage(ref int damage, IEntity damagedEntity, EffectCardDmgType effectCardDmgType)
            {
                if (damagedEntity != entityOwner)
                    return;

                Debug.Log($"{GetType().Name} passive effect is triggered");
                damage += DAMAGE_PENALTY;
                TurnsRemaining = 0;
                damagedEntity.PassiveEffectHandler.RemoveEffect(this);
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                GameManager.Instance.OnEntityDamageDeal -= TryToIncreaseIncomingDamage;
                base.EndPassiveEffect(entityInit);
            }
        }
    }
}
