using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class UnstableTeleporter : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            var availablePortalPanels = MapManager.Instance.GetAllPanelsOfType(PanelEffect.Portal);
            var randomPortalPanel = availablePortalPanels[Random.Range(0, availablePortalPanels.Count)];

            entityInit.SuppressPanelEffectTrigger = true;
            GameManager.Instance.TeleportEntity(randomPortalPanel.transform.position, entityInit);
            onComplete?.Invoke();
            yield break;
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);
            if (MapManager.FindShortestPathConsideringDirection(entityInit.GetCurrentPanel, player.GetCurrentPanel, entityInit)
                .Count < 10)
            {
                onComplete?.Invoke(false);
                return;
            }

            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }
    }
}