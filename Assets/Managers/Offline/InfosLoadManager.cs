using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class InfosLoadManager : MonoBehaviour
    {
        [SerializeField] private string CharactersInfoPath;
        [SerializeField] private string EnemiesInfoPath;

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