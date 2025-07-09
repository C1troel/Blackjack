using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] string testPredefindCharacterName;
        [SerializeField] GameObject playerPref;

        [SerializeField] GameObject testPlayerSpawnPanel;
        [SerializeField] GameObject testEnemySpawnPanel;
        [SerializeField] GameObject inputBlock;

        [SerializeField] Canvas playerHUD;
        [SerializeField] PlayerHUDController playerHUDController;

        [SerializeField] MapManager mapManager; // потрібен інший менеджер мапи(офлайновий)
        [SerializeField] float playersZCordOffset;

        private float currentZCordForPlayers = 0.5f;

        private BasePlayerController playerData;
        private EntitySpawnManager enemySpawnManager;

        private List<IEntity> entitiesList = new List<IEntity>();
        public static GameManager Instance { get; private set; }

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
            enemySpawnManager = EntitySpawnManager.Instance;
            OnPlayerLoad();
            SpawnStartingEnemies();
            TurnManager.Instance.InitializeTurnOrder(entitiesList);
            TestAddingEffectCards(GetEntityWithType(EntityType.Player));
        }

        private void TestAddingEffectCards(IEntity entity)
        {
            EffectCardDealer.Instance.DealRandomEffectCard(entity);
        }

        public static void AddComponentByName(GameObject obj, string typeName)
        {
            Type type = Type.GetType(typeName);

            /*if (type == null)
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null)
                        break;
                }
            }*/

            if (type == null)
            {
                Debug.LogError($"ComponentHelper: Не вдалось знайти тип '{typeName}'");
                return;
            }

            if (!typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogError($"ComponentHelper: Тип '{typeName}' не є Component");
                return;
            }

            obj.AddComponent(type);
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
            var player = SpawnPlayerAndAddToList(testPlayerSpawnPanel.transform.position, CharacterType.TimeStopper);
            playerData = ((MonoBehaviour)player).GetComponent<BasePlayerController>();
            playerHUDController.ManagePlayerHud(player);
            MapManagerSubscription(player);
        }

        private void SpawnStartingEnemies()
        {
            var spawnedEnemy = enemySpawnManager.SpawnEnemy(testEnemySpawnPanel.transform.position, EnemyType.Bodyguard);
            entitiesList.Add(spawnedEnemy);
            spawnedEnemy.gameObject.SetActive(true);
            MapManagerSubscription(spawnedEnemy);
        }

        #region Старий код оновлення списку стану гравців
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
        #endregion

        /*private void ToggleAllHUDButtons(bool enable = false)
        {
            foreach (Transform ui in playerHUD.transform)
            {
                if (ui.CompareTag("Button"))
                {
                    ui.GetComponent<Button>().interactable = enable;
                }
            }
        }*/

        /*private void UpdatePlayerHud(IEntity player)
        {
            var playerSceneView = playerHUD.transform.Find("CharacterModel");

            for (int i = 0; i < playerSceneView.childCount; i++)
            {
                if (i == 0)
                {
                    var playerHpTextBox = playerSceneView.GetChild(i).GetComponent<TextMeshProUGUI>();
                    playerHpTextBox.text = $"{player.GetEntityHp}/{player.GetEntityMaxHp}";
                }
            }
        }*/

        public void TogglePlayersHUD(bool enable)
        {
            Debug.Log($"PlayerHUD activeness before: {playerHUD.gameObject.activeSelf}");
            playerHUD.gameObject.SetActive(enable);
            Debug.Log($"PlayerHUD activeness after: {playerHUD.gameObject.activeSelf}");
        }

        public void ToggleInputBlock(bool isActive)
        {
            inputBlock.SetActive(isActive);
        }

        private void DrawRequest()
        {
            if (CanPerformAction())
            { }

            ToggleInputBlock(true); // оновлення кнопок худа

            mapManager.MakeADraw(playerData);
        }

        public void StartPlayerTurn(BasePlayerController player)
        {
            DealDamage(player, 80, false);
            player.ResetEffectCardsUsages();
            ToggleInputBlock(false);
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

        public List<IEntity> GetEntitiesList() => entitiesList;

        public void DealDamage(IEntity entity, int damage, bool isBlockable = false)
        {
            if (entity.GetCurrentPanel != null && entity.GetCurrentPanel.GetEffectPanelInfo.effect == PanelEffect.Hospital)
            {
                Debug.Log($"Cant damage entity nameData: {((MonoBehaviour)entity).name}");
                return;
            }

            entity.GetDamage(isBlockable ? (damage - entity.GetEntityDef) : damage);

            // нижній метод потрібно оновити після того, як буде перелік ходів гравця та ворогів
            /*UpdateClientScrollViewClientRpc(ReturnPlayerInfos());*/
        }

        public void Heal(IEntity entity, int amount, bool isPercentage = false)
        {
            float percentage = (float)amount / 100;
            entity.Heal(isPercentage ? ((int)(entity.GetEntityMaxHp * percentage)) : amount);

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

        public IEntity GetRandomPlayerExcept(List<IEntity> exceptions)
        {
            IEntity randomEntity = null;

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

        /*private void OnPlayerMoveEnd(ulong Id)
        {
            ToggleInputBlock(true);
        }*/

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

        private IEntity SpawnPlayerAndAddToList(Vector3 cords, CharacterType characterType) // Функція визивається сервером, щоб при прогрузці гравця, додати його до переліку сутностей
        {
            var player = EntitySpawnManager.Instance.SpawnPlayableCharacter(cords, characterType);
            player.ResetEffectCardsUsages();
            entitiesList.Add(player);

            player.transform.position = new Vector3(testPlayerSpawnPanel.transform.position.x, testPlayerSpawnPanel.transform.position.y, currentZCordForPlayers);
            currentZCordForPlayers = playersZCordOffset;

            return player;
        }

        private bool IsPositionCloseEnough(Vector3 current, Vector3 target, float tolerance = 0.01f)
        {
            return Vector3.Distance(current, target) <= tolerance;
        }
    }
}
