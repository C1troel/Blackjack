using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class PortalPanelEffect : IPanelEffect
    {
        public IEnumerator Execute(IEntity entityInit, Action onComplete)
        {
            var portals = MapManager.Instance.GetAllPanelsOfType(PanelEffect.Portal);
            portals.RemoveAll(panel => panel == entityInit.GetCurrentPanel);

            var targetPos = portals[UnityEngine.Random.Range(0, portals.Count)].transform.position;
            
            GameManager.Instance.TeleportEntity(targetPos, entityInit);

            Debug.Log("PortalEffect: Teleport done.");

            yield return null;
            onComplete?.Invoke();
        }
    }
}