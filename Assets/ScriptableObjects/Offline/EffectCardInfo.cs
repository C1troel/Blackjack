using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [CreateAssetMenu(fileName = "EffectCardInfo", menuName = "Infos/New EffectCardInfo")]
    public class EffectCardInfo : ScriptableObject
    {
        [SerializeField] List<EffectCardMaterial> _effectCardMaterials;
        [SerializeField] EffectCardDmgType _effectCardDmgType;
        [SerializeField] EffectCardType _effectCardType;

        public List<EffectCardMaterial> EffectCardMaterials => _effectCardMaterials;
        public EffectCardDmgType EffectCardDmgType => _effectCardDmgType;
        public EffectCardType EffectCardType => _effectCardType;
    }
}