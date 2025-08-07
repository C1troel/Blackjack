using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IProjectile
    {
        void StartProjectileActivity(Action<IProjectile> callback);
        Action ReflectProjectile();
        void Initialize(Action onComplete, IEntity targetEntity, IEntity entityOwner, PanelScript landingPanel, EffectCardInfo effectCardInfo);
        EffectCardInfo effectCardInfo { get;}
        IEntity entityOwner { get;}
    }
}