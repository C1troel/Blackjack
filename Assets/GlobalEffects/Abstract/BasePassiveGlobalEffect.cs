using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public enum PassiveEffectType
        {
            TimeStop,
            Chronomaster,
            Wound,
            Patch,
            Plague,
            ShoppingMania,
            Poison,
            Rage,
            Clairvoyance,
            DoubleDown
        }

        public abstract class BasePassiveGlobalEffect
        {
            protected IEntity entityOwner;
            public PassiveGlobalEffectInfo PassiveGlobalEffectInfo { get; private set; }
            public int TurnsRemaining { get; protected set; }

            public BasePassiveGlobalEffect(PassiveGlobalEffectInfo passiveGlobalEffectInfo)
            {
                this.PassiveGlobalEffectInfo = passiveGlobalEffectInfo;
                TurnsRemaining = passiveGlobalEffectInfo.TurnDuration;
            }

            public BasePassiveGlobalEffect(int turns)
            {
                TurnsRemaining = turns;

                string className = GetType().Name;

                if (Enum.TryParse(className, out PassiveEffectType effectType))
                {
                    PassiveGlobalEffectInfo = InfosLoadManager.Instance.GetPassiveGlobalEffectInfo(effectType);
                }
                else
                {
                    Debug.LogError($"Failed to parse PassiveEffectType from class name '{className}'");
                }
            }

            public void CacheEntityOwner(IEntity entityOwner) => this.entityOwner = entityOwner;

            public abstract void HandlePassiveEffect(IEntity entityOwner);

            public virtual void ApplyAsConditionalEffect()
            {
                Debug.Log($"Conditional effect {this.PassiveGlobalEffectInfo.EffectType} applying...");
            }

            public virtual void TriggerImmediately()
            {
                Debug.Log($"{GetType().Name} being triggered immediately");
            }

            public virtual void EndPassiveEffect(IEntity entityInit)
            {
                Debug.Log($"End of passive effect {GetType().Name}...");
            }
        }
    }
}