using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class BettingManager : MonoBehaviour
    {
        [SerializeField] private GameObject bettingPicker;
        [SerializeField] private GameObject bettingResultWindow;

        [SerializeField] int startRoundToAppearCounter;

        private ulong? currentPlayerPickerId = null;

        private Dictionary<IEntity, List<int>> playersBets = new Dictionary<IEntity, List<int>>();

        private int roundToAppearCounter;

        public static BettingManager Instance { get; private set; }

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

        public void AddPlayerBet(int playerBet)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);

            if (playersBets.ContainsKey(player) && IsInRange(playerBet, 1, 13))
            {
                if (playersBets[player].Contains(playerBet))
                {
                    Debug.Log($"Cannot Adding bet for player, player bet is {playerBet}");
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
                    Debug.Log($"Player in betting with bet {bets}");
                }
            }
        }

        private bool IsInRange(int value, float min, float max)
        {
            return value >= min && value <= max;
        }

        private void StartBettingClientRpc()
        {
            bettingResultWindow.SetActive(true);
        }
    }
}
