using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class RechargePanelEffect : IPanelEffect
    {
        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            onComplete?.Invoke();

            yield return null;

            MapManager.Instance.MakeADraw(entity);
        }
    }
}