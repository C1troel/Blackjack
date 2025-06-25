using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : NetworkBehaviour
{
    [SerializeField] private Canvas battleHUD;

    [SerializeField] private GameObject NextCardPref;
    [SerializeField] private GameObject NextCardAnchor;
    [SerializeField] private GameObject AtkAnchor;
    [SerializeField] private GameObject AtkAnchorDest;
    [SerializeField] private GameObject DefAnchor;
    [SerializeField] private GameObject DefAnchorDest;
    [SerializeField] private GameObject BattleAvatarPref;
    [SerializeField] private GameObject AtkHandContainer;
    [SerializeField] private GameObject DefHandContainer;
    [SerializeField] private GameObject BattleButtonsContainer;

    /// <summary>
    /// Наступні 8 змінних потрібні для тесту додавання карт до рук гравця(поки не налаштована система ефектних карт)
    /// </summary>
    [SerializeField] private GameObject TESTAddingCardButtonsAtk;
    [SerializeField] private Button TESTAdd1CardButtonAtk;
    [SerializeField] private Button TESTAdd2CardButtonAtk;
    [SerializeField] private Button TESTAdd3CardButtonAtk;

    [SerializeField] private GameObject TESTAddingCardButtonsDef;
    [SerializeField] private Button TESTAdd1CardButtonDef;
    [SerializeField] private Button TESTAdd2CardButtonDef;
    [SerializeField] private Button TESTAdd3CardButtonDef;

    [SerializeField] private TextMeshProUGUI TESTleftCardsAddingText;
    [SerializeField] private TextMeshProUGUI TESTAlreadyAddedCardsText;


    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private float appearenceSpeed;
    [SerializeField] private float cardGiveSpeed;
    [SerializeField] private float turnTime;
    [SerializeField] private float cardSpawnDelay;

    public static BattleManager Instance { get; private set; }

    private List<Sprite> cardsList = new List<Sprite>();
    private List<Sprite> activeDeck = new List<Sprite>();

    private List<GameObject> battleAvatars = new List<GameObject>();

    private Button _atackButton;
    private Button _defendButton;
    private Button _insuranceButton;
    private Button _insuranceSkippingButton;
    private Button _splitDefButton;
    private Button _splitAtkButton;
    private Button _splitSkippingButton;

    private List<Tuple<ulong,List<NextCardScript>>> playersHands = new List<Tuple<ulong, List<NextCardScript>>>(); // 0 = atk, 1 = def

    private Coroutine cardGiving;
    private Coroutine timerRunning;

    private float timer;

    private ValueTuple<int, int> playerCardAdds = new ValueTuple<int, int>(0, 0);

    private int leftCardAdds = 3;
    private int TESTalreadyCardsAdds = 0;

    private bool? atkPlayerSplitChoose = null;
    private bool? atkPlayerInsuranceChoose = null;

    #region Стандартні мережеві функції
    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        var sprites = Resources.LoadAll<Sprite>("Cards/PlayingCards");
        cardsList.AddRange(sprites);

        SetupBattleButtons();
        
        SetListenersToAddingButtons();
    }
    #endregion

    #region ServerRpc

    [ServerRpc(RequireOwnership = false)]
    private void RequestForStartDefendServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForStartDefendServerRpc");
        }

        StartCoroutine(FirstDealingCards());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForSplitServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForSplitServerRpc");
        }

        StartCoroutine(FirstDealingCards(true));
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForAtkSplitServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForAtkSplitServerRpc");
        }

        atkPlayerSplitChoose = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForAtkSplitSkipServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForAtkSplitServerRpc");
        }

        atkPlayerSplitChoose = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForStartAttackServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForStartAttackServerRpc");
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { playersHands[1].Item1 }
            }
        };

        DefenderPlayerTurnClientRpc(clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForAtkInsuranceServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForAtkInsuranceServerRpc");
        }

        atkPlayerInsuranceChoose = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestForAtkInsuranceSkipServerRpc()
    {
        if (IsServer)
        {
            Debug.LogWarning("Server Call: RequestForAtkInsuranceSkipServerRpc");
        }

        atkPlayerInsuranceChoose = false;
    }

    #endregion

    #region ClientRpc
    [ClientRpc]
    private void SetupPlayersHandsClientRpc(ulong atkId, ulong defId)
    {
        playersHands.Add(new Tuple<ulong, List<NextCardScript>>(atkId, new List<NextCardScript>()));
        playersHands.Add(new Tuple<ulong, List<NextCardScript>>(defId, new List<NextCardScript>()));
    }

    [ClientRpc]
    private void AttackerPlayerTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("AttackerPlayerTurnClientRpc");

        _atackButton.gameObject.SetActive(true);

        TESTAddingCardButtonsAtk.SetActive(true);

        timerRunning = StartCoroutine(Starttimer(Activity.atkTurn));
    }

    [ClientRpc]
    private void DefenderPlayerTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("DefenderPlayerTurnClientRpc");

        #region TestAddingCards
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        #endregion

        _defendButton.gameObject.SetActive(true);
        _splitDefButton.gameObject.SetActive(true);

        TESTAddingCardButtonsDef.SetActive(true);

        timerRunning = StartCoroutine(Starttimer(Activity.defTurn));
    }

    [ClientRpc]
    private void AttackButtonTurnClientRpc(bool isTurningOn = true, ClientRpcParams clientRpcParams = default)
    {
        _atackButton.gameObject.SetActive(isTurningOn);
    }

    [ClientRpc]
    private void DefendButtonTurnClientRpc(bool isTurningOn = true, ClientRpcParams clientRpcParams = default)
    {
        _defendButton.gameObject.SetActive(isTurningOn);
    }

    [ClientRpc]
    private void InsuranceButtonTurnClientRpc(bool isTurningOn = true, ClientRpcParams clientRpcParams = default)
    {
        _insuranceButton.gameObject.SetActive(isTurningOn);
    }

    [ClientRpc]
    private void AtkSplitButtonsTurnClientRpc(bool isTurningOn = true, ClientRpcParams clientRpcParams = default)
    {
        _splitSkippingButton.gameObject.SetActive(isTurningOn);
        _splitAtkButton.gameObject.SetActive(isTurningOn);

        timerRunning = StartCoroutine(Starttimer(Activity.split));
    }

    [ClientRpc]
    private void AtkInsuranceButtonsTurnClientRpc(bool isTurningOn = true, ClientRpcParams clientRpcParams = default)
    {
        _insuranceButton.gameObject.SetActive(isTurningOn);
        _insuranceSkippingButton.gameObject.SetActive(isTurningOn);

        timerRunning = StartCoroutine(Starttimer(Activity.insurance));
    }

    [ClientRpc]
    private void SpawnNextCardClientRpc(string cardName, int handNumber = -1, bool isAttacker = true, bool facedDown = false)
    {
        var nextCard = Instantiate(NextCardPref, NextCardAnchor.transform);

        nextCard.transform.GetChild(0).GetComponent<Image>().sprite = SpriteLoadManager.Instance.GetBasicCardSprite(cardName);

        Vector3 originalScale = nextCard.transform.lossyScale;
        nextCard.transform.SetParent(nextCard.transform.parent.transform.parent);
        nextCard.transform.localScale = originalScale;

        var card = nextCard.GetComponent<NextCardScript>();

        GameObject cardContainer = null;
        Vector2 destinationCords = new Vector2();

        if (!facedDown)
            nextCard.GetComponent<NextCardScript>().FlipCard();

        if (isAttacker && handNumber == -1)
        {
            card.handNumber = handNumber;
            playersHands[0].Item2.Add(card); // 0 гравець - атакуючий
            cardContainer = AtkHandContainer;
            destinationCords = AtkHandContainer.transform.position;
        }
        else if (!isAttacker && handNumber == -1)
        {
            card.handNumber = handNumber;
            playersHands[1].Item2.Add(card); // 1 гравець - захисник
            cardContainer = DefHandContainer;
            destinationCords = DefHandContainer.transform.position;
        }
        else if (isAttacker && !(handNumber == -1))
        {
            card.isAppended = true;
            card.handNumber = handNumber;
            playersHands[0].Item2.Add(card);
            var hands = playersHands[0].Item2.FindAll(card => card.isAppended == false);

            cardContainer = hands[handNumber].gameObject.transform
                .Cast<Transform>()
                .Select(t => t.gameObject)
                .FirstOrDefault(container => container.name.EndsWith("ToRight"));
            cardContainer.SetActive(true);

            destinationCords = AtkHandContainer.transform.position;
        }
        else if (!isAttacker && !(handNumber == -1))
        {
            card.isAppended = true;
            card.handNumber = handNumber;
            playersHands[1].Item2.Add(card);
            var hands = playersHands[1].Item2.FindAll(card => card.isAppended == false);

            cardContainer = hands[handNumber].gameObject.transform
                .Cast<Transform>()
                .Select(t => t.gameObject)
                .FirstOrDefault(container => container.name.EndsWith("ToLeft"));
            cardContainer.SetActive(true);
            destinationCords = DefHandContainer.transform.position;
        }

        StartCoroutine(WaitForGiveCard(card, destinationCords, cardContainer));
    }

    [ClientRpc]
    private void SpawnBattleAvatarsClientRpc(string atkName, string defName)
    {
        var atkBattleAvatar = Instantiate(BattleAvatarPref, AtkAnchor.transform.position, Quaternion.identity, battleHUD.transform);
        atkBattleAvatar.GetComponent<Animator>().runtimeAnimatorController = SpriteLoadManager.Instance.GetAnimatorController(atkName);

        StartCoroutine(MoveTowardsTarget(atkBattleAvatar.transform, AtkAnchorDest.transform.position, appearenceSpeed));

        var defBattleAvatar = Instantiate(BattleAvatarPref, DefAnchor.transform.position, Quaternion.identity, battleHUD.transform);
        defBattleAvatar.GetComponent<Animator>().runtimeAnimatorController = SpriteLoadManager.Instance.GetAnimatorController(defName);

        battleAvatars.Add(defBattleAvatar);
        battleAvatars.Add(atkBattleAvatar);

        StartCoroutine(MoveTowardsTarget(defBattleAvatar.transform, DefAnchorDest.transform.position, appearenceSpeed));
    }

    [ClientRpc]
    private void CallBattleHUDClientRpc()
    {
        battleHUD.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void DisableBattleHUDClientRpc()
    {
        battleHUD.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void DeleteAllRestBattleHUDObjectClientRpc()
    {
        Debug.Log("DeleteAllRestBattleHUDObjectClientRpc");

        foreach (Transform card in AtkHandContainer.transform)
            Destroy(card.gameObject);

        foreach (Transform card in DefHandContainer.transform)
            Destroy(card.gameObject);

        foreach (var avatar in battleAvatars)
            Destroy(avatar);

        foreach (var card in playersHands[0].Item2)
            Destroy(card.gameObject);

        foreach (var card in playersHands[1].Item2)
            Destroy(card.gameObject);

        if (IsServer)
            playerCardAdds = new ValueTuple<int, int>(0, 0);

        playersHands.Clear();

        if (cardGiving != null)
        {
            StopCoroutine(cardGiving);
            cardGiving = null;
        }

        activeDeck.Clear();
        battleAvatars.Clear();
        atkPlayerSplitChoose = null;
        atkPlayerInsuranceChoose = null;
        ResetAddingCards();
    }

    [ClientRpc]
    private void RevealFacedDownCardClientRpc() => playersHands[1].Item2.Find(card => card.isFacedDown).FlipCard();    
    
    [ClientRpc]
    private void CardSplittingClientRpc(bool isAtk = true)
    {
        if (isAtk)
        {
            var addCard = playersHands[0].Item2.Find(card => card.isAppended == true);

            addCard.isAppended = false;
            addCard.handNumber = -1;
            addCard.transform.SetParent(AtkHandContainer.transform);
        }
    }

    #endregion

    #region Корутины
    private IEnumerator FirstDealingCards(bool isDefSplitting = false)
    {
        SpawnNextCard(-1, true, false); // сначала 1 карту для атакующего
        yield return new WaitForSeconds(cardSpawnDelay);

        SpawnNextCard(-1, false, false); // 1 карту для защитника
        yield return new WaitForSeconds(cardSpawnDelay);

        SpawnNextCard(0, true, false); // 1 карту для атакующего
        yield return new WaitForSeconds(cardSpawnDelay);

        SpawnNextCard(0, false, true); // 1 карту перевернутую для защитника
        yield return new WaitForSeconds(cardSpawnDelay);

        while (cardGiving != null)
            yield return null;

        StartCoroutine(ContinueBattle(isDefSplitting));
    }

    private IEnumerator WaitForGiveCard(NextCardScript card, Vector2 destinationCords, GameObject cardContainer)
    {
        while (cardGiving != null)
        {
            yield return null;
        }
        cardGiving = StartCoroutine(MoveAndGiveCard(card, destinationCords, cardContainer));
    }

    private IEnumerator MoveAndGiveCard(NextCardScript card, Vector2 destinationCords, GameObject cardContainer)
    {

        while (Vector2.Distance(card.transform.position, destinationCords) > 0.3f)
        {
            card.transform.position = Vector2.Lerp(card.transform.position, destinationCords, cardGiveSpeed * Time.deltaTime);
            yield return null;
        }

        card.transform.position = destinationCords;
        card.transform.SetParent(cardContainer.transform, true);
        cardGiving = null;
    }

    private IEnumerator MoveTowardsTarget(Transform objTransform, Vector3 targetPosition, float speed)
    {
        while (Vector2.Distance(objTransform.position, targetPosition) > 0.1f)
        {
            objTransform.position = Vector2.Lerp(objTransform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        objTransform.position = targetPosition;
    }

    private IEnumerator ContinueBattle(bool isDefSplitting = false)
    {
        if (GetScoreFromString(playersHands[1].Item2[0].gameObject.transform
            .Find("1Side").GetComponent<Image>().sprite.name) == 11)

        {
            yield return StartCoroutine(AtkInsuranceHandler());
        }

        RevealFacedDownCardClientRpc();

        yield return new WaitForSeconds(2);

        yield return StartCoroutine(SummarizeAndDealDamage(isDefSplitting));

        DisableBattleHUDClientRpc();

        TestPlayerSpawner.Instance.TurnPlayersHUDClientRpc(true);
        TestPlayerSpawner.Instance.GetPlayerWithId(playersHands[0].Item1).StartMove();

        DeleteAllRestBattleHUDObjectClientRpc();
    }

    private IEnumerator AtkSplitHandler()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { playersHands[0].Item1 }
            }
        };

        AtkSplitButtonsTurnClientRpc(true, clientRpcParams);

        while (atkPlayerSplitChoose == null)
        {
            yield return null;
        }
    }

    private IEnumerator Starttimer(Activity act, int actTimer = 0)
    {
        timer = turnTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = ((int)timer).ToString();
            yield return null;
        }

        timerText.text = "";

        switch (act)
        {
            case Activity.atkTurn:
                OnAttackButtonClick();
                break;

            case Activity.defTurn:
                OnDefendButtonClick();
                break;

            case Activity.insurance:
                OnInsuranceSkippingButtonClick();
                break;

            case Activity.split:
                OnSplitSkippingButtonClick();
                break;

            default:
                timerRunning = null;
                yield break;
        }

        timerRunning = null;
        yield break;
    }

    private IEnumerator AtkInsuranceHandler()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { playersHands[0].Item1 }
            }
        };

        AtkInsuranceButtonsTurnClientRpc(true, clientRpcParams);

        while (atkPlayerInsuranceChoose == null)
        {
            yield return null;
        }
    }

    private IEnumerator SummarizeAndDealDamage(bool isDefSplitting = false)
    {
        int totalAtkScore = 0;
        int totalDefScore = 0;

        bool isAtkBlackJack = false;
        bool isDefBlackJack = false;

        bool isEvade = false;

        for (int i = 0; i < playersHands[0].Item2.Count; i++)
        {
            totalAtkScore += GetScoreFromString(playersHands[0].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

            if (totalAtkScore == 21 && i == 1)
            {
                isAtkBlackJack = true;
                break;
            }

            if (i == 1)
            {
                if (CheckForSplit(playersHands[0].Item2[i], playersHands[0].Item2[i - 1]) && atkPlayerInsuranceChoose != true)
                {
                    yield return StartCoroutine(AtkSplitHandler());
                    break;
                }
            }
        }

        for (int i = 0; i < playersHands[1].Item2.Count; i++)
        {
            totalDefScore += GetScoreFromString(playersHands[1].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

            if (totalDefScore == 21 && i == 1)
            {
                isDefBlackJack = true;
                break;
            }

            if (i == 1 && isDefSplitting)
                isEvade = CheckForSplit(playersHands[1].Item2[i], playersHands[1].Item2[i - 1]);
        }

        Debug.Log($"totalDefScore: {totalDefScore}");

        var atkPlayer = TestPlayerSpawner.Instance.GetPlayerWithId(playersHands[0].Item1);
        var defPlayer = TestPlayerSpawner.Instance.GetPlayerWithId(playersHands[1].Item1);

        if (isEvade && isDefSplitting) // Можлива ще якась логіка по типу анімації
        {
            Debug.Log($"Split!!!");
            yield break;
        }

        if (atkPlayerSplitChoose == true)
        {
            atkPlayerSplitChoose = null;
            Debug.Log($"ATKSplit!!!");
            CardSplittingClientRpc();
            yield return StartCoroutine(SplitController(isDefSplitting));
            yield return new WaitForSeconds(3);
            yield break;
        }
        else if (atkPlayerSplitChoose == false)
            Debug.Log($"SkipATKSplit!!!");

        if (!isAtkBlackJack && (playerCardAdds.Item1 != 0))
        {
            yield return StartCoroutine(SpawnLeftCards(true));
            totalAtkScore = SummarizeHandDamage(playersHands[0].Item2);
        }
        if (!isDefBlackJack && (playerCardAdds.Item2 != 0))
        {
            yield return StartCoroutine(SpawnLeftCards(false));
            totalDefScore = SummarizeHandDamage(playersHands[1].Item2);
        }

        if (atkPlayerInsuranceChoose == true) // якщо гравець атаки застрахувався то він отримує негайно половину неблокуючих пошкоджень по собі
        {
            Debug.Log($"Insurance!!!");
            TestPlayerSpawner.Instance.DealDamage(atkPlayer, totalAtkScore/2);
            yield break;
        }

        var totalDamage = totalAtkScore == totalDefScore ? 5 :
            (totalAtkScore + atkPlayer.GetPlayerAtk + 10) -
            (isDefSplitting ? 0 : (totalDefScore + defPlayer.GetPlayerDef));

        Debug.Log($"totalAtkScore: {totalAtkScore}");
        Debug.Log($"totalDefScore: {totalDefScore}");
        Debug.Log($"TotalDamage: {totalDamage}");

        if (isAtkBlackJack && isDefBlackJack) // Пуш = 5 пошкоджень
        {
            TestPlayerSpawner.Instance.DealDamage(defPlayer, 5);
            Debug.Log($"Push!!!");
            yield break;
        }
        else if (isAtkBlackJack) // коли блекджек у бійця атаки то він наносить подвоєні пошкодження захиснику та блокуючі пошкодження іншому випадковому гравцю
        {
            TestPlayerSpawner.Instance.DealDamage(defPlayer, totalDamage * 2);
            var anotherPlayer = TestPlayerSpawner.Instance.GetRandomPlayerExcept(atkPlayer.GetPlayerId, defPlayer.GetPlayerId);

            TestPlayerSpawner.Instance.DealDamage(anotherPlayer, totalDamage, true);
            Debug.Log($"isAtkBlackJack!!!");
            yield break;
        }
        else if (isDefBlackJack) // коли блекджек у бійця захисту то він відбиває всі пошкодження атакуючого назад та виліковується на половину цих пошкоджень
        {
            TestPlayerSpawner.Instance.DealDamage(atkPlayer, totalDamage);
            TestPlayerSpawner.Instance.Heal(defPlayer, totalDamage / 2);
            Debug.Log($"isDefBlackJack!!!");
            yield break;
        }

        TestPlayerSpawner.Instance.DealDamage(defPlayer, totalDamage);
    }

    private IEnumerator SpawnLeftCards(bool isForAtk)
    {
        if (isForAtk)
        {
            while (playerCardAdds.Item1 > 0)
            {
                SpawnNextCard(0, true, false);
                --playerCardAdds.Item1;

                while (cardGiving != null)
                    yield return null;
            }
        }
        else if (!isForAtk)
        {
            while (playerCardAdds.Item2 > 0)
            {
                SpawnNextCard(0, false, false);
                --playerCardAdds.Item2;

                while (cardGiving != null)
                    yield return null;
            }
        }
    }

    private IEnumerator SplitController(bool isDefSplitting = false)
    {
        if (!IsServer)
            yield break;

        var hands = playersHands[0].Item2.FindAll(card => card.isAppended == false);
        List<ulong> alreadyAttackedPlayers = new List<ulong>();

        int totalAtkScore = 0;
        int totalDefScore = 0;

        bool isDefBlackJack = false;
        bool isCanSplit = true;

        for (int i = 0; i < playersHands[1].Item2.Count; i++) // додати підрахунок додаткових карт
        {
            totalDefScore += GetScoreFromString(playersHands[1].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

            if (totalDefScore == 21 && i == 1)
            {
                isDefBlackJack = true;
                break;
            }
        }

        for (int i = 0; i < playerCardAdds.Item1; i++)
        {
            if (i == 0)
            {
                SpawnNextCard(0, true, false);
                yield return new WaitForSeconds(1);

                while (cardGiving != null)
                    yield return null;
            }

            if (i == 0 && isCanSplit)
            {
                if (CheckForSplit(playersHands[0].Item2[0], playersHands[0].Item2[playersHands[0].Item2.Count - 1]))
                {
                    yield return StartCoroutine(AtkSplitHandler());

                    if (atkPlayerSplitChoose == true)
                    {
                        Debug.Log($"SecondATKSplit!!!");
                        atkPlayerSplitChoose = null;
                        CardSplittingClientRpc();
                        yield return new WaitForSeconds(2);

                        isCanSplit = false;
                        continue;
                    }
                }
            }

            hands = playersHands[0].Item2.FindAll(card => card.isAppended == false);

            for (int j = 0; j < hands.Count; j++)
            {
                for (int k = i; k < playerCardAdds.Item1; k++)
                {
                    if (isCanSplit && j == 0 && k == i)
                        continue;

                    SpawnNextCard(j, true, false);
                    yield return new WaitForSeconds(1);

                    while (cardGiving != null)
                        yield return null;
                }
            }

            break;
        }

        playerCardAdds.Item1 = 0;

        // ! додати підрахунок додаткових карт атакуючому
        for (int i = 0; i < hands.Count; i++) // підрахунок усього дамагу гравця атаки 
        {
            totalAtkScore += GetScoreFromString(hands[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

            var additionalCards = playersHands[0].Item2.FindAll(card => card.handNumber == i);

            if (additionalCards.Count > 0)
            {
                foreach (var card in additionalCards)
                {
                    totalAtkScore += GetScoreFromString(card.gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);
                }
            }

            if (i == 0)
            {
                var defPlayer = TestPlayerSpawner.Instance.GetPlayerWithId(playersHands[1].Item1);
                var atkPlayer = TestPlayerSpawner.Instance.GetPlayerWithId(playersHands[0].Item1);

                if (!isDefBlackJack && (playerCardAdds.Item2 != 0))
                {
                    yield return StartCoroutine(SpawnLeftCards(false));
                    totalDefScore = SummarizeHandDamage(playersHands[1].Item2);
                }

                var totalDamage = totalAtkScore == totalDefScore ? 5 :
                (totalAtkScore + atkPlayer.GetPlayerAtk + 10) -
                (isDefSplitting ? 0 : (totalDefScore + defPlayer.GetPlayerDef));

                if (isDefBlackJack)
                {
                    TestPlayerSpawner.Instance.DealDamage(atkPlayer, totalDamage);
                    TestPlayerSpawner.Instance.Heal(defPlayer, totalDamage / 2);
                    Debug.Log($"AfterSplitIsDefBlackJack!!!");
                    continue;
                }

                TestPlayerSpawner.Instance.DealDamage(defPlayer, totalDamage);
                Debug.Log($"Split damage to playerId: {defPlayer.GetPlayerId}; Dmg: {totalDamage}");

                alreadyAttackedPlayers.Add(atkPlayer.GetPlayerId);
                alreadyAttackedPlayers.Add(defPlayer.GetPlayerId);

                totalAtkScore = 0;
                continue;
            }

            var anotherPlayer = TestPlayerSpawner.Instance.GetRandomPlayerExcept(alreadyAttackedPlayers);

            if (anotherPlayer == null)
                yield break;

            TestPlayerSpawner.Instance.DealDamage(anotherPlayer, totalAtkScore, true);
            Debug.Log($"Split damage to playerId: {anotherPlayer.GetPlayerId}; Dmg: {totalAtkScore}");
            alreadyAttackedPlayers.Add(anotherPlayer.GetPlayerId);

            totalAtkScore = 0;
        }
    }

    #endregion

    #region Звичайні функції

    private int SummarizeHandDamage(List<NextCardScript> playerHand)
    {
        int handDamage = 0;

        foreach (var card in playerHand)
        {
            handDamage += GetScoreFromString(card.gameObject.transform
                .Find("1Side").GetComponent<Image>().sprite.name);
        }

        return handDamage;
    }

    private bool CheckForSplit(NextCardScript card1, NextCardScript card2)
    {
        string card1Name = card1.gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name; // Назва спрайту на лицевій стороні карти1
        int card1Score = int.Parse(card1Name.Substring(card1Name.Length - 2));

        string card2Name = card2.gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name; // Назва спрайту на лицевій стороні карти2
        int card2Score = int.Parse(card2Name.Substring(card2Name.Length - 2));

        return card1Score == card2Score;
    }

    private int GetScoreFromString(string str)
    {
        int score = int.Parse(str.Substring(str.Length - 2));

        return score switch
        {
            1 => 11,
            11 => 10,
            12 => 10,
            13 => 10,
            _ => score,
        };
    }

    public void StartBattle(PlayerController atk, PlayerController def)
    {
        if ((!CanAttack(atk)) || (!CanAttack(def)))
            return;

        SetupPlayersHandsClientRpc(atk.GetPlayerId, def.GetPlayerId);

        TestPlayerSpawner.Instance.TurnPlayersHUDClientRpc(false);

        CallBattleHUDClientRpc();

        SetupDeck();

        string atkName = atk.GetPlayerInfo().contorllerName;
        string defName = def.GetPlayerInfo().contorllerName;

        SpawnBattleAvatarsClientRpc(atkName, defName);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { atk.GetPlayerId }
            }
        };

        AttackerPlayerTurnClientRpc(clientRpcParams);

        // Видать сначала обом игрокам по 2 карти Диллер(Защитник)/Атакер

    }

    private void SetupBattleButtons()
    {
        foreach (Transform button in BattleButtonsContainer.transform)
        {
            switch (button.name)
            {
                case "Attack":
                    _atackButton = button.GetComponent<Button>();
                    _atackButton.onClick.AddListener(OnAttackButtonClick);
                    break;

                case "Defend":
                    _defendButton = button.GetComponent<Button>();
                    _defendButton.onClick.AddListener(OnDefendButtonClick);
                    break;

                case "Insurance":
                    _insuranceButton = button.GetComponent<Button>();
                    _insuranceButton.onClick.AddListener(OnInsuranceButtonClick);
                    break;

                case "SplitDef":
                    _splitDefButton = button.GetComponent<Button>();
                    _splitDefButton.onClick.AddListener(OnSplitDefButtonClick);
                    break;

                case "Split":
                    _splitAtkButton = button.GetComponent<Button>();
                    _splitAtkButton.onClick.AddListener(OnSplitAtkButtonClick);
                    break;

                case "Skip":
                    _splitSkippingButton = button.GetComponent<Button>();
                    _splitSkippingButton.onClick.AddListener(OnSplitSkippingButtonClick);
                    break;

                case "InsuranceSkip":
                    _insuranceSkippingButton = button.GetComponent<Button>();
                    _insuranceSkippingButton.onClick.AddListener(OnInsuranceSkippingButtonClick);
                    break;

                default:
                    break;
            }
        }
    }

    private void OnAttackButtonClick()
    {
        _atackButton.gameObject.SetActive(false);
        Debug.Log("OnAttackButtonClick");
        RequestForStartAttackServerRpc();

        TurnCardAddingButtons(false);
        ResetAddingCardsTextBoxes(true);

        StopTimer();
    }

    private void OnDefendButtonClick()
    {
        _defendButton.gameObject.SetActive(false);
        _splitDefButton.gameObject.SetActive(false);
        Debug.Log("OnDefendButtonClick");
        RequestForStartDefendServerRpc();

        TurnCardAddingButtons(false);
        ResetAddingCardsTextBoxes(false);

        StopTimer();
    }

    private void OnInsuranceButtonClick()
    {
        _insuranceButton.gameObject.SetActive(false);
        _insuranceSkippingButton.gameObject.SetActive(false);

        Debug.Log("OnInsuranceButtonClick");

        RequestForAtkInsuranceServerRpc();

        StopTimer();
    }

    private void OnSplitDefButtonClick()
    {
        _defendButton.gameObject.SetActive(false);
        _splitDefButton.gameObject.SetActive(false);

        Debug.Log("OnSplitDefButtonClick");
        RequestForSplitServerRpc();

        StopTimer();
    }

    private void OnSplitAtkButtonClick()
    {
        _splitAtkButton.gameObject.SetActive(false);
        _splitSkippingButton.gameObject.SetActive(false);

        Debug.Log("OnSplitAtkButtonClick");

        RequestForAtkSplitServerRpc();

        StopTimer();
    }

    private void OnSplitSkippingButtonClick()
    {
        _splitAtkButton.gameObject.SetActive(false);
        _splitSkippingButton.gameObject.SetActive(false);

        Debug.Log("OnSplitSkippingButtonClick");

        RequestForAtkSplitSkipServerRpc();

        StopTimer();
    }

    private void OnInsuranceSkippingButtonClick()
    {
        _insuranceButton.gameObject.SetActive(false);
        _insuranceSkippingButton.gameObject.SetActive(false);

        Debug.Log("OnInsuranceSkippingButtonClick");

        RequestForAtkInsuranceSkipServerRpc();

        StopTimer();
    }

    private void StopTimer()
    {
        if (timerRunning == null)
            return;

        StopCoroutine(timerRunning);
        timerText.text = "";
        timerRunning = null;
    }

    private void ActivateButtons(bool isAttacker , ulong playerId, bool isPreparation = true)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { playerId }
            }
        };

        if (isPreparation)
            if (isAttacker)
                AttackButtonTurnClientRpc(true, clientRpcParams);
            else
                DefendButtonTurnClientRpc(true, clientRpcParams);
        else
            InsuranceButtonTurnClientRpc(true, clientRpcParams);
    }

    private void SpawnNextCard(int handNumber = -1, bool isAttacker = true, bool facedDown = false) // 0 = карта не додається до вже готової руки(максимум 4)
    {
        if (activeDeck.Count == 0)
            SetupDeck();

        SpawnNextCardClientRpc(activeDeck[0].name, handNumber, isAttacker, facedDown);
        activeDeck.Remove(activeDeck[0]);
    }

    private void SetupDeck()
    {
        activeDeck.AddRange(cardsList);

        System.Random random = new System.Random();
        activeDeck = activeDeck.OrderBy(x => random.Next()).ToList();
    }

    private bool CanAttack(PlayerController player)
    {
        return true;
    }

    #endregion

    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public enum Activity
    {
        atkTurn,
        defTurn,
        insurance,
        split
    }

    #region TESTAddingCards
    private void SetListenersToAddingButtons()
    {
        TESTAdd1CardButtonAtk.onClick.AddListener(OnAdd1CardButtonAtk);
        TESTAdd2CardButtonAtk.onClick.AddListener(OnAdd2CardButtonAtk);
        TESTAdd3CardButtonAtk.onClick.AddListener(OnAdd3CardButtonAtk);

        TESTAdd1CardButtonDef.onClick.AddListener(OnAdd1CardButtonDef);
        TESTAdd2CardButtonDef.onClick.AddListener(OnAdd2CardButtonDef);
        TESTAdd3CardButtonDef.onClick.AddListener(OnAdd3CardButtonDef);

    }

    private void TurnCardAddingButtons(bool isTurnOn)
    {
        TESTAddingCardButtonsAtk.SetActive(isTurnOn);
        TESTAddingCardButtonsDef.SetActive(isTurnOn);
    }

    private void ResetAddingCardsTextBoxes(bool isAtk)
    {
        TESTAlreadyAddedCardsText.text = string.Empty;
        TESTleftCardsAddingText.text = string.Empty;

        if (TESTalreadyCardsAdds == 0)
            return;

        if (isAtk)
            TESTSendAtkAddCardsServerRpc(TESTalreadyCardsAdds);
        else
            TESTSendDefAddCardsServerRpc(TESTalreadyCardsAdds);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TESTSendAtkAddCardsServerRpc(int addCardsAmount)
    {
        playerCardAdds.Item1 = addCardsAmount;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TESTSendDefAddCardsServerRpc(int addCardsAmount)
    {
        playerCardAdds.Item2 = addCardsAmount;
    }

    private void ResetAddingCards()
    {
        leftCardAdds = 3;
        TESTalreadyCardsAdds = 0;
    }

    private void OnAdd1CardButtonAtk()
    {
        if (leftCardAdds == 0)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 1;
        ++TESTalreadyCardsAdds;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }
    
    private void OnAdd2CardButtonAtk()
    {
        if (leftCardAdds < 2)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 2;
        TESTalreadyCardsAdds += 2;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }
    
    private void OnAdd3CardButtonAtk()
    {
        if (leftCardAdds < 3)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 3;
        TESTalreadyCardsAdds += 3;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }
    
    private void OnAdd1CardButtonDef()
    {
        if (leftCardAdds == 0)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 1;
        ++TESTalreadyCardsAdds;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }
    
    private void OnAdd2CardButtonDef()
    {
        if (leftCardAdds < 2)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 2;
        TESTalreadyCardsAdds += 2;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }

    private void OnAdd3CardButtonDef()
    {
        if (leftCardAdds < 3)
        {
            Debug.Log("AlreadyMaxAddingCards!!!");
            return;
        }

        leftCardAdds -= 3;
        TESTalreadyCardsAdds += 3;

        TESTleftCardsAddingText.text = leftCardAdds.ToString();
        TESTAlreadyAddedCardsText.text = TESTalreadyCardsAdds.ToString();
    }

    #endregion
}
