using Panel;
using Singeplayer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] string testPredefindCharacterName;
        [SerializeField] GameObject playerPref;

        [SerializeField] List<PlayerController> entitiesList = new List<PlayerController>();

        [SerializeField] GameObject spawnPanel;

        [SerializeField] Canvas playerHUD;

        [SerializeField] MapManager mapManager; // потрібен інший менеджер мапи(офлайновий)
        [SerializeField] float playersZCordOffset;

        private PlayerController playerData;

        public static GameManager Instance { get; private set; }

        private float currentZCordForPlayers = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            OnPlayerLoad(); // метод при завантажені гравця на мапу
        }

        private void Start()
        {

        }

        private void Update()
        {
        }

        private void OnApplicationQuit() // збереження даних при виході під час гри
        {
            
        }

        private void OnApplicationPause(bool pause) // збереження даних при виході під час гри
        {
            
        }

        private void OnPlayerLoad()
        {
            var player = SpawnPlayerAndAddToList();
            playerData = ((MonoBehaviour)player).GetComponent<PlayerController>();
            MapManagerSubscription(player);
        }

        /*private void UpdateClientScrollViewClientRpc() // Функція оновлення ScrollView(списка усіх поточних сутностей)
        {
            *//*if (IsServer)
            {
                Debug.LogWarning("serverCall");
            }*//*

            var content = playerHUD.transform.Find("Scroll View").GetComponent<ScrollRect>().content;


            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            var playerInfosList = playerInfos.ToList();

            foreach (var playerInfo in playerInfos)
            {
                var newPlayerInfo = Instantiate(contentPrefab);

                var playerIdHandle = newPlayerInfo.GetComponent<TestPlayerInfoHandler>();
                playerIdHandle.PlayerId = playerInfo.Id;

                var playerName = newPlayerInfo.transform.Find("PlayerName");
                playerName.GetComponent<TextMeshProUGUI>().text = $"Player {playerInfo.Id}";

                var playerHPText = newPlayerInfo.transform.Find("PlayerHP");

                *//*int playerHP = playerInfosList.Find(player => player.Id == ).GetPlayerHp;*//*
                playerHPText.GetComponent<TextMeshProUGUI>().text = $"{playerInfo.hp}/100";

                newPlayerInfo.SetParent(content, false);
            }
        }*/

        private void UpdateAllHUDButtons(bool enable = false)
        {
            foreach (Transform ui in playerHUD.transform)
            {
                if (ui.CompareTag("Button"))
                {
                    ui.GetComponent<Button>().interactable = enable;
                }
            }
        }

        private void UpdatePlayerHud(IEntity player)
        {
            playerHUD.transform.Find("CharacterModel").Find("PlayerHP").GetComponent<TextMeshProUGUI>().text = $"{player.GetEntityHp}/{player.GetEntityMaxHp}";
        }

        public void TurnPlayersHUD(bool disable)
        {
            playerHUD.gameObject.SetActive(disable);
        }

        private void DrawRequest()
        {
            if (CanPerformAction())
            { }

            // оновлення кнопок худа

            mapManager.MakeADraw(playerData);
        }

        public IEnumerator TeleportEntity(Vector2 cords, IEntity entity, PanelScript panelTrigger = null)
        {
            var targetTeleportPosition = new Vector3(cords.x, cords.y, currentZCordForPlayers);
            Debug.Log($"Teleporting Entity with name: {entity.GetEntityName}");
            ((MonoBehaviour)entity).transform.position = targetTeleportPosition;
            Debug.Log($"Player with name {entity.GetEntityName} got teleported!");

            if (panelTrigger != null)
            {
                while (entity.GetCurrentPanel != panelTrigger)
                    yield return null;

                StartCoroutine(PanelEffectsManager.Instance
                    .TriggerPanelEffect(entity.GetCurrentPanel.GetEffectPanelInfo.effect, entity));
            }
        }

        public IEntity GetEntityWithType(EntityType entityType)
        {
            var searchedEntity = entitiesList.Find(entity => entity.GetEntityType == entityType);
            return searchedEntity;
        }

        public List<PlayerController> GetEntitiesList() => entitiesList;

        public void DealDamage(IEntity entity, int damage, bool isBlockable = false)
        {
            if (entity.GetCurrentPanel.GetEffectPanelInfo.effect == PanelEffect.Hospital)
            {
                Debug.Log($"Cant damage entity nameData: {((MonoBehaviour)entity).name}");
                return;
            }

            entity.GetDamage(isBlockable ? (damage - entity.GetEntityDef) : damage);

            if (entity.GetEntityType == EntityType.Player)
            {
                UpdatePlayerHud(entity);
            }

            // нижній метод потрібно оновити після того, як буде перелік ходів гравця та ворогів
            /*UpdateClientScrollViewClientRpc(ReturnPlayerInfos());*/
        }

        public void Heal(IEntity entity, int amount, bool isPercentage = false)
        {
            float percentage = (float)amount / 100;
            entity.Heal(isPercentage ? ((int)(entity.GetEntityMaxHp * percentage)) : amount);

            if (entity.GetEntityType == EntityType.Player)
            {
                UpdatePlayerHud(entity);
            }

            // нижній метод потрібно оновити після того, як буде перелік ходів гравця та ворогів
            /*UpdateClientScrollViewClientRpc(ReturnPlayerInfos());*/
        }

        /*public PlayerController GetRandomEntityExcept(IEntity atkId, ulong defId)
        {
            PlayerController randomPlayer = null;

            if (!playerList.Any(player => player.GetPlayerId != atkId && player.GetPlayerId != defId))
                return null;

            do
            {
                randomPlayer = playerList[UnityEngine.Random.Range(0, playerList.Count)];
            }
            while (randomPlayer.GetPlayerId == atkId || randomPlayer.GetPlayerId == defId);

            return randomPlayer;
        }*/

        public PlayerController GetRandomPlayerExcept(List<IEntity> exceptions)
        {
            PlayerController randomEntity = null;

            if (!entitiesList.Any(entity => !exceptions.Contains(entity)))
                return null;

            do
            {
                randomEntity = entitiesList[UnityEngine.Random.Range(0, entitiesList.Count)];
            }
            while (exceptions.Contains(randomEntity));

            return randomEntity;
        }

        private void MapManagerSubscription(IEntity entity)
        {
            mapManager.SubscribeToEntityMoveEndEvent(entity);
        }

        /*private void MapManagerUnsubscription(ulong clientId)
        {
            mapManager.UnsubscribeToPlayerMoveEndEvent(playerList.Find(player => player.GetPlayerId == clientId));
        }*/

        /*public PlayerController GetPlayerWithId(ulong id)
        {
            return playerList.Find(player => player.GetPlayerId == id);
        }*/

        private void OnPlayerMoveEnd(ulong Id)
        {
            UpdateAllHUDButtons(true);
        }

        public void TryToDraw() // Функція, щоб клієнт міг подати запрос про хід
        {
            /*RequestUpdatePlayerHPServerRpc();*/
            /*RequestForPlayersInfoServerRpc();*/
            DrawRequest();
        }

        public void TryToUpdate() // Функція, щоб клієнт міг подати тестовий запит
        {
            /*RequestUpdatePlayerHPServerRpc();*/
            /*RequestForPlayersInfoServerRpc();*/

        }

        private bool CanPerformAction()
        {
            return true;
        }

        /*private PlayerInfo[] ReturnPlayerInfos() // Функція для повернення інформації про гравця у вигляді даних, взятих зі списку гравців серверу
        {
            List<PlayerInfo> playerInfos = new List<PlayerInfo>();

            foreach (var player in playerList)
            {
                playerInfos.Add(player.GetPlayerInfo());
            }

            return playerInfos.ToArray();
        }*/

        /*public void UpdateAllPlayersScrollViewInfo()
        {
            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }*/

        /*public void UpdatePlayerHUD(PlayerController player)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.GetPlayerId }
                }
            };

            UpdateHudClientRpc(player.GetPlayerInfo());
        }*/

        private IEntity SpawnPlayerAndAddToList() // Функція визивається сервером, щоб при прогрузці гравця, додати його до переліку сутностей
        {
            var playerObj = Instantiate(playerPref);
            var player = playerObj.GetComponent<PlayerController>();
            player.SetupEntity(testPredefindCharacterName);
            player.AddEffectCardsUsages();
            entitiesList.Add(player);

            player.transform.position = new Vector3(spawnPanel.transform.position.x, spawnPanel.transform.position.y, currentZCordForPlayers);
            currentZCordForPlayers = playersZCordOffset;

            return player;
        }

        private bool IsPositionCloseEnough(Vector3 current, Vector3 target, float tolerance = 0.01f)
        {
            return Vector3.Distance(current, target) <= tolerance;
        }
    }
}
