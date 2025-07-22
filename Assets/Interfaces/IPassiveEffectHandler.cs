using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public interface IPassiveEffectHandler
        {
            void AddEffect(BasePassiveGlobalEffect effect);
            void ProcessEffects();
            void RemoveEffect(BasePassiveGlobalEffect effect);
            bool CheckForActiveEffectType(PassiveEffectType effectType);
        }
    }
}