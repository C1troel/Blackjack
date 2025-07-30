using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [CreateAssetMenu(fileName = "EffectCardInfo", menuName = "Infos/New EffectCardInfo")]
    public class EffectCardInfo : ScriptableObject
    {

        [Header("Meta Info")]
        [SerializeField] Sprite _effectCardSprite;

        [Header("Settings")]
        [SerializeField] int _effectiveDistanceInPanels;
        [SerializeField] int _effectsDuration;
        [SerializeField] List<EffectCardMaterial> _effectCardMaterials;
        [SerializeField] List<EffectCardPurpose> _effectCardPurposes;
        [SerializeField] EffectCardDmgType _effectCardDmgType;
        [SerializeField] EffectCardType _effectCardType;

        [Header("Vulnerabilities")]
        [SerializeField] private List<VulnerabilityDescriptor> _vulnerabilities;

        public List<VulnerabilityDescriptor> Vulnerabilities => _vulnerabilities;
        public List<EffectCardMaterial> EffectCardMaterials => _effectCardMaterials;
        public List<EffectCardPurpose> EffectCardPurposes => _effectCardPurposes;
        public EffectCardDmgType EffectCardDmgType => _effectCardDmgType;
        public EffectCardType EffectCardType => _effectCardType;
        public Sprite EffectCardSprite => _effectCardSprite;
        public int EffectiveDistanceInPanels => _effectiveDistanceInPanels;
        public int EffectsDuration => _effectsDuration;
    }
}