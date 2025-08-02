using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Rage : BasePassiveGlobalEffect
        {
            private const int DAMAGE_STAT_BONUS = 30;
            private const int DAMAGE_PENALTY = 10;

            public Rage(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {
                GameManager.Instance.OnEntityDamageDeal += TryToIncreaseIncomingDamage;
            }

            public Rage(int turns) : base(turns)
            {
                GameManager.Instance.OnEntityDamageDeal += TryToIncreaseIncomingDamage;
            }

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                Debug.Log($"{GetType().Name} passive effect is being triggered");
                entityOwner.RaiseAtkStat(DAMAGE_STAT_BONUS);

                TurnsRemaining--;
            }

            private void TryToIncreaseIncomingDamage(ref int damage, IEntity damagedEntity)
            {
                if (damagedEntity != entityOwner)
                    return;

                Debug.Log($"{GetType().Name} passive effect is triggered");
                damage += DAMAGE_PENALTY;
            }

            public override void TriggerImmediately()
            {
                entityOwner.RaiseAtkStat(DAMAGE_STAT_BONUS);
                base.TriggerImmediately();
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                GameManager.Instance.OnEntityDamageDeal -= TryToIncreaseIncomingDamage;
                base.EndPassiveEffect(entityInit);
            }
        }
    }
}
