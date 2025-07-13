using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public abstract class BaseEffectCardLogic : IEffectCardLogic
    {
        public EffectCardInfo EffectCardInfo { get; protected set; }
        /*public List<EffectCardMaterial> EffectCardMaterials { get; protected set; }
        public EffectCardDmgType EffectCardDmgType { get; protected set; }*/

        public virtual void SetupEffectCardLogic(EffectCardInfo info)
        {
            EffectCardInfo = info;
            /*EffectCardMaterials = info.EffectCardMaterials;
            EffectCardDmgType = info.EffectCardDmgType;*/
        }

        public abstract void ApplyEffect(IEntity entityInit = null);
    }
}