using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Singleplayer;

namespace Multiplayer
{
    public class BettingManager : NetworkBehaviour
    {
        [SerializeField] private GameObject bettingPicker;
        [SerializeField] private GameObject bettingResultWindow;

        [SerializeField] int startRoundToAppearCounter;

        private ulong? currentPlayerPickerId = null;

        private Dictionary<PlayerController, List<int>> playersBets = new Dictionary<PlayerController, List<int>>();

        private int roundToAppearCounter;

        public static BettingManager Instance { get; private set; }
        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            roundToAppearCounter = startRoundToAppearCounter;
            StartCoroutine(bettingPicker.GetComponent<BettingPickerController>().SetUpBets());
        }

        private void OnRoundStart() // дана функція повинна визиватися з івента старта раунда !сервером!
        {
            --roundToAppearCounter;

            if (roundToAppearCounter == 0)
            {
                StartBettingClientRpc();
                roundToAppearCounter = startRoundToAppearCounter;
            }
        }

        public void EnablePicker() => StartCoroutine(bettingPicker.GetComponent<BettingPickerController>().EnablePicker());
        public void DisablePicker() => StartCoroutine(bettingPicker.GetComponent<BettingPickerController>().DisablePicker());

        [ServerRpc(RequireOwnership = false)]
        public void RequestForAddPlayerBetServerRpc(int playerBet, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            var player = TestPlayerSpawner.Instance.GetPlayerWithId(senderId);

            if (playersBets.ContainsKey(player) && IsInRange(playerBet, 1, 13))
            {
                if (playersBets[player].Contains(playerBet))
                {
                    Debug.Log($"Cannot Adding bet for player {senderId}, player bet is {playerBet}");
                }
                else if (IsInRange(playerBet, 1, 13))
                {
                    playersBets[player].Add(playerBet);
                }
            }
            else if (IsInRange(playerBet, 1, 13))
                playersBets.Add(player, new List<int> { playerBet });

            PanelEffectsManager.Instance.StopWaiting();

            Debug.Log("AllPlayers participates in betting:");
            foreach (var bet in playersBets)
            {
                foreach (var bets in bet.Value)
                {
                    Debug.Log($"Player in betting {bet.Key.GetPlayerId} with bet {bets}");
                }
            }
        }

        private bool IsInRange(int value, float min, float max)
        {
            return value >= min && value <= max;
        }

        [ClientRpc]
        private void StartBettingClientRpc()
        {
            bettingResultWindow.SetActive(true);
        }
    }
}