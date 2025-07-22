using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.ActiveEffects;

namespace Singleplayer {
    [CreateAssetMenu(fileName = "CharacterInfo", menuName = "Infos/New CharacterInfo")]
    public class CharacterInfo : ScriptableObject
    {
        [Header("Base Stats")]
        [SerializeField] private int _defaultHp;
        [SerializeField] private int _defaultMoney;
        [SerializeField] private int _defaultChips;
        [SerializeField] private int _defaultAtk;
        [SerializeField] private int _defaultDef;
        [SerializeField] private int _defaultCardUsages;
        [SerializeField] private ActiveGlobalEffectInfo _activeGlobalEffect;

        [Header("Meta Info")]
        [SerializeField] private string _characterName;
        [SerializeField] private EntityType _entityType;
        [SerializeField] private CharacterType _characterType;
        public int DefaultHp => _defaultHp;
        public int DefaultMoney => _defaultMoney;
        public int DefaultChips => _defaultChips;
        public int DefaultAtk => _defaultAtk;
        public int DefaultDef => _defaultDef;
        public int DefaultCardUsages => _defaultCardUsages;
        public string CharacterName => _characterName;
        public EntityType EntityType => _entityType;
        public CharacterType CharacterType => _characterType;
        public ActiveGlobalEffectInfo SpecialAbility => _activeGlobalEffect;
    }
}