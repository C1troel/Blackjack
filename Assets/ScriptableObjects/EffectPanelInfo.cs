using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panel;

/// <summary>
/// Використовується для панелей з ефектами
/// </summary>

[CreateAssetMenu(fileName = "EffectPanelInfo", menuName = "Infos/New EffectPanelInfo")]
public class EffectPanelInfo : ScriptableObject
{
    [SerializeField] private PanelEffect _effect;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private GameObject _panelPrefab;
    [SerializeField] private bool _isSpawn;
    [SerializeField] private int _amount;
    [SerializeField] private int _luckTier;
    [SerializeField] private bool _isForceStop;

    public Sprite sprite => _sprite;
    public GameObject panelPrefab => _panelPrefab;
    public bool isSpawn => _isSpawn;
    public PanelEffect effect => _effect;
    public int amount => _amount;
    public int luckTier => _luckTier;
    public bool isForceStop => _isForceStop;
}
