using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Singleplayer {

    public class EntitySpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject playableCharacterPrefab;
        private List<EnemyInfo> enemyInfosList;
        private List<CharacterInfo> characterInfosList;

        public static EntitySpawnManager Instance { get; private set; }
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

        private void Start()
        {
            enemyInfosList = InfosLoadManager.Instance.GetAllEnemyInfos().ToList();
            characterInfosList = InfosLoadManager.Instance.GetAllCharacterInfos().ToList();
        }

        public BasePlayerController SpawnPlayableCharacter(Vector3 position, CharacterType characterType)
        {
            var characterInfo = characterInfosList.Find(info => info.CharacterType == characterType);

            var characterGO = Instantiate(playableCharacterPrefab, position, Quaternion.identity);
            AttachPlayableCharacterScript(characterGO, characterType);

            var character = characterGO.GetComponent<BasePlayerController>();
            character.SetupPlayer(characterInfo);
            return character;
        }

        private void AttachPlayableCharacterScript(GameObject characterGO, CharacterType characterType)
        {
            string typeName = $"{this.GetType().Namespace}.{characterType}";
            GameManager.AddComponentByName(characterGO, typeName);
        }

        public BaseEnemy SpawnEnemy(Vector3 position, EnemyType enemyType)
        {
            var enemyInfo = enemyInfosList.Find(info => info.EnemyType == enemyType);

            var enemyGO = Instantiate(enemyPrefab, position, Quaternion.identity);
            AttachEnemyScript(enemyGO, enemyType);

            var enemy = enemyGO.GetComponent<BaseEnemy>();
            enemy.SetupEnemy(enemyInfo);
            return enemy;
        }

        private void AttachEnemyScript(GameObject enemyGO, EnemyType enemyType)
        {
            string typeName = $"{this.GetType().Namespace}.{enemyType}";
            GameManager.AddComponentByName(enemyGO, typeName);
        }
    }
}