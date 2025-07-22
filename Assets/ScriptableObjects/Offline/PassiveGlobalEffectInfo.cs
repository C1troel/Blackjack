using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        [CreateAssetMenu(fileName = "PassiveGlobalEffectInfo", menuName = "Infos/New PassiveGlobalEffect")]
        public class PassiveGlobalEffectInfo : ScriptableObject
        {
            [Header("Meta Info")]
            [SerializeField] private string _description;
            [SerializeField] private PassiveEffectType _effectType;

            [Header("Settings")]
            [SerializeField] private int _turnDuration;

            public string Description => _description;
            public PassiveEffectType EffectType => _effectType;
            public int TurnDuration => _turnDuration;
        }
    }
}