using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class Chronomaster : BasePassiveGlobalEffect
        {
            private const string TIME_STOP_BYPASS_LAYER = "ColorObject";

            public Chronomaster(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public Chronomaster(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                TurnsRemaining--;
                Debug.Log($"Allow moving in stopped time lasts for {TurnsRemaining} turns");
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                var entityMono = entityInit as MonoBehaviour;
                entityMono.gameObject.layer = 0;

                if (GlobalEffectsManager.Instance.CheckForTimeStoppers(entityInit))
                    entityMono.StartCoroutine(entityInit.StopAnimationSmoothly(2));
            }
        }
    }
}