using Singeplayer;
using Singleplayer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private List<EnemyInfo> enemyInfosList;

    public static EnemySpawnManager Instance { get; private set; }
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
    }

    public BaseEnemy SpawnEnemy(Vector3 position, EnemyType enemyType)
    {
        var enemyInfo = enemyInfosList.Find(info => info.EnemyType == enemyType);

        var enemyGO = Instantiate(enemyPrefab, position, Quaternion.identity);
        var enemy = enemyGO.GetComponent<BaseEnemy>();
        enemy.SetupEnemy(enemyInfo);
        return enemy;
    }
}

public enum EnemyType
{
    Bodyguard = 1
}