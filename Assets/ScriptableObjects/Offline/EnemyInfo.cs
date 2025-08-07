using Singleplayer;
using Singleplayer.ActiveEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [CreateAssetMenu(fileName = "EnemyInfo", menuName = "Infos/New EnemyInfo")]
    public class EnemyInfo : ScriptableObject
    {

        [Header("Settings")]
        [SerializeField] private int _defaultHp;
        [SerializeField] private int _defaultAtk;
        [SerializeField] private int _defaultMoney;
        [SerializeField] private int _defaultDef;
        [SerializeField] private int _defaultCardUsages;
        [SerializeField] private int _maxEffectCards;
        [SerializeField] private bool _isBoss;
        [SerializeField] private bool _canTriggerPanels;
        [SerializeField] private ActiveGlobalEffectInfo _specialAbility;

        [Header("Meta Info")]
        [SerializeField] private string _enemyName;
        [SerializeField] private EntityType _entityType;
        [SerializeField] private EnemyType _enemyType;
        public int DefaultHp => _defaultHp;
        public int DefaultMoney => _defaultMoney;
        public int DefaultAtk => _defaultAtk;
        public int DefaultDef => _defaultDef;
        public int DefaultCardUsages => _defaultCardUsages;
        public int MaxEffectCards => _maxEffectCards;
        public bool CanTriggerPanels => _canTriggerPanels;
        public bool IsBoss => _isBoss;
        public ActiveGlobalEffectInfo SpecialAbility => _specialAbility;

        public string CharacterName => _enemyName;
        public EntityType EntityType => _entityType;
        public EnemyType EnemyType => _enemyType;
    }
}