using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IMapObject
    {
        void OnEntityStay(Action onCompleteCallback, IEntity stayedEntity);
    }
}