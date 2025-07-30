using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public interface IPassiveEffectHandler
        {
            void TryToAddEffect(BasePassiveGlobalEffect effect);
            void ProcessEffects();
            void ApplyAsConditionalEffect(PassiveEffectType effectType);
            void RemoveEffect(BasePassiveGlobalEffect effect);
            void RemoveAllEffects();
            bool CheckForActiveEffectType(PassiveEffectType effectType);
            BasePassiveGlobalEffect GetEffect(PassiveEffectType effectType);
        }
    }
}