using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Services.Qos.V2.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Singleplayer
{
    public class BlackjackManager : MonoBehaviour
    {
        private const int DEALER_STAND_SCORE = 17;
        private const int MAX_PLAYER_HANDS = 4;
        private const float TOP_CARD_OF_DECK_OFFSET = 27.2f;

        [Header("Prefabs")]
        [SerializeField] private GameObject blackjackCardPref;
        [SerializeField] private GameObject playerHandPref;

        [Header("Scene objects")]
        [SerializeField] private Canvas blackjackHUD;
        [SerializeField] private GameObject deck;
        [SerializeField] private GameObject startBtnsContainer;
        [SerializeField] private GameObject betsContainer;
        [SerializeField] private HandController dealerHand;
        [SerializeField] private GridLayoutGroup playerHandsContainer;

        [Header("Control buttons")]
        [SerializeField] private Button hitBtn;
        [SerializeField] private Button standBtn;
        [SerializeField] private Button doubleBtn;
        [SerializeField] private Button splitBtn;
        [SerializeField] private Button insuranceBtn;

        [Header("Settings")]
        [SerializeField] private float cardGiveSpeed;
        [SerializeField] private string playingCardsPath = "Cards/PlayingCards";

        private GameObject controlBtnsParent;

        private List<Sprite> activeDeck = new List<Sprite>();
        private List<Sprite> cardsList = new List<Sprite>();

        private Vector3 topCardOfDeck = Vector3.zero;

        private PlayerAct playerAct = PlayerAct.None;

        public bool isBlackjackGameRunning { get; private set; } = false;

        public static BlackjackManager Instance { get; private set; }

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
            var sprites = Resources.LoadAll<Sprite>(playingCardsPath);
            cardsList.AddRange(sprites);

            controlBtnsParent = hitBtn.transform.parent.gameObject;
        }

        public void StartBlackjack()
        {
            GameManager.Instance.TogglePlayersHUD(false);
            isBlackjackGameRunning = true;

            SetupDeck();

            ToggleStartBtns(true);

            EnableBlackjackHUD();

            topCardOfDeck = deck.transform.GetChild(deck.transform.childCount - 1).position;
            topCardOfDeck.y -= TOP_CARD_OF_DECK_OFFSET;
        }

        public void StartGame()
        {
            RequestForStartGame();
        }

        public void ExitGame()
        {
            Debug.Log("Blackjack game exit...");
            blackjackHUD.gameObject.SetActive(false);
            GameManager.Instance.TogglePlayersHUD(true);

            isBlackjackGameRunning = false;

            startBtnsContainer.SetActive(false);
            betsContainer.SetActive(false);
            ToggleControlBtns(false);

            playerAct = PlayerAct.None;

            foreach (Transform hand in playerHandsContainer.transform)
                Destroy(hand.gameObject);

            foreach (Transform hand in dealerHand.transform)
                Destroy(hand.gameObject);
        }

        private void SetupDeck()
        {
            activeDeck.Clear();
            activeDeck.AddRange(cardsList);

            System.Random random = new System.Random();
            activeDeck = activeDeck.OrderBy(x => random.Next()).ToList();
        }

        private int CheckPlayerMoney(bool isHalf = false)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            int playerMoney = isHalf ? (player.GetEntityMoney / 2) : player.GetEntityMoney;

            if (playerMoney > 0)
                return playerMoney;

            Debug.Log($"Player does not have enough money");
            return playerMoney;
        }

        private int CheckPlayerChips(bool isHalf = false)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            int playerChips = isHalf ? (player.GetEntityChips / 2) : player.GetEntityChips;

            if (playerChips > 0)
                return playerChips;

            Debug.Log($"Player does not have enough chips");
            return playerChips;
        }

        private void RequestForStartGame()
        {
            ToggleStartBtns(false);

            ToggleBetsBtns(true); // на якомусь етапі потрібно зробити виключення неможливих ставок
        }

        public void MakeBetAllMoneyIn1() => MakeBet(BetType.AllMoneyIn1);
        public void MakeBetAllMoneyIn2() => MakeBet(BetType.AllMoneyIn2);
        public void MakeBetHalfMoneyIn1() => MakeBet(BetType.HalfMoneyIn1);
        public void MakeBetHalfMoneyIn2() => MakeBet(BetType.HalfMoneyIn2);
        public void MakeBetAllChipsIn1() => MakeBet(BetType.AllChipsIn1);
        public void MakeBetAllChipsIn2() => MakeBet(BetType.AllChipsIn2);
        public void MakeBetHalfChipsIn1() => MakeBet(BetType.HalfChipsIn1);
        public void MakeBetHalfChipsIn2() => MakeBet(BetType.HalfChipsIn2);

        private void MakeBet(BetType betValue)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;
            int handsCount = 0;
            int initialBet = 0;
            int playerAllMoney = CheckPlayerMoney();
            int playerHalfMoney = CheckPlayerMoney(true);
            int playerAllChips = CheckPlayerChips();
            int playerHalfChips = CheckPlayerChips(true);


            switch (betValue)
            {
                case BetType.AllMoneyIn1:

                    if (playerAllMoney <= 0)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerAllMoney, false);
                    initialBet = playerAllMoney;
                    handsCount = 1;

                    break;

                case BetType.AllMoneyIn2:

                    if (playerAllMoney <= 1)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerAllMoney, false);
                    initialBet = playerAllMoney;
                    handsCount = 2;
                    break;

                case BetType.HalfMoneyIn1:

                    if (playerHalfMoney <= 0)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerHalfMoney, false);
                    initialBet = playerHalfMoney;
                    handsCount = 1;
                    break;

                case BetType.HalfMoneyIn2:

                    if (playerHalfMoney <= 1)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerHalfMoney, false);
                    initialBet = playerHalfMoney;
                    handsCount = 2;
                    break;

                case BetType.AllChipsIn1:

                    if (playerAllChips <= 0)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerAllChips, true);
                    initialBet = playerAllChips;
                    handsCount = 1;
                    break;

                case BetType.AllChipsIn2:

                    if (playerAllChips <= 1)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerAllChips, true);
                    initialBet = playerAllChips;
                    handsCount = 2;
                    break;

                case BetType.HalfChipsIn1:

                    if (playerHalfChips <= 0)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerHalfChips, true);
                    initialBet = playerHalfChips;
                    handsCount = 1;
                    break;

                case BetType.HalfChipsIn2:

                    if (playerHalfChips <= 1)
                    {
                        ExitGame();
                        break;
                    }

                    player.Pay(playerHalfChips, true);
                    initialBet = playerHalfChips;
                    handsCount = 1;
                    break;

                default:
                    break;
            }

            for (int i = 0; i < handsCount; i++)
            {
                if (i == handsCount-1)
                {
                    AddPlayerHand(initialBet);
                    break;
                }

                AddPlayerHand(initialBet / handsCount);
                initialBet /= handsCount;
            }

            betsContainer.SetActive(false);
            StartCoroutine(DealFirstCards());
        }

        private void AddPlayerHand(int playerBet)
        {
            var playerHand = Instantiate(playerHandPref, playerHandsContainer.transform);
            var playerHandController = playerHand.GetComponent<HandController>();

            playerHandController.ManageHandForPlayer(playerBet);
        }

        private IEnumerator DealFirstCards()
        {
            yield return StartCoroutine(DealCardsForPlayer());

            yield return StartCoroutine(SpawnNextBlackjackCard(-1, true, false)); // звичайна карта дилеру

            yield return StartCoroutine(DealCardsForPlayer());

            yield return StartCoroutine(SpawnNextBlackjackCard(-1, true, true)); // перевернута карта дилеру

            StartCoroutine(HandlePlayerActions());
        }

        private IEnumerator DealCardsForPlayer()
        {
            foreach (Transform hand in playerHandsContainer.transform) // роздаємо карти гравцю
            {
                var handNumber = hand.GetSiblingIndex();
                yield return StartCoroutine(SpawnNextBlackjackCard(handNumber, false, false));
            }
        }

        private IEnumerator HandlePlayerActions()
        {
            ToggleControlBtns(true);

            for (int i = 0; i < playerHandsContainer.transform.childCount;)
            {
                var hand = playerHandsContainer.transform.GetChild(i);
                var handController = hand.GetComponent<HandController>();

                if (handController.GetHandScores() > 21)
                {
                    Debug.Log("Busted Hand");
                    ++i;
                    continue;
                }
                else if (handController.CheckForBlackjack())
                {
                    Debug.Log($"Player has blackjack on hand with number {i}");
                    ++i;
                    continue;
                }

                EnableAvailableActionsForHand(handController);

                while (playerAct == PlayerAct.None)
                    yield return null;

                ResetControllBtnsActiveness();
                var handledPlayerAct = playerAct;
                playerAct = PlayerAct.None;

                switch (handledPlayerAct)
                {
                    case PlayerAct.Hit:
                        yield return StartCoroutine(HandleHitPlayerAct(i));
                        continue;

                    case PlayerAct.Stand:
                        ++i;
                        continue;

                    case PlayerAct.Split:
                        HandleSplitPlayerAct(i);
                        continue;

                    case PlayerAct.Double:
                        yield return StartCoroutine(HandleDoubleDownPlayerAct(i));
                        ++i;
                        break;

                    case PlayerAct.Insurance:
                        HandleInsurancePlayerAct(i);
                        ++i;
                        break;
                        
                    default:
                        Debug.LogWarning("handledPlayerAct is None???");
                        break;
                }
            }

            StartCoroutine(PlayDealerTurnAndCheckWinners());
        }

        private IEnumerator PlayDealerTurnAndCheckWinners()
        {
            ToggleControlBtns(false);

            var facedDownCard = dealerHand.GetHandFirstCard().GetAddCardsContainer.GetComponentInChildren<BlackjackCard>();
            facedDownCard.FlipCard();
            yield return new WaitForSeconds(2f);

            bool isDealerBlackjack = dealerHand.CheckForBlackjack();

            while (dealerHand.GetHandScores() < DEALER_STAND_SCORE)
                yield return SpawnNextBlackjackCard(-1, true, false);

            int dealerHandScore = dealerHand.GetHandScores();
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            foreach (Transform hand in playerHandsContainer.transform)
            {
                var handController = hand.GetComponent<HandController>();
                int playerHandScore = handController.GetHandScores();

                if (isDealerBlackjack && handController.isInsured)
                {
                    int totalWin = handController.handBet * 2;
                    player.GainMoney(totalWin, false);
                    Debug.Log($"Hand with total score of {playerHandScore} win insure bet with total prize of {totalWin}");
                }
                else if (playerHandScore < dealerHandScore || playerHandScore > 21)
                {
                    Debug.Log($"Hand with total score of {playerHandScore} loose!!!");
                    continue;
                }
                else if (playerHandScore > dealerHandScore)
                {
                    int win = (int)(handController.hasBlackjack ? handController.handBet * 2.5 : handController.handBet * 2);
                    int totalWin = handController.isInsured ? win * 2 : win;

                    player.GainMoney(totalWin, false);
                    Debug.Log($"Hand with total score of {playerHandScore} win with total prize of {totalWin}");
                }
                else
                {
                    int totalWin = handController.handBet;
                    player.GainMoney(totalWin, false);

                    Debug.Log($"Hand with total score of {playerHandScore} pushes with total prize of {totalWin}");
                }
            }

            ExitGame();
        }

        private void HandleInsurancePlayerAct(int handNumber)
        {
            var handController = playerHandsContainer.transform.GetChild(handNumber).GetComponent<HandController>();

            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            var insuranceBet = handController.handBet / 2;

            if (player.GetEntityChips >= insuranceBet)
                player.Pay(insuranceBet, true);
            else
                player.Pay(insuranceBet, false);

            handController.Insure();
        }

        private void HandleSplitPlayerAct(int handNumber)
        {
            var handController = playerHandsContainer.transform.GetChild(handNumber).GetComponent<HandController>();
            var firstHandCard = handController.GetHandFirstCard();
            var secondHandCard = firstHandCard.GetAddCardsContainer.transform.GetChild(0);

            var newHand = Instantiate(playerHandPref, playerHandsContainer.transform);
            newHand.transform.SetSiblingIndex(handNumber+1);

            var newHandController = newHand.GetComponent<HandController>();
            newHandController.ManageHandForPlayer(handController.handBet);

            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            if (player.GetEntityChips >= handController.handBet)
                player.Pay(handController.handBet, true);
            else
                player.Pay(handController.handBet, false);

            secondHandCard.SetParent(newHandController.GetCardsContainer.transform);
            newHandController.GetHandScores();
        }

        private IEnumerator HandleDoubleDownPlayerAct(int handNumber)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            var handController = playerHandsContainer.transform.GetChild(handNumber).GetComponent<HandController>();

            if (player.GetEntityChips >= handController.handBet)
                player.Pay(handController.handBet, true);
            else
                player.Pay(handController.handBet, false);

            handController.DoubleDown();

            yield return StartCoroutine(SpawnNextBlackjackCard(handNumber, false, false));
        }

        private IEnumerator HandleHitPlayerAct(int handNumber)
        {
            yield return StartCoroutine(SpawnNextBlackjackCard(handNumber, false, false));
        }

        private void EnableAvailableActionsForHand(HandController hand)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            if (hand.GetHandScores() > 21)
            {
                Debug.LogWarning("Try to act with busted hand");
                return;
            }

            hitBtn.interactable = true;
            standBtn.interactable = true;

            if (!hand.isDoubled && CanPlaceAdditionalBet(hand.handBet, PlayerAct.Double))
                doubleBtn.interactable = true;

            if (!hand.isInsured && hand.GetCardsCount() == 2
                && dealerHand.GetHandFirstCard().GetCardValue() == 11
                && CanPlaceAdditionalBet(hand.handBet, PlayerAct.Insurance))
            {
                insuranceBtn.interactable = true;
            }

            if (hand.GetCardsCount() == 2 && CheckForPlayerSplit(hand) 
                && CanPlaceAdditionalBet(hand.handBet, PlayerAct.Split)
                && GetPlayerHandsCount != MAX_PLAYER_HANDS)
            {
                splitBtn.interactable = true;
            }
        }

        private bool CanPlaceAdditionalBet(int handBet, PlayerAct playerAct)
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;
            var playerMoney = player.GetEntityMoney;
            var playerChips = player.GetEntityChips;

            switch (playerAct)
            {
                case PlayerAct.Split:

                    if (playerChips >= handBet || playerMoney >= handBet)
                        return true;
                    else
                        return false;

                case PlayerAct.Double:

                    if (playerChips >= handBet || playerMoney >= handBet)
                        return true;
                    else
                        return false;

                case PlayerAct.Insurance:

                    int insuranceBet = handBet / 2;

                    if (playerChips >= insuranceBet || playerMoney >= insuranceBet)
                        return true;
                    else
                        return false;

                default:
                    return true;
            }
        }

        private void ToggleControlBtns(bool isActive)
        {
            controlBtnsParent.SetActive(isActive);
        }

        private void ResetControllBtnsActiveness()
        {
            hitBtn.interactable = false;
            standBtn.interactable = false;
            insuranceBtn.interactable = false;
            splitBtn.interactable= false;
            doubleBtn.interactable = false;
        }

        private bool CheckForPlayerSplit(HandController handController)
        {
            var firsCard = handController.GetHandFirstCard();

            if (firsCard == null || firsCard.transform.childCount != 1)
            {
                Debug.LogWarning("Checking split fast return");
                return false;
            }

            var firstCardName = firsCard.gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name;
            int firstCardScore = int.Parse(firstCardName.Substring(firstCardName.Length - 2));

            var secondCard = firsCard.GetAddCardsContainer.transform.GetChild(0).GetComponent<BlackjackCard>();
            int secondCardScore = int.Parse(firstCardName.Substring(firstCardName.Length - 2));

            return firstCardScore == secondCardScore;
        }

        private IEnumerator SpawnNextBlackjackCard(int handNumber = -1, bool isDealer = true, bool facedDown = false)
        {
            if (activeDeck.Count == 0)
                SetupDeck();

            yield return StartCoroutine(SpawnBlackjackCard(activeDeck[0].name, handNumber, isDealer, facedDown));
            activeDeck.Remove(activeDeck[0]);
        }

        private IEnumerator SpawnBlackjackCard(string cardName, int handNumber, bool isDealer, bool facedDown)
        {
            HandController manipulatedHandController = null;
            var blackjackCard = Instantiate(blackjackCardPref, topCardOfDeck, Quaternion.identity, blackjackHUD.transform);

            blackjackCard.transform.GetChild(0).GetComponent<Image>().sprite = SpriteLoadManager.Instance.GetBasicCardSprite(cardName);

            /*Vector3 originalScale = blackjackCard.transform.lossyScale;
            blackjackCard.transform.SetParent(blackjackCard.transform.parent.transform.parent);
            blackjackCard.transform.localScale = originalScale;*/

            var card = blackjackCard.GetComponent<BlackjackCard>();

            GameObject cardContainer = null;
            Vector2 destinationCords = new Vector2();

            if (!facedDown)
            {
                blackjackCard.GetComponent<BlackjackCard>().FlipCard();
                Debug.Log($"BlackjackManager object activeness is {this.gameObject.activeSelf}");
                yield return new WaitForSeconds(1f);
            }

            if (isDealer)
            {
                if (dealerHand.GetCardsCount() == 0)
                {
                    cardContainer = dealerHand.GetCardsContainer;
                    destinationCords = dealerHand.transform.position;
                }
                else if (dealerHand.GetCardsCount() == 1)
                {
                    var dealersAddCardsContainerOfFirstCard = dealerHand.GetHandFirstCard().GetAddCardsContainer;
                    cardContainer = dealersAddCardsContainerOfFirstCard.gameObject;

                    destinationCords = dealerHand.transform.position;
                }
                else
                {
                    var dealersAddCardsContainerOfFirstCard = dealerHand.GetHandFirstCard().GetAddCardsContainer;

                    if (dealersAddCardsContainerOfFirstCard.transform.childCount == 0)
                    {
                        cardContainer = dealersAddCardsContainerOfFirstCard.gameObject;
                        destinationCords = dealersAddCardsContainerOfFirstCard.transform.position;
                    }
                    else
                    {
                        cardContainer = dealersAddCardsContainerOfFirstCard.gameObject;
                        destinationCords = dealersAddCardsContainerOfFirstCard.transform.GetChild((dealersAddCardsContainerOfFirstCard.transform.childCount - 1)).position;
                    }
                }

                manipulatedHandController = dealerHand;
            }
            else if (!isDealer && handNumber == -1)
            {
                Debug.LogError("handNumber is not assigned");
                yield break;
            }
            else if (!isDealer && !(handNumber == -1))
            {
                var hand = playerHandsContainer.transform.GetChild(handNumber).GetComponent<HandController>();

                if (hand.GetCardsCount() == 0)
                {
                    cardContainer = hand.GetCardsContainer;
                    destinationCords = hand.transform.position;
                }
                else if (hand.GetCardsCount() == 1)
                {
                    var addCardsContainerOfFirstCard = hand.GetHandFirstCard().GetAddCardsContainer;
                    cardContainer = addCardsContainerOfFirstCard.gameObject;

                    destinationCords = hand.transform.position;
                }
                else
                {
                    var addCardsContainerOfFirstCard = hand.GetHandFirstCard().GetAddCardsContainer;

                    if (addCardsContainerOfFirstCard.transform.childCount == 0)
                    {
                        cardContainer = addCardsContainerOfFirstCard.gameObject;
                        destinationCords = addCardsContainerOfFirstCard.transform.position;
                    }
                    else
                    {
                        cardContainer = addCardsContainerOfFirstCard.gameObject;
                        destinationCords = addCardsContainerOfFirstCard.transform.GetChild((addCardsContainerOfFirstCard.transform.childCount - 1)).position;
                    }
                }

                manipulatedHandController = hand;
            }

            yield return StartCoroutine(MoveAndGiveCard(card, destinationCords, cardContainer));

            manipulatedHandController.GetHandScores();
        }

        private IEnumerator MoveAndGiveCard(BlackjackCard card, Vector2 destinationCords, GameObject cardContainer)
        {

            while (Vector2.Distance(card.transform.position, destinationCords) > 0.3f)
            {
                card.transform.position = Vector2.MoveTowards(card.transform.position, destinationCords, cardGiveSpeed * Time.deltaTime);
                yield return null;
            }

            card.transform.position = destinationCords;
            card.transform.SetParent(cardContainer.transform, true);
        }

        private void ToggleStartBtns(bool isEnable)
        {
            startBtnsContainer.SetActive(isEnable);
        }

        private void ToggleBetsBtns(bool isEnable)
        {
            betsContainer.SetActive(isEnable);
        }

        private void EnableBlackjackHUD()
        {
            blackjackHUD.gameObject.SetActive(true);
        }

        public void OnHitBtnClick() => playerAct = PlayerAct.Hit;

        public void OnStandBtnClick() => playerAct = PlayerAct.Stand;

        public void OnSplitBtnClick() => playerAct = PlayerAct.Split;

        public void OnInsuranceBtnClick() => playerAct = PlayerAct.Insurance;
        
        public void OnDoubleDownBtnClick() => playerAct = PlayerAct.Double;

        private int GetPlayerHandsCount => playerHandsContainer.transform.childCount;

        private enum PlayerAct
        {
            None = 0,
            Hit = 1,
            Stand = 2,
            Split = 3,
            Double = 4,
            Insurance = 5
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
}