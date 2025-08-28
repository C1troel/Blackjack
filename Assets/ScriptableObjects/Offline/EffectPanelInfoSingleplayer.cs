using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Singleplayer
{
    [CreateAssetMenu(fileName = "EffectPanelInfoSingleplayer", menuName = "Infos/New EffectPanelInfoSingleplayer")]
    public class EffectPanelInfoSingleplayer : ScriptableObject
    {
        [SerializeField] private PanelEffect _effect;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Sprite _frameSprite;
        [SerializeField] private bool _isSpawn;
        [SerializeField] private bool _allowDuplicates;
        [SerializeField] private bool _isForceStop;
        [SerializeField] private int _amount;
        [SerializeField] private int _luckTier;

        public Sprite Sprite => _sprite;
        public Sprite FrameSprite => _frameSprite;
        public bool IsSpawn => _isSpawn;
        public PanelEffect Effect => _effect;
        public int Amount => _amount;
        public int LuckTier => _luckTier;
        public bool IsForceStop => _isForceStop;
        public bool AllowDuplicates => _allowDuplicates;
    }
}