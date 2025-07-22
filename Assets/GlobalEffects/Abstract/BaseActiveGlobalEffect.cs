using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public abstract class BaseActiveGlobalEffect
        {
            public ActiveGlobalEffectInfo ActiveGlobalEffectInfo { get; private set; }
            public event Action GlobalEffectStateEvent;
            public int CooldownRounds { get; protected set; }
            protected IEntity entityOwner;

            public BaseActiveGlobalEffect(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null)
            {
                this.ActiveGlobalEffectInfo = activeGlobalEffectInfo;
                CooldownRounds = 0;
                this.entityOwner = entityOwner;
            }

            public abstract void TryToActivate();

            public virtual void OnNewTurnStart()
            {
                Debug.Log("Default behaviour of active effect...");
            }

            protected void OnGlobalEffectStateChange() => GlobalEffectStateEvent?.Invoke();

            public static BaseActiveGlobalEffect GetActiveGlobalEffectInstance(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner)
            {
                string currentNamespace = typeof(BaseActiveGlobalEffect).Namespace;

                string activeEffectClassName = $"{currentNamespace}.{activeGlobalEffectInfo.EffectType}";

                var activeEffect = Type.GetType(activeEffectClassName);

                if (activeEffect == null)
                {
                    Debug.LogError($"Unknown active effect: {activeEffectClassName}");
                    return null;
                }


                BaseActiveGlobalEffect activeEffectInstance = Activator.CreateInstance(
                    activeEffect,
                    activeGlobalEffectInfo,
                    entityOwner) as BaseActiveGlobalEffect;

                return activeEffectInstance;
            }
        }

        public enum ActiveEffectType
        {
            TimeStop
        }
    }
}