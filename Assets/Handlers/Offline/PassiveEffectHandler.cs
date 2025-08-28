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

                effect.CacheEntityOwner(entity);
                effect.TriggerImmediately();
                activeEffects.Add(effect);
            }

            public void RemoveEffect(BasePassiveGlobalEffect effect)
            {
                if (!activeEffects.Contains(effect))
                    return;

                effect.EndPassiveEffect(entity);
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

                List<BasePassiveGlobalEffect> toRemove = new List<BasePassiveGlobalEffect>();

                foreach (var effect in activeEffects.ToList())
                {
                    effect.HandlePassiveEffect(entity);

                    if (effect.TurnsRemaining == 0)
                        toRemove.Add(effect);
                }

                foreach (var effect in toRemove)
                {
                    RemoveEffect(effect);
                }
            }

            public BasePassiveGlobalEffect GetEffect(PassiveEffectType effectType)
            {
                return activeEffects.Find(effect => effect.PassiveGlobalEffectInfo.EffectType == effectType);
            }
        }
    }
}