using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Panel;

public class SpawningManager : MonoBehaviour
{
    [SerializeField] GameObject PlayerPefab;
    [SerializeField] private GameObject _spawner;

    private List<GameObject> _spawns;
    private Transform _spawnPoint;
    private PanelSpawner _panelSpawner => _spawner.GetComponent<PanelSpawner>();

    void Start()
    {
        _panelSpawner.SpawnPanels(this);

        _spawnPoint = _spawns.Find(panel => panel.CompareTag("SpawnPanel")).transform;

        var player = Instantiate(PlayerPefab);
        player.transform.position = new Vector3(_spawnPoint.position.x, _spawnPoint.position.y, player.transform.position.z);

        player.SetActive(true);
    }

    void Update()
    {
        
    }

    public void AssingSpawners(List<GameObject> spawns)
    {
        _spawns = spawns;
    }
}
