using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        [CreateAssetMenu(fileName = "ActiveGlobalEffectInfo", menuName = "Infos/New ActiveGlobalEffect")]
        public class ActiveGlobalEffectInfo : ScriptableObject
        {
            [Header("Meta Info")]
            [SerializeField] private string _description;
            [SerializeField] private bool _isEvent;
            [SerializeField] private ActiveEffectType _effectType;

            [Header("Settings")]
            [SerializeField] private int _turnDuration;
            [SerializeField] private int _cooldownRounds;

            [Header("Vulnerabilities")]
            [SerializeField] private List<VulnerabilityDescriptor> _vulnerabilities;

            public List<VulnerabilityDescriptor> Vulnerabilities => _vulnerabilities;
            public string Description => _description;
            public ActiveEffectType EffectType => _effectType;
            public bool IsEvent => _isEvent;
            public int TurnDuration => _turnDuration;
            public int CooldownRounds => _cooldownRounds;
        }
    }
}