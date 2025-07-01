using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using System.Collections;
using Panel;
using Multiplayer.Panel;

namespace Multiplayer
{
    public class TestPlayerSpawner : NetworkBehaviour
    {
        [SerializeField] GameObject playerPref;

        [SerializeField] List<PlayerController> playerList = new List<PlayerController>();

        [SerializeField] List<GameObject> spawnPanels = new List<GameObject>();
        [SerializeField] RectTransform contentPrefab;

        [SerializeField] Canvas playerHUD;

        [SerializeField] MapManager mapManager;
        [SerializeField] float playersZCordOffset;

        private List<ulong> connectedPlayers = new List<ulong>();

        public static TestPlayerSpawner Instance { get; private set; }

        public PlayerInfo CurrentPlayerInfo { get; private set; }

        /*private ulong currentPlayerClientId;*/

        private float currentZCordForPlayers = 0.5f;

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnect;
        }

        private void Update()
        {
            /*if (timer >= 2)
            {
                Debug.Log($"ConnectedPlayers ID: {connectedPlayers.Count}");
                Debug.Log($"playerListCount: {playerList.Count}");
                timer = 0;
            }
            timer += Time.deltaTime;*/

        }

        #region Стандартні мережеві функції
        private void OnPlayerDisconnect(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"Player disconnected ID: {clientId}");

            connectedPlayers.Remove(clientId);

