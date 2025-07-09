using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IEffectCard
    {
        void UseCard();
        List<EffectCardMaterial> GetCardMaterials { get; }

        void SetupEffectCard(EffectCardInfo effectCardInfo);
        EffectCardDmgType GetCardDmgType { get; }
    }

    public enum EffectCardMaterial
    {
        None,
        Metal,
        Glass,
        Liquid,
        Fire,
        Emotion,
        Paper
    }

    public enum EffectCardDmgType
    {
        None,
        Physical,
        Magical
    }
}