using Singleplayer.ActiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class EventPanelEffect : IPanelEffect
    {
        private List<EventActiveGlobalEffectInfo> eventsList = new List<EventActiveGlobalEffectInfo>();

        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if (eventsList.Count == 0)
            {
                var activeGlobalEffects = InfosLoadManager.Instance.GetAllActiveGlobalEffects(true);

                foreach (var effect in activeGlobalEffects)
                    eventsList.Add(effect as EventActiveGlobalEffectInfo);
            }

            if (Random.Range(0, 2) == 0)
                TriggerBadEvent();
            else
                TriggerGoodEvent();

            yield return null;
            onComplete?.Invoke();
        }

        private void TriggerBadEvent()
        {
            var badEvents = eventsList.FindAll(globalEvent => !globalEvent.IsGood);

            var randomEvent = badEvents[Random.Range(0, badEvents.Count)];

            ActivateEventEffect(randomEvent);
        }

        private void TriggerGoodEvent()
        {
            var goodEvents = eventsList.FindAll(globalEvent => globalEvent.IsGood);

            var randomEvent = goodEvents[Random.Range(0, goodEvents.Count)];

            ActivateEventEffect(randomEvent);
        }

        private void ActivateEventEffect(EventActiveGlobalEffectInfo globalEvent)
        {
            var logicType = Type.GetType($"{globalEvent.GetType().Namespace}.{globalEvent.EffectType}");

            if (logicType == null)
            {
                Debug.LogError($"Unknown logic type: {globalEvent.EffectType}");
                return;
            }

            BaseActiveGlobalEffect globalEffect = Activator.CreateInstance(logicType, globalEvent, null) as BaseActiveGlobalEffect;
            globalEffect.TryToActivate();
        }
    }
}