using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private Canvas battleHUD;
        [SerializeField] private GameObject battleConfirmUI;

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

        [SerializeField] private BattlePlayerEffectCardsHandler battleEffectCardsHandler;
        [SerializeField] private BattleEffectCardApplier battleEffectCardsApplier;

        [SerializeField] private TextMeshProUGUI TESTleftCardsAddingText;
        [SerializeField] private TextMeshProUGUI TESTAlreadyAddedCardsText;

        [SerializeField] private TextMeshProUGUI timerText;

        [SerializeField] private float appearenceSpeed;
        [SerializeField] private float cardGiveSpeed;
        [SerializeField] private float turnTime;
        [SerializeField] private float cardSpawnDelay;

        private List<Sprite> activeDeck = new List<Sprite>();

        private List<GameObject> battleAvatars = new List<GameObject>();

        private Button _atackButton;
        private Button _defendButton;
        private Button _insuranceButton;
        private Button _insuranceSkippingButton;
        private Button _splitDefButton;
        private Button _splitAtkButton;
        private Button _splitSkippingButton;

        private List<Tuple<IEntity, List<NextCardScript>>> entitiesHands = new List<Tuple<IEntity, List<NextCardScript>>>(); // 0 = atk, 1 = def

        private Coroutine cardGiving;
        private Coroutine timerRunning;

        private ValueTuple<int, int> entitiesCardAdds = new ValueTuple<int, int>(0, 0); // Item1 = atk, Item2 = def

        /*private int leftCardAdds = 3;*/
        /*private int alreadyAddedCardsCount = 0;*/

        private bool? atkPlayerSplitChoose = null;
        private bool? atkPlayerInsuranceChoose = null;

        public bool IsBattleActive { get; private set; } = false;

        public delegate void EntityDrawCardDelegate(NextCardScript takenCard, IEntity entityInit);
        public EntityDrawCardDelegate OnEntityDrawCard;

        public static BattleManager Instance { get; private set; }

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
            SetupBattleButtons();

            /*SetListenersToAddingButtons();*/
        }

        public void TryToStartBattle(IEntity atk, IEntity def)
        {
            if ((!CanAttack(atk)) || (!CanAttack(def)))
                return;

            IsBattleActive = true;

            GameManager.Instance.TogglePlayersHUD(false);

            CallBattleHUD();

            SetupHands(atk, def);

            if (atk.GetEntityType != EntityType.Player)
            {
                StartBattle(atk, def);
                return;
            }

            battleConfirmUI.SetActive(true);
        }

        public void OnStartBattleClick()
        {
            battleConfirmUI.SetActive(false);
            StartBattle(entitiesHands[0].Item1, entitiesHands[1].Item1);
        }

        public void OnSkipBattleClick()
        {
            EndBattle();
        }

        private void StartBattle(IEntity atk, IEntity def)
        {
            SetupDeck();

            string atkName = ((MonoBehaviour)atk).GetComponent<Animator>().runtimeAnimatorController.name;
            string defName = ((MonoBehaviour)def).GetComponent<Animator>().runtimeAnimatorController.name;

            SpawnBattleAvatars(atkName, defName);

            if (atk.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown))
                entitiesCardAdds.Item1 += 1;

            AttackerPlayerTurn(atk);

        }

        private void AttackerPlayerTurn(IEntity entity)
        {
            Debug.Log("AttackerEntityTurn");

            switch (entity.GetEntityType)
            {
                case EntityType.Player:
                    AllowAtkForPlayer(entity);
                    break;

                case EntityType.Enemy:
                    AtkForEnemy(entity);
                    break;

                case EntityType.Ally:
                    // код для реалізації атаки союзників(міньйонів), якщо вони будуть
                    break;

                default:
                    break;
            }
        }

        private void AllowAtkForPlayer(IEntity entity)
        {
            var player = entity as BasePlayerController;
            _atackButton.gameObject.SetActive(true);

            if (entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown))
                return;

            battleEffectCardsApplier.ToggleBattleCardApplier(true);
            battleEffectCardsHandler.ShowPlayerBattleEffectCards(player, true);

            /*TESTAddingCardButtonsAtk.SetActive(true);*/
        }

        private void OnAttackButtonClick()
        {
            _atackButton.gameObject.SetActive(false);
            Debug.Log("OnAttackButtonClick");
            StartAttack();

            battleEffectCardsApplier.ToggleBattleCardApplier(false);
            battleEffectCardsHandler.HideAndReturnPlayerBattleEffectCards();
            /*TurnCardAddingButtons(false);
            ResetAddingCardsTextBoxes(true);*/
        }

        private void AtkForEnemy(IEntity entity) // метод для реалізації атаки, противником
        {
            var enemy = entity as BaseEnemy;

            if (entity.GetCurrentPanel.GetEffectPanelInfo.Effect != PanelEffect.VIPClub &&
                    !entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown))
            {
                enemy.EnemyEffectCardsHandler.UseAllCardsByPurpose(EffectCardPurpose.BattleAttack);
            }

            StartAttack();
        }

        private void StartAttack()
        {
            DefenderPlayerTurn();
        }

        private void DefenderPlayerTurn()
        {
            Debug.Log("DefenderPlayerTurn");
            var defendingEntity = entitiesHands[1].Item1;

            switch (defendingEntity.GetEntityType)
            {
                case EntityType.Player:
                    AllowDefForPlayer(defendingEntity);
                    break;

                case EntityType.Enemy:
                    DefForEnemy(defendingEntity);
                    break;

                case EntityType.Ally:
                    // код для реалізації атаки союзників(міньйонів), якщо вони будуть
                    break;

                default:
                    break;
            }
        }

        private void AllowDefForPlayer(IEntity entity)
        {
            var player = entity as BasePlayerController;
            /*#region TestAddingCards
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            #endregion*/

            if (IsFrozenDuringTimeStop(player))
            {
                StartDefend();
                return;
            }

            _defendButton.gameObject.SetActive(true);
            _splitDefButton.gameObject.SetActive(true);

            if (entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown))
                return;

            battleEffectCardsApplier.ToggleBattleCardApplier(true);
            battleEffectCardsHandler.ShowPlayerBattleEffectCards(player, false);
            /*TESTAddingCardButtonsDef.SetActive(true);*/
        }

        private void OnDefendButtonClick()
        {
            _defendButton.gameObject.SetActive(false);
            _splitDefButton.gameObject.SetActive(false);
            Debug.Log("OnDefendButtonClick");
            StartDefend();

            battleEffectCardsApplier.ToggleBattleCardApplier(false);
            battleEffectCardsHandler.HideAndReturnPlayerBattleEffectCards();
            /*TurnCardAddingButtons(false);
            ResetAddingCardsTextBoxes(false);*/
        }

        private void OnSplitDefButtonClick()
        {
            _defendButton.gameObject.SetActive(false);
            _splitDefButton.gameObject.SetActive(false);

            Debug.Log("OnSplitDefButtonClick");
            Split();

            battleEffectCardsApplier.ToggleBattleCardApplier(false);
            battleEffectCardsHandler.HideAndReturnPlayerBattleEffectCards();
        }

        private void DefForEnemy(IEntity entity)
        {
            var enemy = entity as BaseEnemy;

            if (IsFrozenDuringTimeStop(enemy))
            {
                StartDefend();
                return;
            }

            // також можливо додати шанс на вибір спліта / не шанс а перевірка на те, що він і так програє
            // також інші обробки, наприклад ефектів

            if (entity.GetCurrentPanel.GetEffectPanelInfo.Effect != PanelEffect.VIPClub &&
                    !entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown))
            {
                enemy.EnemyEffectCardsHandler.UseAllCardsByPurpose(EffectCardPurpose.BattleDefense);
            }

            StartDefend();
        }

        private void StartDefend()
        {
            StartCoroutine(FirstDealingCards());
        }

        private void Split()
        {
            StartCoroutine(FirstDealingCards(true));
        }

        private IEnumerator FirstDealingCards(bool isDefSplitting = false)
        {
            var defender = entitiesHands[1].Item1;

            SpawnNextCard(-1, true, false); // сначала 1 карту для атакуючого
            yield return new WaitForSeconds(cardSpawnDelay);

            if (!IsFrozenDuringTimeStop(defender))
            {
                SpawnNextCard(-1, false, false); // 1 карту для захисника
                yield return new WaitForSeconds(cardSpawnDelay);
            }

            SpawnNextCard(0, true, false); // 1 карту для атакуючого
            yield return new WaitForSeconds(cardSpawnDelay);

            if (!IsFrozenDuringTimeStop(defender))
            {
                SpawnNextCard(0, false, true); // 1 карту перевернуту для захисника
                yield return new WaitForSeconds(cardSpawnDelay);
            }

            while (cardGiving != null)
                yield return null;

            StartCoroutine(ContinueBattle(isDefSplitting));
        }

        private IEnumerator ContinueBattle(bool isDefSplitting = false)
        {
            var defenderHand = entitiesHands[1].Item2;
            if (defenderHand.Count != 0 && GetScoreFromString(entitiesHands[1].Item2[0].gameObject.transform
                .Find("1Side").GetComponent<Image>().sprite.name) == 11)
            {
                yield return StartCoroutine(AtkInsuranceHandler());
            }

            if (defenderHand.Count != 0)
                RevealFacedDownCard();

            yield return new WaitForSeconds(2);

            yield return StartCoroutine(SummarizeAndDealDamage(isDefSplitting));

            EndBattle();
        }

        private void EndBattle()
        {
            ResetBattleHUD();

            #region debugInfo
            foreach (var entity in GameManager.Instance.GetEntitiesList())
            {
                Debug.Log($"Entity with name {entity.GetEntityName} have {entity.GetEntityHp}");
            }
            #endregion

            GameManager.Instance.TogglePlayersHUD(true);

            IsBattleActive = false;

            /*var entityInit = entitiesHands[0].Item1;*/

            /*entityInit.StartMove();*/

            DeleteAllRestBattleHUDObject();
        }

        private IEnumerator SummarizeAndDealDamage(bool isDefSplitting = false)
        {
            int totalAtkScore = 0;
            int totalDefScore = 0;

            bool isAtkBlackJack = false;
            bool isDefBlackJack = false;

            bool isAtkHaveDoubleEffect = entitiesHands[0].Item1.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown);
            bool isAtkHaveSplitEffect = entitiesHands[0].Item1.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Split);

            bool isEvade = false;

            if (!isAtkHaveDoubleEffect && !isAtkHaveSplitEffect)
            {
                for (int i = 0; i < entitiesHands[0].Item2.Count; i++)
                {
                    totalAtkScore += GetScoreFromString(entitiesHands[0].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

                    if (totalAtkScore == 21 && i == 1)
                    {
                        isAtkBlackJack = true;
                        break;
                    }

                    if (i == 1)
                    {
                        if (CheckForSplit(entitiesHands[0].Item2[i], entitiesHands[0].Item2[i - 1]) && atkPlayerInsuranceChoose != true)
                        {
                            yield return StartCoroutine(AtkSplitHandler());
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < entitiesHands[1].Item2.Count; i++)
            {
                totalDefScore += GetScoreFromString(entitiesHands[1].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

                if (totalDefScore == 21 && i == 1)
                {
                    isDefBlackJack = true;
                    break;
                }

                if (i == 1 && isDefSplitting)
                    isEvade = CheckForSplit(entitiesHands[1].Item2[i], entitiesHands[1].Item2[i - 1]);
            }

            var atkPlayer = entitiesHands[0].Item1;
            var defPlayer = entitiesHands[1].Item1;

            if (isEvade && isDefSplitting) // Можлива ще якась логіка по типу анімації
            {
                Debug.Log($"Split!!!");
                yield break;
            }

            if (atkPlayerSplitChoose == true || isAtkHaveSplitEffect)
            {
                atkPlayerSplitChoose = null;
                Debug.Log($"ATKSplit!!!");
                CardSplitting();
                yield return StartCoroutine(SplitController(isDefSplitting));
                yield return new WaitForSeconds(3);
                yield break;
            }
            else if (atkPlayerSplitChoose == false)
                Debug.Log($"SkipATKSplit!!!");

            if (!isAtkBlackJack && (entitiesCardAdds.Item1 != 0))
            {
                yield return StartCoroutine(SpawnLeftCards(true));
                totalAtkScore = SummarizeHandDamage(entitiesHands[0].Item2);
            }
            if (!isDefBlackJack && (entitiesCardAdds.Item2 != 0))
            {
                yield return StartCoroutine(SpawnLeftCards(false));
                totalDefScore = SummarizeHandDamage(entitiesHands[1].Item2);
            }

            if (atkPlayerInsuranceChoose == true) // якщо гравець атаки застрахувався то він отримує негайно половину неблокуючих пошкоджень по собі
            {
                Debug.Log($"Insurance!!!");
                GameManager.Instance.DealDamage(atkPlayer, totalAtkScore / 2);
                yield break;
            }

            if (isAtkHaveDoubleEffect)
                totalAtkScore *= 2;
            if (defPlayer.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Split))
                totalDefScore /= 2;

            var totalDamage = totalAtkScore == totalDefScore ? 5 :
                (totalAtkScore + atkPlayer.GetEntityAtk + 10) -
                (isDefSplitting ? 0 : (totalDefScore + defPlayer.GetEntityDef));

            Debug.Log($"totalAtkScore: {totalAtkScore}");
            Debug.Log($"totalDefScore: {totalDefScore}");
            Debug.Log($"TotalDamage: {totalDamage}");

            if (isAtkBlackJack && isDefBlackJack) // Пуш = 5 пошкоджень
            {
                GameManager.Instance.DealDamage(defPlayer, 5);
                Debug.Log($"Push!!!");
                yield break;
            }
            else if (isAtkBlackJack) // коли блекджек у бійця атаки то він наносить подвоєні пошкодження захиснику та блокуючі пошкодження іншому випадковому гравцю
            {
                GameManager.Instance.DealDamage(defPlayer, totalDamage * 2);

                var exceptionsEntitiesList = new List<IEntity>()
                {
                    atkPlayer,
                    defPlayer
                };
                var anotherEntity = GameManager.Instance.GetRandomPlayerExcept(exceptionsEntitiesList);

                GameManager.Instance.DealDamage(anotherEntity, totalDamage, true);
                Debug.Log($"isAtkBlackJack!!!");
                yield break;
            }
            else if (isDefBlackJack) // коли блекджек у бійця захисту то він відбиває всі пошкодження атакуючого назад та виліковується на половину цих пошкоджень
            {
                GameManager.Instance.DealDamage(atkPlayer, totalDamage);
                GameManager.Instance.Heal(defPlayer, totalDamage / 2);
                Debug.Log($"isDefBlackJack!!!");
                yield break;
            }

            GameManager.Instance.DealDamage(defPlayer, totalDamage);
        }

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

        private IEnumerator AtkInsuranceHandler()
        {
            var atkEnity = entitiesHands[0].Item1;

            switch (atkEnity.GetEntityType)
            {
                case EntityType.Player:
                    AtkInsuranceForPlayer();
                    break;

                case EntityType.Enemy:
                    AtkInsuranceForEnemy();
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            while (atkPlayerInsuranceChoose == null)
            {
                yield return null;
            }
        }

        private void AtkInsuranceForPlayer()
        {
            AtkInsuranceButtonsTurn(true);
        }

        private void AtkInsuranceForEnemy() // Обробка страхування гравця атаки, для противників
        {
            if (entitiesHands[0].Item1.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Clairvoyance))
            {
                var faceSide = entitiesHands[1].Item2[1].gameObject.transform.Find("2Side").GetComponent<Image>().sprite.name;
                int cardScore = GetScoreFromString(faceSide);

                if (cardScore != 10)
                    atkPlayerInsuranceChoose = false;
                else
                    atkPlayerInsuranceChoose = true;

                return;
            }

            if (Random.Range(1, 101) <= 5) // приблизний шанс блекджеку з перших двох карт
                atkPlayerInsuranceChoose = true;
            else
                atkPlayerInsuranceChoose = false;
        }

        private void AtkSplit()
        {
            atkPlayerSplitChoose = true;
        }

        private void AtkSplitSkip()
        {
            atkPlayerSplitChoose = false;
        }

        private void AtkInsurance()
        {
            atkPlayerInsuranceChoose = true;
        }

        private void RequestForAtkInsuranceSkipServerRpc()
        {
            atkPlayerInsuranceChoose = false;
        }

        private void SetupHands(IEntity atkId, IEntity defId)
        {
            entitiesHands.Add(new Tuple<IEntity, List<NextCardScript>>(atkId, new List<NextCardScript>()));
            entitiesHands.Add(new Tuple<IEntity, List<NextCardScript>>(defId, new List<NextCardScript>()));
        }

        private void AttackButtonTurn(bool isTurningOn = true)
        {
            _atackButton.gameObject.SetActive(isTurningOn);
        }

        private void DefendButtonTurn(bool isTurningOn = true)
        {
            _defendButton.gameObject.SetActive(isTurningOn);
        }

        private void InsuranceButtonTurn(bool isTurningOn = true)
        {
            _insuranceButton.gameObject.SetActive(isTurningOn);
        }

        private void AtkSplitButtonsTurn(bool isTurningOn = true)
        {
            _splitSkippingButton.gameObject.SetActive(isTurningOn);
            _splitAtkButton.gameObject.SetActive(isTurningOn);
        }

        private void AtkInsuranceButtonsTurn(bool isTurningOn = true)
        {
            _insuranceButton.gameObject.SetActive(isTurningOn);
            _insuranceSkippingButton.gameObject.SetActive(isTurningOn);
        }

        private void SpawnNextCard(string cardName, int handNumber = -1, bool isAttacker = true, bool facedDown = false)
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
                card.FlipCard();
            else
            {
                var entityInit = isAttacker ? entitiesHands[0].Item1 : entitiesHands[1].Item1;
                OnEntityDrawCard?.Invoke(card, entityInit);
            }

            if (isAttacker && handNumber == -1)
            {
                card.handNumber = handNumber;
                entitiesHands[0].Item2.Add(card); // 0 гравець - атакуючий
                cardContainer = AtkHandContainer;
                destinationCords = AtkHandContainer.transform.position;
            }
            else if (!isAttacker && handNumber == -1)
            {
                card.handNumber = handNumber;
                entitiesHands[1].Item2.Add(card); // 1 гравець - захисник
                cardContainer = DefHandContainer;
                destinationCords = DefHandContainer.transform.position;
            }
            else if (isAttacker && !(handNumber == -1))
            {
                card.isAppended = true;
                card.handNumber = handNumber;
                entitiesHands[0].Item2.Add(card);
                var hands = entitiesHands[0].Item2.FindAll(card => card.isAppended == false);

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
                entitiesHands[1].Item2.Add(card);
                var hands = entitiesHands[1].Item2.FindAll(card => card.isAppended == false);

                cardContainer = hands[handNumber].gameObject.transform
                    .Cast<Transform>()
                    .Select(t => t.gameObject)
                    .FirstOrDefault(container => container.name.EndsWith("ToLeft"));
                cardContainer.SetActive(true);
                destinationCords = DefHandContainer.transform.position;
            }

            StartCoroutine(WaitForGiveCard(card, destinationCords, cardContainer));
        }

        private void SpawnBattleAvatars(string atkName, string defName)
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

        private void CallBattleHUD()
        {
            battleHUD.gameObject.SetActive(true);
        }

        private void ResetBattleHUD()
        {
            battleHUD.gameObject.SetActive(false);
            battleConfirmUI.SetActive(false);
        }

        private void DeleteAllRestBattleHUDObject()
        {
            Debug.Log("DeleteAllRestBattleHUDObjectClientRpc");

            foreach (Transform card in AtkHandContainer.transform)
                Destroy(card.gameObject);

            foreach (Transform card in DefHandContainer.transform)
                Destroy(card.gameObject);

            foreach (var avatar in battleAvatars)
                Destroy(avatar);

            foreach (var card in entitiesHands[0].Item2)
                Destroy(card.gameObject);

            foreach (var card in entitiesHands[1].Item2)
                Destroy(card.gameObject);

            entitiesCardAdds = new ValueTuple<int, int>(0, 0);

            entitiesHands.Clear();

            if (cardGiving != null)
            {
                StopCoroutine(cardGiving);
                cardGiving = null;
            }

            activeDeck.Clear();
            battleAvatars.Clear();
            atkPlayerSplitChoose = null;
            atkPlayerInsuranceChoose = null;
        }

        private void RevealFacedDownCard() => entitiesHands[1].Item2.Find(card => card.isFacedDown).FlipCard();

        private void CardSplitting(bool isAtk = true)
        {
            if (isAtk)
            {
                var addCard = entitiesHands[0].Item2.Find(card => card.isAppended == true);

                addCard.isAppended = false;
                addCard.handNumber = -1;
                addCard.transform.SetParent(AtkHandContainer.transform);
            }
        }

        private bool IsFrozenDuringTimeStop(IEntity entity)
        {
            return GlobalEffectsManager.Instance.IsTimeStopped &&
                !entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Chronomaster);
        }

        #region Корутины

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

        private IEnumerator AtkSplitHandler()
        {
            var atkEntity = entitiesHands[0].Item1;

            switch (atkEntity.GetEntityType)
            {
                case EntityType.Player:
                    AllowSplitForPlayer();
                    break;

                case EntityType.Enemy:
                    AtkSplitForEnemy();
                    break;

                case EntityType.Ally:
                    // код, якщо будуть помічники
                    break;

                default:
                    break;
            }

            while (atkPlayerSplitChoose == null)
            {
                yield return null;
            }
        }

        private void AtkSplitForEnemy()
        {
            if (Random.Range(1, 101) <= 50)
                atkPlayerSplitChoose = true;
            else
                atkPlayerSplitChoose = false;
        }

        private void AllowSplitForPlayer()
        {
            AtkSplitButtonsTurn(true);
        }

        /*private IEnumerator Starttimer(Activity act, int actTimer = 0)
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
        }*/

        private IEnumerator SpawnLeftCards(bool isForAtk)
        {
            if (isForAtk)
            {
                while (entitiesCardAdds.Item1 > 0)
                {
                    SpawnNextCard(0, true, false);
                    --entitiesCardAdds.Item1;

                    while (cardGiving != null)
                        yield return null;
                }
            }
            else if (!isForAtk)
            {
                while (entitiesCardAdds.Item2 > 0)
                {
                    SpawnNextCard(0, false, false);
                    --entitiesCardAdds.Item2;

                    while (cardGiving != null)
                        yield return null;
                }
            }
        }

        private IEnumerator SplitController(bool isDefSplitting = false)
        {
            var hands = entitiesHands[0].Item2.FindAll(card => card.isAppended == false);
            List<IEntity> alreadyAttackedEntities = new List<IEntity>();

            int totalAtkScore = 0;
            int totalDefScore = 0;

            bool isAtkHaveDoubleEffect = entitiesHands[0].Item1.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.DoubleDown);
            bool isAtkHaveSplitEffect = entitiesHands[0].Item1.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Split);

            bool isDefBlackJack = false;
            bool isCanSplit = !isAtkHaveDoubleEffect;

            for (int i = 0; i < entitiesHands[1].Item2.Count; i++)
            {
                totalDefScore += GetScoreFromString(entitiesHands[1].Item2[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

                if (totalDefScore == 21 && i == 1)
                {
                    isDefBlackJack = true;
                    break;
                }
            }

            for (int i = 0; i < entitiesCardAdds.Item1; i++)
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
                    if (CheckForSplit(entitiesHands[0].Item2[0], entitiesHands[0].Item2[entitiesHands[0].Item2.Count - 1]))
                    {
                        yield return StartCoroutine(AtkSplitHandler());

                        if (atkPlayerSplitChoose == true)
                        {
                            Debug.Log($"SecondATKSplit!!!");
                            atkPlayerSplitChoose = null;
                            CardSplitting();
                            yield return new WaitForSeconds(2);

                            isCanSplit = false;
                            continue;
                        }
                    }
                }

                hands = entitiesHands[0].Item2.FindAll(card => card.isAppended == false);
                bool isFirstHandAlreadyFull = entitiesHands[0].Item2
                    .FindAll(card => card.isAppended == true && card.handNumber == 0).Count == entitiesCardAdds.Item1;

                for (int j = 0; j < hands.Count; j++)
                {
                    if (j == 0 && isFirstHandAlreadyFull)
                        continue;

                    for (int k = i; k < entitiesCardAdds.Item1; k++)
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

            entitiesCardAdds.Item1 = 0;

            // ! додати підрахунок додаткових карт атакуючому
            for (int i = 0; i < hands.Count; i++) // підрахунок усього дамагу гравця атаки 
            {
                totalAtkScore += GetScoreFromString(hands[i].gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);

                var additionalCards = entitiesHands[0].Item2.FindAll(card => card.handNumber == i);

                if (additionalCards.Count > 0)
                {
                    foreach (var card in additionalCards)
                    {
                        totalAtkScore += GetScoreFromString(card.gameObject.transform.Find("1Side").GetComponent<Image>().sprite.name);
                    }
                }

                if (isAtkHaveDoubleEffect)
                    totalAtkScore *= 2;

                if (i == 0)
                {
                    var defPlayer = entitiesHands[1].Item1;
                    var atkPlayer = entitiesHands[0].Item1;

                    if (!isDefBlackJack && (entitiesCardAdds.Item2 != 0))
                    {
                        yield return StartCoroutine(SpawnLeftCards(false));
                        totalDefScore = SummarizeHandDamage(entitiesHands[1].Item2);
                    }

                    if (defPlayer.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Split))
                        totalDefScore /= 2;

                    var totalDamage = totalAtkScore == totalDefScore ? 5 :
                    (totalAtkScore + atkPlayer.GetEntityAtk + 10) -
                    (isDefSplitting ? 0 : (totalDefScore + defPlayer.GetEntityDef));

                    if (isAtkHaveSplitEffect)
                        totalDamage /= 2;

                    Debug.Log($"SplitController: totalAtk: {totalDamage} totalDef: {totalDefScore}");

                    if (isDefBlackJack)
                    {
                        GameManager.Instance.DealDamage(atkPlayer, totalDamage);
                        GameManager.Instance.Heal(defPlayer, totalDamage / 2);
                        Debug.Log($"AfterSplitIsDefBlackJack!!!");
                        continue;
                    }

                    GameManager.Instance.DealDamage(defPlayer, totalDamage);
                    Debug.Log($"Split damage to entity: {defPlayer.GetEntityName}; Dmg: {totalDamage}");

                    alreadyAttackedEntities.Add(atkPlayer);
                    alreadyAttackedEntities.Add(defPlayer);

                    totalAtkScore = 0;
                    continue;
                }

                var anotherEntity = GameManager.Instance.GetRandomPlayerExcept(alreadyAttackedEntities);

                if (anotherEntity == null)
                    yield break;

                if (isAtkHaveSplitEffect)
                    totalAtkScore /= 2;

                GameManager.Instance.DealDamage(anotherEntity, totalAtkScore, true);
                Debug.Log($"Split damage to Entity: {anotherEntity.GetEntityName}; Dmg: {totalAtkScore}");
                alreadyAttackedEntities.Add(anotherEntity);

                totalAtkScore = 0;
            }
        }

        #endregion

        #region Звичайні функції

        public void ToggleBattleHudControls(bool isActive)
        {
            _atackButton.interactable = isActive;
            _defendButton.interactable = isActive;
            _splitDefButton.interactable = isActive;
            battleEffectCardsApplier.ToggleBattleCardApplier(isActive);
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

        private void OnInsuranceButtonClick()
        {
            _insuranceButton.gameObject.SetActive(false);
            _insuranceSkippingButton.gameObject.SetActive(false);

            Debug.Log("OnInsuranceButtonClick");

            AtkInsurance();
        }

        private void OnSplitAtkButtonClick()
        {
            _splitAtkButton.gameObject.SetActive(false);
            _splitSkippingButton.gameObject.SetActive(false);

            Debug.Log("OnSplitAtkButtonClick");

            AtkSplit();

            StopTimer();
        }

        private void OnSplitSkippingButtonClick()
        {
            _splitAtkButton.gameObject.SetActive(false);
            _splitSkippingButton.gameObject.SetActive(false);

            Debug.Log("OnSplitSkippingButtonClick");

            AtkSplitSkip();

            StopTimer();
        }

        private void OnInsuranceSkippingButtonClick()
        {
            _insuranceButton.gameObject.SetActive(false);
            _insuranceSkippingButton.gameObject.SetActive(false);

            Debug.Log("OnInsuranceSkippingButtonClick");

            RequestForAtkInsuranceSkipServerRpc();
        }

        private void StopTimer()
        {
            if (timerRunning == null)
                return;

            StopCoroutine(timerRunning);
            timerText.text = "";
            timerRunning = null;
        }

        /*private void ActivateButtons(bool isAttacker, ulong playerId, bool isPreparation = true)
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
        }*/

        private void SpawnNextCard(int handNumber = -1, bool isAttacker = true, bool facedDown = false) // 0 = карта не додається до вже готової руки(максимум 4)
        {
            if (activeDeck.Count == 0)
                SetupDeck();

            SpawnNextCard(activeDeck[0].name, handNumber, isAttacker, facedDown);
            activeDeck.Remove(activeDeck[0]);
        }

        private void SetupDeck()
        {
            activeDeck.AddRange(GameManager.Instance.BasicCardsList);

            System.Random random = new System.Random();
            activeDeck = activeDeck.OrderBy(x => random.Next()).ToList();
        }

        private bool CanAttack(IEntity entity)
        {
            if (entity.GetEntityHp == 0)
                return false;

            return true;
        }

        #endregion

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

        #region Додавання додаткових карт
        public void AddAdditionalCards(int amount, bool forAtk)
        {
            if (forAtk)
                entitiesCardAdds.Item1 += amount;
            else
                entitiesCardAdds.Item2 += amount;
        }
        #endregion

        public BattleEffectCardApplier GetBattlePlayerEffectCardsApplier => battleEffectCardsApplier;

        /*private void ResetAddingCards()
        {
            *//*leftCardAdds = 3;*//*
            alreadyAddedCardsCount = 0;
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

            if (alreadyAddedCardsCount == 0)
                return;

            if (isAtk)
                TESTSendAtkAddCards(alreadyAddedCardsCount);
            else
                TESTSendDefAddCards(alreadyAddedCardsCount);
        }

        private void TESTSendAtkAddCards(int addCardsAmount)
        {
            playerCardAdds.Item1 = addCardsAmount;
        }

        private void TESTSendDefAddCards(int addCardsAmount)
        {
            playerCardAdds.Item2 = addCardsAmount;
        }

        private void ResetAddingCards()
        {
            *//*leftCardAdds = 3;*//*
            alreadyAddedCardsCount = 0;
        }

        private void OnAdd1CardButtonAtk()
        {
            if (leftCardAdds == 0)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 1;
            ++alreadyAddedCardsCount;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        private void OnAdd2CardButtonAtk()
        {
            if (leftCardAdds < 2)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 2;
            alreadyAddedCardsCount += 2;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        private void OnAdd3CardButtonAtk()
        {
            if (leftCardAdds < 3)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 3;
            alreadyAddedCardsCount += 3;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        private void OnAdd1CardButtonDef()
        {
            if (leftCardAdds == 0)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 1;
            ++alreadyAddedCardsCount;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        private void OnAdd2CardButtonDef()
        {
            if (leftCardAdds < 2)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 2;
            alreadyAddedCardsCount += 2;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        private void OnAdd3CardButtonDef()
        {
            if (leftCardAdds < 3)
            {
                Debug.Log("AlreadyMaxAddingCards!!!");
                return;
            }

            leftCardAdds -= 3;
            alreadyAddedCardsCount += 3;

            TESTleftCardsAddingText.text = leftCardAdds.ToString();
            TESTAlreadyAddedCardsText.text = alreadyAddedCardsCount.ToString();
        }

        #endregion*/
    }
}
