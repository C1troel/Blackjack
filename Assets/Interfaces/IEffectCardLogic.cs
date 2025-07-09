using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IEffectCardLogic
    {
        void ApplyEffect(IEntity entityInit);
        void SetupEffectCardLogic(EffectCardInfo info);

        public List<EffectCardMaterial> EffectCardMaterials { get; }
        public EffectCardDmgType EffectCardDmgType { get; }
    }
}