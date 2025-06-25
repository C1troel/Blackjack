using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panel
{
    public class PanelSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject PanelPrefab;
        [SerializeField] private Vector3 StartCords;
        [SerializeField] private int SpawnCount;
        [SerializeField] private int xOffSet;
        [SerializeField] private int yOffSet;

        private List<GameObject> _spawnedPanels;
        private Vector2 _panelOffset;
        private List<GameObject> GetSpawnerPanels()
        {
            var result = _spawnedPanels.FindAll(panel => panel.tag == "SpawnPanel");

            return result;
        }
        /*=> _spawnedPanels.FindAll(panel => panel.tag == "Spawn");*/

        private void Start()
        {
            _spawnedPanels = new List<GameObject>();
            _panelOffset = ApplyPanelOffset();
        }

        public Vector2 ApplyPanelOffset()
        {
            var result = PanelPrefab.GetComponent<SpriteRenderer>().bounds.size;
            result.y += xOffSet;
            result.x += xOffSet;

            return result;
        }

        public void SpawnPanels(Object sender)
        {
            (Vector3, PanelScript.Pos) currentPanelInfo = (StartCords, PanelScript.Pos.None);
            GameObject previousPanel = null;

            for (int i = 0; i < SpawnCount; i++)
            {
                GameObject panel;

                if (i == 0)
                {
                    panel = SpawnPanel(StartCords, true);
                    panel.SetActive(true);
                    _spawnedPanels.Add(panel);
                    continue;
                }

                currentPanelInfo = GetSpawnCords(PanelScript.GetRandomSide(), currentPanelInfo.Item1);
                panel = SpawnPanel(currentPanelInfo.Item1);
                panel.SetActive(true);

                previousPanel = _spawnedPanels[_spawnedPanels.Count - 1] == null ? null : _spawnedPanels[_spawnedPanels.Count - 1];

                if (previousPanel != null)
                {
                    panel.GetComponent<PanelScript>()?.AssignSideOfPanel(previousPanel, currentPanelInfo.Item2);
                }

                _spawnedPanels.Add(panel);
            }

            ((SpawningManager)sender).AssingSpawners(GetSpawnerPanels());
        }

        private GameObject SpawnPanel(Vector3 spawnCords, bool isSpawn = false)
        {
            GameObject panel;

            panel = Instantiate(PanelPrefab);
            panel.transform.position = spawnCords;

            if (isSpawn)
            {
                panel.tag = "SpawnPanel";
                panel.transform.GetChild(0).tag = "SpawnPanel";
            }

            return panel;
        }

        private (Vector3, PanelScript.Pos) GetSpawnCords(PanelScript.Pos position, in Vector3 cords)
        {
            Vector3 SpawnCords = cords;

            switch (position)
            {
                case PanelScript.Pos.Left:
                    SpawnCords.x -= _panelOffset.x;
                    break;

                case PanelScript.Pos.Top:
                    SpawnCords.y -= _panelOffset.y;
                    break;

                case PanelScript.Pos.Right:
                    SpawnCords.x += _panelOffset.x;
                    break;

                case PanelScript.Pos.Bottom:
                    SpawnCords.y += _panelOffset.y;
                    break;

                default:
                    break;
            }

            return (SpawnCords, position);
        }
    }
}
