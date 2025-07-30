using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        [CreateAssetMenu(fileName = "EventActiveGlobalEffectInfo", menuName = "Infos/New EventActiveGlobalEffect")]
        public class EventActiveGlobalEffectInfo : ActiveGlobalEffectInfo
        {
            [Header("Event data")]
            [SerializeField] private bool _isGood;

            public bool IsGood => _isGood;
        }
    }
}
