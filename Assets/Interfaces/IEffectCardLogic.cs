using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IEffectCardLogic
    {
        IEnumerator ApplyEffect(Action onComplete, IEntity entityInit);
        void TryToUseCard(Action<bool> onComplete, IEntity entityInit);
        void SetupEffectCardLogic(EffectCardInfo info);
        bool CheckIfCanBeUsed(IEntity entityOwner);
        void ToggleMarkAsCounterCard(bool isMarked);

        EffectCardInfo EffectCardInfo { get; }
        List<IOutlinable> TargetObjectsList { get; }
        bool CanUse { get; }
        bool CanCounter { get; }
       /* public List<EffectCardMaterial> EffectCardMaterials { get; }
        public EffectCardDmgType EffectCardDmgType { get; }*/
    }
}