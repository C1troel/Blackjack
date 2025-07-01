using Singleplayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Infos/New EnemyInfo")]
public class EnemyInfo : ScriptableObject
{

    [Header("Base Stats")]
    [SerializeField] private int _defaultHp;
    [SerializeField] private int _defaultAtk;
    [SerializeField] private int _defaultMoney;
    [SerializeField] private int _defaultDef;
    [SerializeField] private int _defaultCardUsages;
    [SerializeField] private bool _isBoss;
    [SerializeField] private bool _canTriggerPanels;

    [Header("Meta Info")]
    [SerializeField] private string _enemyName;
    [SerializeField] private EntityType _entityType;
    public int DefaultHp => _defaultHp;
    public int DefaultMoney => _defaultMoney;
    public int DefaultAtk => _defaultAtk;
    public int DefaultDef => _defaultDef;
    public int DefaultCardUsages => _defaultCardUsages;
    public bool CanTriggerPanels => _canTriggerPanels;
    public bool IsBoss => _isBoss;
    public string CharacterName => _enemyName;
    public EntityType EntityType => _entityType;
}
