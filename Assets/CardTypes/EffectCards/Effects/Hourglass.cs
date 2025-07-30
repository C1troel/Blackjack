using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;
using UnityEngine.Video;

namespace Singleplayer
{
    public class Hourglass : BaseEffectCardLogic
    {
        private const string TIME_STOP_BYPASS_LAYER = "ColorObject";

        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            StopTime(entityInit);

            onComplete?.Invoke();
            yield break;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }

        private void StopTime(IEntity entityInit)
        {
            int passiveEffectsDuration = this.EffectCardInfo.EffectsDuration;
            var timeStopPassiveEffect = new TimeStop(passiveEffectsDuration);
            var chronoMasterPassiveEffect = new Chronomaster(passiveEffectsDuration);

            entityInit.PassiveEffectHandler.TryToAddEffect(timeStopPassiveEffect);
            entityInit.PassiveEffectHandler.TryToAddEffect(chronoMasterPassiveEffect);

            var entityMono = entityInit as MonoBehaviour;
            entityMono.gameObject.layer = LayerMask.NameToLayer(TIME_STOP_BYPASS_LAYER);

            if (entityInit.Animator.speed == 0)
                entityMono.StartCoroutine(entityInit.ResumeAnimationSmoothly(2));

            GlobalEffectsManager.Instance.StopTime(entityInit);
        }
    }
}