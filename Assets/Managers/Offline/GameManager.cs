using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
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
        [SerializeField] GameObject droppedMoneyPrefab;

        [SerializeField] Canvas playerHUD;
        [SerializeField] PlayerHUDManager playerHUDManager;

        [SerializeField] MapManager mapManager;
        [SerializeField] float playersZCordOffset;

        private float currentZCordForPlayers = 0.5f;

        public BasePlayerController PlayerData { get; private set; }
        public List<Sprite> BasicCardsList { get; private set; }
        public bool IsChoosing { get; private set; }

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
            BasicCardsList = SpriteLoadManager.Instance.GetAllBasicCardSprites();
            enemySpawnManager = EntitySpawnManager.Instance;

            MapManager.Instance.RandomlyAssignEffectPanels();
            /*StartGame();*/
        }

        #region Tests
        private IEnumerator TestAwaitAndReturnEntitiesAtDistanceFromEntity(IEntity startEntity, int distance)
        {
            yield return new WaitForSeconds(5);

            var foundEntitiesList = MapManager.FindEntitiesAtDistance(startEntity.GetCurrentPanel, distance);

            if (foundEntitiesList.Count == 0)
            {
                Debug.Log($"Doesn`t find any entity at distance {distance} from entity {startEntity.GetEntityName}");
            }
            else
            {
                Debug.Log($"Found entities at distance {distance} from entity {startEntity.GetEntityName}");

                foreach (var entity in foundEntitiesList)
                {
                    Debug.Log($"Found entity with name {entity.GetEntityName} within {distance} panels");
                }
            }
        }

        private IEnumerator TestAwaitAndReturnDistanceBetweenEntities(IEntity startEntity, IEntity targetEntity)
        {
            yield return new WaitForSeconds(5);

            var shortestPanelsAmount = MapManager.FindDistanceBetweenPanels(startEntity.GetCurrentPanel, targetEntity.GetCurrentPanel);
            Debug.Log($"Shortes panels amount from entity {startEntity} to {targetEntity} is {shortestPanelsAmount}");
        }
        
        private IEnumerator TestAwaitAndGetShortestAvailablePathBetweenPanels(IEntity startEntity, IEntity targetEntity)
        {
            yield return new WaitForSeconds(5);

            var shortestPath = MapManager.FindShortestPathConsideringDirection(startEntity.GetCurrentPanel, targetEntity.GetCurrentPanel, startEntity);

            Debug.Log($"Shortest path from {startEntity.GetEntityName} to {targetEntity.GetEntityName} contains {shortestPath.Count} panels");

            foreach (var panel in shortestPath)
            {
                Debug.Log($"{panel.name}");
            }
        }

        private void TestChangeTurnsOrder()
        {
            var player = entitiesList[0];
            var entity = entitiesList[1];

            entitiesList[0] = entity;
            entitiesList[1] = player;
        }

        private void TestPreselectCardForBlackjack()
        {
            var preselectedCard = SpriteLoadManager.Instance.GetBasicCardSprite("Club01");
            BlackjackManager.Instance.PreselectCardForNextGame(preselectedCard);
        }

        private void TestAddingEffectCards()
        {
            var player = entitiesList[0];
            var enemy = entitiesList[1];

            EffectCardDealer.Instance.DealEffectCardOfType(player, EffectCardType.Hourglass);
            EffectCardDealer.Instance.DealEffectCardOfType(enemy, EffectCardType.Hourglass);
        }
        #endregion

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

        public void StartGame()
        {
            OnPlayerLoad();
            SpawnStartingEnemies();

            /*TestChangeTurnsOrder();*/
            TurnManager.Instance.InitializeTurnOrder(entitiesList);

            #region Tests
            /*StartCoroutine(TestAwaitAndReturnEntitiesAtDistanceFromEntity(entitiesList[0], 6));
            StartCoroutine(TestAwaitAndGetShortestAvailablePathBetweenPanels(entitiesList[1], entitiesList[0]));
            StartCoroutine(TestAwaitAndReturnDistanceBetweenEntities(entitiesList[0], entitiesList[1]));*/
            TestAddingEffectCards();
            #endregion
        }

        private void OnPlayerLoad()
        {
            var player = SpawnPlayerAndAddToList(testPlayerSpawnPanel.transform.position, CharacterType.TimeStopper);
            PlayerData = ((MonoBehaviour)player).GetComponent<BasePlayerController>();
            playerHUDManager.ManagePlayerHud(player);
            MapManagerSubscription(player);
        }

        private void SpawnStartingEnemies()
        {
            var spawnedEnemy = enemySpawnManager.SpawnEnemy(testEnemySpawnPanel.transform.position, EnemyType.Bodyguard);
            entitiesList.Add(spawnedEnemy);
            spawnedEnemy.gameObject.SetActive(true);
            MapManagerSubscription(spawnedEnemy);
        }

        public void StartChoosingTarget(Action<IEntity> callback, List<IEntity> allowedTargets = null)
        {
            Debug.Log("Choosing is started");
            IsChoosing = true;
            IEntity chosenTarget = null;
            List<IEntity> possibleTargetEntities = new List<IEntity>();

            if (allowedTargets == null)
                possibleTargetEntities = entitiesList.Where(entity => entity.GetEntityType != EntityType.Player).ToList();
            else
                possibleTargetEntities = allowedTargets;

            foreach (var entity in possibleTargetEntities)
            {
                entity.OnSelfClickHandled += OnEntityChosen;
                entity.SetOutline();
            }

            void OnEntityChosen(IEntity entity)
            {
                chosenTarget = entity;

                foreach (var ent in possibleTargetEntities)
                {
                    ent.OnSelfClickHandled -= OnEntityChosen;
                    ent.RemoveOutline();
                }

                IsChoosing = false;

                callback?.Invoke(entity);
            }
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

        public void TogglePlayersHUD(bool enable)
        {
            Debug.Log($"PlayerHUD activeness before: {playerHUD.gameObject.activeSelf}");
            playerHUD.gameObject.SetActive(enable);
            Debug.Log($"PlayerHUD activeness after: {playerHUD.gameObject.activeSelf}");
        }

        /*public void ToggleInputBlock(bool isActive)
        {
            inputBlock.SetActive(isActive);
        }*/

        public void TogglePlayerHudButtons(bool isActive)
        {
            playerHUDManager.TogglePlayerHudButtons(isActive);
        }

        private void DrawRequest()
        {
            if (CanPerformAction())
            { }

            TogglePlayerHudButtons(false); // оновлення кнопок худа

            mapManager.MakeADraw(PlayerData);
        }

        public void SpecialAbilityRequest()
        {
            PlayerData.SpecialAbility.TryToActivate();
        }

        public void StartPlayerTurn(BasePlayerController player)
        {
            player.StartTurn();
        }

        public void TeleportEntity(Vector2 cords, IEntity entity, PanelScript panelTrigger = null)
        {
            StartCoroutine(TeleportEntityRoutine(cords, entity, panelTrigger));
        }

        private IEnumerator TeleportEntityRoutine(Vector2 cords, IEntity entity, PanelScript panelTrigger = null)
        {
            var targetTeleportPosition = new Vector3(cords.x, cords.y, currentZCordForPlayers);
            Debug.Log($"Teleporting Entity with name: {entity.GetEntityName}");
            ((MonoBehaviour)entity).transform.position = targetTeleportPosition;
            Debug.Log($"Player with name {entity.GetEntityName} got teleported!");

            if (entity.GetEntityType == EntityType.Player)
                ((MonoBehaviour)entity).GetComponentInChildren<CameraController>().ForceSnapToPlayer();

            if (panelTrigger != null)
            {
                while (entity.GetCurrentPanel != panelTrigger)
                    yield return null;

                StartCoroutine(PanelEffectsManager.Instance
                    .TriggerPanelEffect(entity.GetCurrentPanel, entity));
            }
        }

        public IEntity GetEntityWithType(EntityType entityType)
        {
            var searchedEntity = entitiesList.Find(entity => entity.GetEntityType == entityType);
            return searchedEntity;
        }

        public List<IEntity> GetEntitiesList() => entitiesList;

        public GameObject GetDroppedMoneyPrefab => droppedMoneyPrefab;
        public SavingMoneyController GetSavingMoneyController => playerHUDManager.GetSavingMoneyController;
        public ShoppingController GetShoppingController => playerHUDManager.GetShoppingController;

        public void DealDamage(IEntity entity, int damage, bool isBlockable = false)
        {
            if (entity == null || entity.GetEntityHp == 0)
            {
                Debug.LogWarning("Try to deal damage but entity is null or knocked out");
                return;
            }

            if (entity.GetCurrentPanel != null && entity.GetCurrentPanel.GetEffectPanelInfo.Effect == PanelEffect.VIPClub)
            {
                Debug.Log($"Cant damage entity nameData: {((MonoBehaviour)entity).name}");
                return;
            }

            if (entity.PassiveEffectHandler.GetEffect(PassiveEffectType.Wound) is Wound woundEffect)
            {
                woundEffect.IncreaseDamage(ref damage);
                entity.PassiveEffectHandler.ApplyAsConditionalEffect(PassiveEffectType.Wound);
            }
            if (entity.PassiveEffectHandler.GetEffect(PassiveEffectType.Patch) is Patch patchEffect)
            {
                patchEffect.NulifyDamage(ref damage);
                entity.PassiveEffectHandler.ApplyAsConditionalEffect(PassiveEffectType.Patch);
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
