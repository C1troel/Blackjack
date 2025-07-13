using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IPanelEffect
    {
        IEnumerator Execute(IEntity entity, Action onComplete);
    }
}