            playerList.Remove(playerList.Find(player => player.GetPlayerId == clientId));

            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }

        private void OnPlayerConnect(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"Player connected ID: {clientId}");

            if (!connectedPlayers.Contains(clientId))
            {
                connectedPlayers.Add(clientId);
                AddPlayerToListAndSpawn(clientId);
            }

            UpdateConnectedPlayersClientRpc(connectedPlayers.ToArray());

            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };

            UpdateOwnInfoForPlayerClientRpc(GetPlayerWithId(clientId).GetPlayerInfo(), clientRpcParams);

            MapManagerSubscription(clientId);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                mapManager.playerMoveEnd += OnPlayerMoveEnd;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            /*currentPlayerClientId = OwnerClientId;
            Debug.Log($"Player ID saved: {currentPlayerClientId}");*/
        }
        #endregion

        #region ClientRpcs

        [ClientRpc]
        private void UpdateOwnInfoForPlayerClientRpc(PlayerInfo playerInfo, ClientRpcParams clientRpcParams = default)
        {
            CurrentPlayerInfo = playerInfo;
        }

        [ClientRpc]
        private void UpdateConnectedPlayersClientRpc(ulong[] playerIds)
        {
            connectedPlayers.Clear();
            connectedPlayers.AddRange(playerIds);
        }

        /*[ClientRpc]
        private void UpdatePlayerListClientRpc(ulong newPlayerId)
        {
            // Найдите игрока на клиенте и добавьте его в локальный список
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(newPlayerId, out var networkClient))
            {
                var player = networkClient.PlayerObject.gameObject;
                playerList.Add(player.GetComponent<PlayerController>());
            }
        }*/

        [ClientRpc]
        private void UpdateClientScrollViewClientRpc(PlayerInfo[] playerInfos) // Функція оновлення ScrollView(списка усіх поточних гравців) на HUD в усіх гравців
        {
            /*if (IsServer)
            {
                Debug.LogWarning("serverCall");
            }*/

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

                /*int playerHP = playerInfosList.Find(player => player.Id == ).GetPlayerHp;*/
                playerHPText.GetComponent<TextMeshProUGUI>().text = $"{playerInfo.hp}/100";

                newPlayerInfo.SetParent(content, false);
            }
        }

        [ClientRpc]
        private void UpdateAllHUDButtonsClientRpc(bool enable = false, ClientRpcParams clientRpcParams = default)
        {
            foreach (Transform ui in playerHUD.transform)
            {
                if (ui.CompareTag("Button"))
                {
                    ui.GetComponent<Button>().interactable = enable;
                }
            }
        }

        [ClientRpc]
        private void UpdateHudClientRpc(PlayerInfo playerInfo, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log($"playerInfo.Id: {playerInfo.Id}");

            playerHUD.transform.Find("CharacterModel").Find("PlayerHP").GetComponent<TextMeshProUGUI>().text = $"{playerInfo.hp}/100";
        }

        [ClientRpc]
        public void TurnPlayersHUDClientRpc(bool disable)
        {
            playerHUD.gameObject.SetActive(disable);
        }

        #endregion

        #region ServerRpcs

        [ServerRpc(RequireOwnership = false)]
        public void RequestUpdatePlayerHPServerRpc(ServerRpcParams rpcParams = default) // Функція визивається сервером, якщо гравець подасть запрос на зменшення хп
        {
            /*if (IsServer)
            {
                Debug.LogWarning("serverCall");
            }*/

            ulong senderId = rpcParams.Receive.SenderClientId;

            Debug.Log($"sender ID: {senderId}");

            var currentPlayer = playerList.Find(player => player.GetPlayerId == senderId);

            Debug.Log($"currentPlayer ID: {currentPlayer.GetPlayerId}");

            currentPlayer.GetDamage(20);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { currentPlayer.GetPlayerId }
                }
            };

            UpdateHudClientRpc(currentPlayer.GetPlayerInfo(), clientRpcParams);

            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCurrentPlayerInfoServerRpc(ServerRpcParams rpcParams = default) // Функція визивається сервером, якщо гравець подасть запрос на зменшення хп
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            var currentPlayer = playerList.Find(player => player.GetPlayerId == senderId);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { currentPlayer.GetPlayerId }
                }
            };

            UpdateOwnInfoForPlayerClientRpc(currentPlayer.GetPlayerInfo(), clientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestForPlayersInfoServerRpc()
        {
            if (IsServer)
            {
                foreach (var player in playerList)
                {
                    Debug.LogWarning($"Player {player.GetPlayerId} currentPanel: {player.GetCurrentPanel.name}");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DrawRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            if (CanPerformAction())
            { }

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { rpcParams.Receive.SenderClientId }
                }
            };

            UpdateAllHUDButtonsClientRpc(false, clientRpcParams);

            var player = playerList.Find(player => player.GetPlayerId == rpcParams.Receive.SenderClientId);

            mapManager.MakeADraw(player);
        }
        #endregion

        public IEnumerator TeleportPlayer(Vector2 cords, ulong playerId, PanelScript panelTrigger = null)
        {
            var targetTeleportPosition = new Vector3(cords.x, cords.y, currentZCordForPlayers);
            Debug.Log($"Teleporting Player with id: {playerId}");
            var player = GetPlayerWithId(playerId);
            player.transform.position = targetTeleportPosition;
            Debug.Log($"Player with id {playerId} got teleported!");

            if (panelTrigger != null)
            {
                while (player.GetCurrentPanel != panelTrigger)
                    yield return null;

                StartCoroutine(PanelEffectsManager.Instance
                    .TriggerPanelEffect(player.GetCurrentPanel.GetEffectPanelInfo.effect, player.GetPlayerId));
            }
        }

        #region Звичайні функції

        public List<PlayerController> GetPlayersList() => playerList;

        public void DealDamage(PlayerController player, int damage, bool isBlockable = false)
        {
            if (IsServer)
            {
                Debug.Log("Server call! (DealDamage)");
            }
            if (player.GetCurrentPanel.GetEffectPanelInfo.effect == PanelEffect.Hospital)
            {
                Debug.Log($"Cant damage player with id {player.GetPlayerId}");
                return;
            }

            player.GetDamage(isBlockable ? (damage - player.GetPlayerDef) : damage);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.GetPlayerId }
                }
            };

            UpdateHudClientRpc(player.GetPlayerInfo(), clientRpcParams);

            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }

        public void Heal(PlayerController player, int amount, bool isPercentage = false)
        {
            float percentage = (float)amount / 100;
            player.Heal(isPercentage ? ((int)(player.GetPlayerMaxHp * percentage)) : amount);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.GetPlayerId }
                }
            };

            UpdateHudClientRpc(player.GetPlayerInfo(), clientRpcParams);

            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }

        public PlayerController GetRandomPlayerExcept(ulong atkId, ulong defId)
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
        }

        public PlayerController GetRandomPlayerExcept(List<ulong> exceptions)
        {
            PlayerController randomPlayer = null;

            if (!playerList.Any(player => !exceptions.Contains(player.GetPlayerId)))
                return null;

            do
            {
                randomPlayer = playerList[UnityEngine.Random.Range(0, playerList.Count)];
            }
            while (exceptions.Contains(randomPlayer.GetPlayerId));

            return randomPlayer;
        }

        private void MapManagerSubscription(ulong clientId)
        {
            mapManager.SubscribeToPlayerMoveEndEvent(playerList.Find(player => player.GetPlayerId == clientId));
        }

        /*private void MapManagerUnsubscription(ulong clientId)
        {
            mapManager.UnsubscribeToPlayerMoveEndEvent(playerList.Find(player => player.GetPlayerId == clientId));
        }*/

        public PlayerController GetPlayerWithId(ulong id)
        {
            return playerList.Find(player => player.GetPlayerId == id);
        }

        private void OnPlayerMoveEnd(ulong Id)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { Id }
                }
            };

            UpdateAllHUDButtonsClientRpc(true, clientRpcParams);
        }

        public void TryToDraw() // Функція, щоб клієнт міг подати запрос про хід
        {
            /*RequestUpdatePlayerHPServerRpc();*/
            /*RequestForPlayersInfoServerRpc();*/
            DrawRequestServerRpc();
        }

        public void TryToUpdate() // Функція, щоб клієнт міг подати тестовий
        {
            /*RequestUpdatePlayerHPServerRpc();*/
            RequestForPlayersInfoServerRpc();

        }

        private bool CanPerformAction()
        {
            return true;
        }

        private PlayerInfo[] ReturnPlayerInfos() // Функція для повернення інформації про гравця у вигляді даних, взятих зі списку гравців серверу
        {
            List<PlayerInfo> playerInfos = new List<PlayerInfo>();

            foreach (var player in playerList)
            {
                playerInfos.Add(player.GetPlayerInfo());
            }

            return playerInfos.ToArray();
        }

        public void UpdateAllPlayersScrollViewInfo()
        {
            UpdateClientScrollViewClientRpc(ReturnPlayerInfos());
        }

        public void UpdatePlayerHUD(PlayerController player)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.GetPlayerId }
                }
            };

            UpdateHudClientRpc(player.GetPlayerInfo());
        }

        private void AddPlayerToListAndSpawn(ulong clientId) // Функція визивається сервером, щоб при заході гравця, додати його до переліку гравців серверу
        {
            if (!IsServer)
                return;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
            {
                var player = networkClient.PlayerObject.gameObject;
                player.GetComponent<PlayerController>().AddEffectCardsUsages();
                playerList.Add(player.GetComponent<PlayerController>());

                if (spawnPanels.Count != 0)
                {
                    player.transform.position = new Vector3(spawnPanels[0].transform.position.x, spawnPanels[0].transform.position.y, currentZCordForPlayers);
                    currentZCordForPlayers = playersZCordOffset;
                    spawnPanels.RemoveAt(0);
                }
                else
                    Debug.LogWarning("SpawnPanels are empty");

                /*UpdatePlayerListClientRpc(clientId);*/
            }
            else
                Debug.LogWarning("Player with clientId not found");
        }

        private bool IsPositionCloseEnough(Vector3 current, Vector3 target, float tolerance = 0.01f)
        {
            return Vector3.Distance(current, target) <= tolerance;
        }
        #endregion
    }
}
