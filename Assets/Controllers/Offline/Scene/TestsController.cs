using Singleplayer.ActiveEffects;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Singleplayer
{
    public class TestsController : MonoBehaviour
    {
        public static TestsController Instance { get; private set; }

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


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Пробел нажат!");
                TriggerTornadoEffect();
            }
        }

        private void KillRandomEntity()
        {
            var gameManager = GameManager.Instance;
            var enemy = gameManager.GetEntityWithType(EntityType.Enemy);
            gameManager.DealDamage(enemy, 500);
        }

        private void TriggerTornadoEffect()
        {
            var tornadoInfo = InfosLoadManager.Instance.GetActiveGlobalEffectInfo(ActiveEffectType.Tornado);
            var tornadoEffect = new Tornado(tornadoInfo);
            tornadoEffect.TryToActivate();
        }

        private void TeleportAllEntitiesToPanel()
        {
            var gameManager = GameManager.Instance;
            var entities = gameManager.GetEntitiesList();

            var vipClubPanels = MapManager.Instance.GetAllPanelsOfType(PanelEffect.VIPClub);
            var randomVIPClubPanel = vipClubPanels[Random.Range(0, vipClubPanels.Count)];


            foreach (var entity in entities)
            {
                entity.SuppressPanelEffectTrigger = true;
                gameManager.TeleportEntity(randomVIPClubPanel.transform.position, entity);
            }
        }

    }
}