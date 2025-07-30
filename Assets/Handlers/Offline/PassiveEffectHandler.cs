using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class PassiveEffectHandler : IPassiveEffectHandler
        {
            private readonly IEntity entity;
            private readonly List<BasePassiveGlobalEffect> activeEffects = new();

            public PassiveEffectHandler(IEntity entity)
            {
                this.entity = entity;
            }

            public void TryToAddEffect(BasePassiveGlobalEffect effect)
            {
                if (entity.GetEntityHp == 0)
                {
                    Debug.Log("Trying to apply passive effect to dead entity");
                    return;
                }

                var existingEffect = GetEffect(effect.PassiveGlobalEffectInfo.EffectType);
                if (existingEffect != null)
                {
                    activeEffects.Remove(existingEffect);
                }

                activeEffects.Add(effect);
            }

            public void RemoveEffect(BasePassiveGlobalEffect effect)
            {
                activeEffects.Remove(effect);
            }

            public void RemoveAllEffects()
            {
                foreach (var effect in activeEffects)
                    effect.EndPassiveEffect(entity);

                activeEffects.Clear();
            }

            public bool CheckForActiveEffectType(PassiveEffectType effectType)
            {
                return activeEffects.Any(effect =>
                    effect != null &&
                    effect.PassiveGlobalEffectInfo != null &&
                    effect.PassiveGlobalEffectInfo.EffectType == effectType);
            }

            public void ApplyAsConditionalEffect(PassiveEffectType effectType)
            {
                if (!activeEffects.Any(effect => effect.PassiveGlobalEffectInfo.EffectType == effectType))
                    return;

                var conditionalPassiveEffect = activeEffects.Find(effect => effect.PassiveGlobalEffectInfo.EffectType == effectType);
                conditionalPassiveEffect.ApplyAsConditionalEffect();
                RemoveEffect(conditionalPassiveEffect);
            }

            public void ProcessEffects()
            {
                if (activeEffects.Count == 0)
                    return;

                for (int i = activeEffects.Count - 1; i >= 0; i--)
                {
                    activeEffects[i].HandlePassiveEffect(entity);

                    if (activeEffects[i].TurnsRemaining == 0)
                    {
                        activeEffects[i].EndPassiveEffect(entity);
                        activeEffects.RemoveAt(i);
                    }
                }
            }

            public BasePassiveGlobalEffect GetEffect(PassiveEffectType effectType)
            {
                return activeEffects.Find(effect => effect.PassiveGlobalEffectInfo.EffectType == effectType);
            }
        }
    }
}