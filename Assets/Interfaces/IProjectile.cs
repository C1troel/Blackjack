using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IProjectile
    {
        void StartProjectileActivity(Action<IProjectile> callback);
    }
}