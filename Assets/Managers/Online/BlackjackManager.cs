using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackManager : NetworkBehaviour
{
    private const float WAIT_FOR_BETS_TIMER = 20f;

    [SerializeField] private GameObject blackjackCardPref;
    [SerializeField] private GameObject playerHandPref;

    [SerializeField] private Canvas blackjackHUD;
    [SerializeField] private GameObject deck;
    [SerializeField] private GameObject startBtnsContainer;
    [SerializeField] private GameObject betsContainer;
    [SerializeField] private GameObject dealerHand;

    [SerializeField] private GridLayoutGroup playerHandsContainer;

    [SerializeField] private Button hitBtn;
    [SerializeField] private Button standBtn;
    [SerializeField] private Button doubleBtn;
    [SerializeField] private Button splitBtn;
    [SerializeField] private Button insuranceBtn;

    [SerializeField] private float turnTime;

    [SerializeField] private string playingCardsPath = "Cards/PlayingCards";

    private Coroutine timerRunning;

    private List<Sprite> activeDeck = new List<Sprite>();
    private List<Sprite> cardsList = new List<Sprite>();

    private Dictionary<ulong, int> playersAndBets = new Dictionary<ulong, int>();

    private Vector2 topCardOfDeck = Vector2.zero;

    private bool isOnePlayer = false;

    private float timer;

    public static BlackjackManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (IsServer)
        {
            var sprites = Resources.LoadAll<Sprite>(playingCardsPath);
            cardsList.AddRange(sprites);
        }

        topCardOfDeck = deck.transform.GetChild(deck.transform.childCount - 1).position;
    }

    public void StartBlackjack(List<ulong> playerIds)
    {
        SetupDeck();

        if (playerIds.Count == 1)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerIds[0] }
                }
            };

            isOnePlayer = true;

            TurnStartBtnsClientRpc(true, clientRpcParams);
        }

        EnableBlackjackHUDClientRpc();
    }

    /*public void */

    public void StartGame()
    {
        RequestForStartGameServerRpc();
    }

    public void MakeBet(BetType betType)
    {
        RequestForMakeBetServerRpc(betType);
    }

    public void ExitGame()
    {
        startBtnsContainer.SetActive(false);
        betsContainer.SetActive(false);
        splitBtn.transform.parent.gameObject.SetActive(false);

        RequestForExitGameServerRpc();
    }

    private void SetupDeck()
    {
        activeDeck.Clear();
        activeDeck.AddRange(cardsList);

        System.Random random = new System.Random();
        activeDeck = activeDeck.OrderBy(x => random.Next()).ToList();
    }

    private int CheckMoney(ulong playerId, bool isHalf = false)
    {
        var gameManager = TestPlayerSpawner.Instance;

        var player = gameManager.GetPlayerWithId(playerId);

        int playerMoney = isHalf ? (player.GetPlayerMoney / 2) : player.GetPlayerMoney;

        if (playerMoney > 0)
            return playerMoney;

        Debug.Log($"Player {playerId} does not have enough money");
        return playerMoney;
    }
    
    private int CheckChips(ulong playerId, bool isHalf = false)
    {
        var gameManager = TestPlayerSpawner.Instance;

        var player = gameManager.GetPlayerWithId(playerId);

        int playerChips = isHalf ? (player.GetPlayerChips / 2) : player.GetPlayerChips;

        if (playerChips > 0)
            return playerChips;

        Debug.Log($"Player {playerId} does not have enough chips");
        return playerChips;
    }

    private IEnumerator WaitForPlayersBets(float actTimer = 0)
    {
        if (actTimer != 0)
            timer = actTimer;
        else
            timer = turnTime;

        bool restart;
        do
        {
            restart = false;
            timer -= Time.deltaTime;

            foreach (var player in playersAndBets.ToList())
            {
                if (player.Value == 0)
                {
                    Console.WriteLine($"Player {player.Key} still does`nt make a bet");
                    restart = true;
                    break;
                }
            }

            yield return null;

        } while (restart && timer > 0);

        if (timer <= 0)
        {
            foreach (var player in playersAndBets)
            {
                if (player.Value == 0)
                {
                    ClientRpcParams clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { player.Key }
                        }
                    };

                    ExitGameClientRpc(clientRpcParams);
                }
            }
        }

        if (timerRunning != null)
        {
            DeactivateTimerClientRpc();
        }
    }

    private IEnumerator Starttimer(Activity act, long playerInit = -1, float actTimer = 0)
    {
        if (actTimer != 0)
            timer = actTimer;
        else
            timer = turnTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            /*timerText.text = ((int)timer).ToString();*/
            yield return null;
        }

        /*timerText.text = "";*/

        if (IsServer)
        {
            switch (act)
            {
                case Activity.start:
                    // якщо при старті блекджека гравець нічого не обирає
                    break;

                case Activity.bets:

                    if (playerInit != -1)
                    {
                        ClientRpcParams clientRpcParams = new ClientRpcParams
                        {
                            Send = new ClientRpcSendParams
                            {
                                TargetClientIds = new[] { (ulong)playerInit }
                            }
                        };

                        ExitGameClientRpc(clientRpcParams);
                    }

                    break;

                case Activity.turn:
                    // якщо при виборі дії при 
                    break;

                default:
                    timerRunning = null;
                    yield break;
            }
        }

        timerRunning = null;
        yield break;
    }

    #region ServerRpc
    [ServerRpc(RequireOwnership = false)]
    private void RequestForStartGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (playersAndBets.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                }
            };

            TurnStartBtnsClientRpc(false, clientRpcParams);

            TurnBetsBtnsClientRpc(true, clientRpcParams);

            timerRunning = StartCoroutine(Starttimer(Activity.bets, (long)serverRpcParams.Receive.SenderClientId));

            /*else
                timerRunning = StartCoroutine(WaitForPlayersBets(WAIT_FOR_BETS_TIMER)); // можливо не тут? (бо це якщо багато гравців)*/
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForExitGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var senderId = serverRpcParams.Receive.SenderClientId;
        if (playersAndBets.ContainsKey(senderId))
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                }
            };

            playersAndBets.Remove(senderId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForMakeBetServerRpc(BetType betValue, ServerRpcParams serverRpcParams = default)
    {
        var senderId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        if (playersAndBets.ContainsKey(senderId) && playersAndBets[senderId] == 0)
        {

            int initialBet = 0;

            switch (betValue)
            {
                case BetType.AllMoneyIn1:
                    initialBet = CheckMoney(senderId);

                    if (initialBet <= 0)
                        ExitGameClientRpc(clientRpcParams);

                    AddPlayerHandClientRpc(senderId, initialBet, 1);

                    break;

                case BetType.AllMoneyIn2:
                    initialBet = CheckMoney(senderId, true);

                    if (initialBet <= 0)
                    {

                        ExitGameClientRpc(clientRpcParams);
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 2);
                    break;

                case BetType.HalfMoneyIn1:
                    initialBet = CheckMoney(senderId, true);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 1);
                    break;

                case BetType.HalfMoneyIn2:

                    initialBet = CheckMoney(senderId, true);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 2);

                    break;

                case BetType.AllChipsIn1:
                    initialBet = CheckChips(senderId);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 1);
                    break;

                case BetType.AllChipsIn2:
                    initialBet = CheckChips(senderId);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 2);
                    break;

                case BetType.HalfChipsIn1:
                    initialBet = CheckChips(senderId, true);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 1);
                    break;

                case BetType.HalfChipsIn2:
                    initialBet = CheckChips(senderId, true);

                    if (initialBet <= 0)
                    {
                        ExitGameClientRpc(clientRpcParams);
                        break;
                    }

                    AddPlayerHandClientRpc(senderId, initialBet, 2);
                    break;

                default:
                    break;
            }

            //playersAndBets[senderId] = betValue;
        }
    }
    #endregion

    #region ClientRpc

    [ClientRpc]
    private void DeactivateTimerClientRpc()
    {
        // виключення тексту таймера
        if (timerRunning != null)
        {
            StopCoroutine(timerRunning);
            timerRunning = null;
        }

        timer = 0;

    }

    [ClientRpc]
    private void AddPlayerHandClientRpc(ulong playerId, int playerBet, int handCount = 1)
    {
        for (int i = 0; i < handCount; i++)
        {
            var playerHand = Instantiate(playerHandPref, playerHandsContainer.transform);
            var playerHandController = playerHand.GetComponent<HandController>();

            playerHandController.ManageHandForPlayer(playerId, playerBet);
        }
    }

    [ClientRpc]
    private void ExitGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        ExitGame();
    }

    [ClientRpc]
    private void TurnStartBtnsClientRpc(bool isEnable, ClientRpcParams clientRpcParams = default)
    {
        startBtnsContainer.SetActive(isEnable);
    }

    [ClientRpc]
    private void TurnBetsBtnsClientRpc(bool isEnable, ClientRpcParams clientRpcParams = default)
    {
        betsContainer.SetActive(isEnable);

        if (isOnePlayer)
            timerRunning = StartCoroutine(Starttimer(Activity.bets));
        else
            timerRunning = StartCoroutine(WaitForPlayersBets(WAIT_FOR_BETS_TIMER));
    }

    [ClientRpc]
    private void EnableBlackjackHUDClientRpc()
    {
        blackjackHUD.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void ResetBlackjackManagerInfoClientRpc()
    {
        // функція для очистки даних "Гри в блекджек" по її закінченню
    }
    #endregion

    enum Activity 
    { 
        start,
        bets,
        turn
    }
    public enum BetType
    { 
        AllMoneyIn1,
        AllMoneyIn2,
        HalfMoneyIn1,
        HalfMoneyIn2,
        AllChipsIn1,
        AllChipsIn2,
        HalfChipsIn1,
        HalfChipsIn2
    }
}
