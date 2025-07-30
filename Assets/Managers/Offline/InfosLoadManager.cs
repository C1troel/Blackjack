using Singleplayer.ActiveEffects;
using Singleplayer.PassiveEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class InfosLoadManager : MonoBehaviour
    {
        [SerializeField] private string CharactersInfoPath;
        [SerializeField] private string EnemiesInfoPath;
        [SerializeField] private string EffectCardsInfoPath;
        [SerializeField] private string PassiveGlobalEffectsInfoPath;
        [SerializeField] private string ActiveGlobalEffectsInfoPath;
        [SerializeField] private string EffectPanelsInfoPath;

        public static InfosLoadManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public EffectPanelInfoSingleplayer[] GetAllEffectPanlesInfo()
        {
            return Resources.LoadAll<EffectPanelInfoSingleplayer>(EffectPanelsInfoPath);
        }

        public ActiveGlobalEffectInfo[] GetAllActiveGlobalEffects(bool isEvent)
        {
            return Resources
                .LoadAll<ActiveGlobalEffectInfo>(ActiveGlobalEffectsInfoPath)
                .Where(effect => effect.IsEvent == isEvent)
                .ToArray();
        }

        public PassiveGlobalEffectInfo GetPassiveGlobalEffectInfo(PassiveEffectType passiveEffectType)
        {
            return Resources.Load<PassiveGlobalEffectInfo>(PassiveGlobalEffectsInfoPath + $"{passiveEffectType}");
        }

        public ActiveGlobalEffectInfo GetActiveGlobalEffectInfo(ActiveEffectType activeEffectType)
        {
            return Resources.Load<ActiveGlobalEffectInfo>(ActiveGlobalEffectsInfoPath + $"{activeEffectType}");
        }

        public EffectCardInfo[] GetAllEffectCardInfos()
        {
            return Resources.LoadAll<EffectCardInfo>(EffectCardsInfoPath);
        }

        public EnemyInfo[] GetAllEnemyInfos()
        {
            return Resources.LoadAll<EnemyInfo>(EnemiesInfoPath);
        }
        
        public CharacterInfo[] GetAllCharacterInfos()
        {
            return Resources.LoadAll<CharacterInfo>(CharactersInfoPath);
        }

        public EffectCardInfo GetEffectCardInfo(string effectCardName)
        {
            return Resources.Load<EffectCardInfo>(EffectCardsInfoPath + $"{effectCardName}");
        }

        public EnemyInfo GetEnemyInfo(string enemyName)
        {
            return Resources.Load<EnemyInfo>(EnemiesInfoPath + $"{enemyName}");
        }

        public CharacterInfo GetCharacterInfo(string characterName)
        {
            return Resources.Load<CharacterInfo>(CharactersInfoPath + $"{characterName}");
        }
    }
}