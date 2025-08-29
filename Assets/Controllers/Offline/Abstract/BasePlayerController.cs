using Singleplayer.PassiveEffects;
using Singleplayer.ActiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace Singleplayer
{
    public abstract class BasePlayerController : MonoBehaviour, IEntity, IOutlinable
    {
        private Action<IEffectCardLogic> onCounterChoiceCallback;
        private Action<bool> onAbilityCounterChoiceCallback;
        public event Action<IEntity> moveEndEvent;
        public event Action<IEntity> OnSelfClickHandled;

        public event Action HpChangeEvent, StatsChangeEvent, CurencyChangeEvent, LeftEffectCardsChangeEvent, LeftStepsChangeEvent;
        public event Action<bool> OnOutlineChanged;

        public Animator Animator { get; protected set; }

        public IPassiveEffectHandler PassiveEffectHandler { get; protected set; }
        public PlayerEffectCardsHandler EffectCardsHandler { get; protected set; }

        public BaseActiveGlobalEffect SpecialAbility {  get; protected set; }

        public CameraController PlayerCamera { get; protected set; }
        /*public bool IsTemporarilyImmortal { get; protected set; }*/ // можливо для іншого режиму складності(наприклад для легкого)

        public bool SuppressPanelEffectTrigger { get; set; } = true;
        public bool IgnoreDirectionOnce { get; set; } = false;

        protected CharacterInfo characterInfo;

        protected SpriteRenderer spriteRenderer;
        protected Material defaultSpriteMaterial;
        protected Material outlineSpriteMaterial;

        protected PanelScript currentPanel;

        protected Coroutine moving;

        protected List<MoveCard> moveCardsDeck = new List<MoveCard>();
        protected List<MoveCard> activeMoveCardsDeck = new List<MoveCard>();
        protected List<Sprite> spritesForMoveCardsDeck = new List<Sprite>();

        protected int hp;
        protected int currentMaxHp;
        protected int money;
        protected int chips;
        protected int atk;
        protected int def;
        protected int leftSteps;
        protected int leftCards;
        protected int defaultCardUsages;

        protected float previousCordY = -1170f;
        protected float initialZ;

        protected float moveSpeed = 300f;

        protected bool canMove = false;
        protected bool isEventAttack = false;

        protected string cardSuit = "Diamond"; // хардкод
        protected string characterName = "";

        protected Direction direction;

        void Start()
        {
            SubscribeToClickEvent();
            initialZ = transform.position.z; // Сохраняем начальную координату z
            SetupMoveCardsDeckPlayerSuit();
            PassiveEffectHandler = new PassiveEffectHandler(this);
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultSpriteMaterial = spriteRenderer.material;
            outlineSpriteMaterial = EffectCardDealer.Instance.GetEffectCardOutlineMaterial; // заглушка, щоб була хоч якась обводка
        }

        void Update()
        {
            if (leftSteps > 0 && moving == null && canMove)
            {
                Debug.Log($"Update Coroutine Started, StepsLeft {leftSteps}");
                moving = StartCoroutine(Move(this));
            }

            /*if (Mathf.Abs(transform.position.y - previousCordY) > 10)
            {
                if (transform.position.y < previousCordY)
                {
                    initialZ += 0.01f;
                    previousCordY = transform.position.y;
                }
                else if (transform.position.y > previousCordY)
                {
                    initialZ -= 0.01f;
                    previousCordY = transform.position.y;
                }
            }

            transform.position = new Vector3(transform.position.x, transform.position.y, initialZ);*/
        }

        private void LateUpdate()
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 0.5f);
        }

        private void SubscribeToClickEvent()
        {
            var clickHandler = GetComponentInChildren<ClickHandler>();
            clickHandler.OnEntityClickEvent += OnEntityClick;
        }

        public void OnEntityClick()
        {
            Debug.Log($"Entity {characterInfo.CharacterName} being clicked");
            OnSelfClickHandled?.Invoke(this);
        }

        private void SetupMoveCardsDeckPlayerSuit()
        {
            spritesForMoveCardsDeck = SpriteLoadManager.Instance.GetAllBasicCardSpritesOfSuit(cardSuit);

            spritesForMoveCardsDeck.RemoveAll(sprite =>
            {
                if (sprite.name.Length < 2) return false;

                string numPart = sprite.name.Substring(sprite.name.Length - 2);
                return int.TryParse(numPart, out int val) && val >= 11;
            });

            moveCardsDeck = new List<MoveCard>();

            foreach (var sprite in spritesForMoveCardsDeck)
            {
                moveCardsDeck.Add(new MoveCard(sprite));
            }
        }

        private void SetupActiveMoveCardsDeck()
        {
            activeMoveCardsDeck.AddRange(moveCardsDeck);

            System.Random random = new System.Random();
            activeMoveCardsDeck = activeMoveCardsDeck.OrderBy(x => random.Next()).ToList();
        }

        public MoveCard GetNextMoveCard()
        {
            if (activeMoveCardsDeck == null || activeMoveCardsDeck.Count == 0)
                SetupActiveMoveCardsDeck();

            int index = Random.Range(0, activeMoveCardsDeck.Count);
            MoveCard nextCard = activeMoveCardsDeck[index];
            activeMoveCardsDeck.RemoveAt(index);
            return nextCard;
        }

        public void OnNewTurnStart()
        {
            ResetPlayerStats();
            PassiveEffectHandler.ProcessEffects();
            NormalizeHp();

            SpecialAbility.OnNewTurnStart();
        }

        public void StartTurn()
        {
            if (hp == 0)
            {
                Revive();
                TurnManager.Instance.EndTurnRequest(this);
                return;
            }

            ResetEffectCardsUsages();
            GameManager.Instance.TogglePlayerHudButtons(true);
            EffectCardsHandler.OnNewTurnStart();
        }

        public abstract void ActivateAbility();

        #region Старий код Контрпіка проти чогось
        /*public void ShowCounterCardOptions(List<IEffectCardLogic> counterCards, Action<IEffectCardLogic> callback)
        {
            onCounterChoiceCallback = callback;

            EffectCardsHandler.EnableAndOutlineCounterCards(counterCards);

            EffectCardApplier.Instance.gameObject.SetActive(true);
            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.AddListener(OnCounterSkip);
            counterSkipBtn.gameObject.SetActive(true);

            MapManager.Instance.OnEffectCardPlayedEvent += HandleCardSelected;
        }

        public void GiveAbilityCounterOption(Action<bool> callback)
        {
            onAbilityCounterChoiceCallback = callback;
            GameManager.Instance.TogglePlayerSpecialAbilityBtn(true);

            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.AddListener(OnAbilityCounterSkip);
            counterSkipBtn.gameObject.SetActive(true);

            SpecialAbility.GlobalEffectStateEvent += OnAbilityUsageForCounter;
        }

        private void HandleCardSelected(IEffectCardLogic selectedCard)
        {
            DisableCounterPick();
            onCounterChoiceCallback?.Invoke(selectedCard);
        }

        private void OnCounterSkip()
        {
            DisableCounterPick();
            onCounterChoiceCallback?.Invoke(null);
        }

        private void OnAbilityUsageForCounter()
        {
            DisableAbilityCounter();
            onAbilityCounterChoiceCallback?.Invoke(true);
        }

        private void OnAbilityCounterSkip()
        {
            DisableAbilityCounter();
            onAbilityCounterChoiceCallback?.Invoke(false);
        }

        private void DisableCounterPick()
        {
            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.RemoveAllListeners();
            counterSkipBtn.gameObject.SetActive(false);
            EffectCardApplier.Instance.gameObject.SetActive(false);

            MapManager.Instance.OnEffectCardPlayedEvent -= HandleCardSelected;
            EffectCardsHandler.DisableAndRemoveOutlineOfCounterCards();
        }

        private void DisableAbilityCounter()
        {
            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.RemoveAllListeners();
            counterSkipBtn.gameObject.SetActive(false);

            SpecialAbility.GlobalEffectStateEvent -= OnAbilityUsageForCounter;
        }*/
        #endregion

        public void ShowCounterOptions(List<IEffectCardLogic> counterCards, Action<IEffectCardLogic> onCardChoice, 
            Action<bool> onAbilityChoice, bool showAbilityBtn)
        {
            onCounterChoiceCallback = onCardChoice;
            onAbilityCounterChoiceCallback = onAbilityChoice;

            EffectCardApplier.Instance.gameObject.SetActive(true);

            // --- Карты ---
            if (counterCards != null && counterCards.Count > 0)
            {
                EffectCardsHandler.EnableAndOutlineCounterCards(counterCards);
                MapManager.Instance.OnEffectCardPlayedEvent += HandleCardSelected;
            }

            // --- Абилка ---
            if (showAbilityBtn && SpecialAbility != null)
            {
                GameManager.Instance.TogglePlayerSpecialAbilityBtn(true);
                SpecialAbility.GlobalEffectStateEvent += OnAbilityUsageForCounter;
            }

            // --- Общая кнопка Skip ---
            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.AddListener(OnGlobalSkip);
            counterSkipBtn.gameObject.SetActive(true);
        }

        private void HandleCardSelected(IEffectCardLogic selectedCard)
        {
            CleanupUI();
            onCounterChoiceCallback?.Invoke(selectedCard);
        }

        private void OnAbilityUsageForCounter()
        {
            CleanupUI();
            onAbilityCounterChoiceCallback?.Invoke(true);
            onCounterChoiceCallback?.Invoke(null); // карта считается невыбранной
        }

        private void OnGlobalSkip()
        {
            CleanupUI();
            onCounterChoiceCallback?.Invoke(null);
            onAbilityCounterChoiceCallback?.Invoke(false);
        }

        private void CleanupUI()
        {
            var counterSkipBtn = EffectCardApplier.Instance.GetCounterSkipBtn;
            counterSkipBtn.onClick.RemoveAllListeners();
            counterSkipBtn.gameObject.SetActive(false);

            EffectCardApplier.Instance.gameObject.SetActive(false);

            // снять подписки
            MapManager.Instance.OnEffectCardPlayedEvent -= HandleCardSelected;
            EffectCardsHandler.DisableAndRemoveOutlineOfCounterCards();

            if (SpecialAbility != null)
                SpecialAbility.GlobalEffectStateEvent -= OnAbilityUsageForCounter;

            GameManager.Instance.TogglePlayerSpecialAbilityBtn(false);
        }

        #region PlayerActions
        public void Jump()
        {
            Animator.SetBool("Jump", true);
        }

        public void JumpOff()
        {
            Animator.SetBool("Jump", false);
        }

        public void Dead()
        {
            Animator.SetBool("Dead", true);
        }

        public void DeadOff()
        {
            Animator.SetBool("Dead", false);
        }
        public void Walk()
        {
            Animator.SetBool("Walk", true);
        }

        public void WalkOff()
        {
            Animator.SetBool("Walk", false);
        }
        public void Run()
        {
            Animator.SetBool("Run", true);
        }
        public void RunOff()
        {
            Animator.SetBool("Run", false);
        }
        public void Attack()
        {
            Animator.SetBool("Attack", true);
        }
        public void AttackOff()
        {
            Animator.SetBool("Attack", false);
        }
        #endregion

        public virtual void SetupPlayer(CharacterInfo characterInfo)
        {
            this.characterInfo = characterInfo;

            hp = characterInfo.DefaultHp;
            currentMaxHp = characterInfo.DefaultHp;
            money = characterInfo.DefaultMoney;
            chips = characterInfo.DefaultChips;
            atk = characterInfo.DefaultAtk;
            def = characterInfo.DefaultDef;
            defaultCardUsages = characterInfo.DefaultCardUsages;
            characterName = characterInfo.CharacterName;

            SetupPlayerSpecialAbility();

            Animator = gameObject.GetComponent<Animator>();

            //direction = ??? // код для визначення можливої траекторії руху після спавну гравця

            PlayerCamera = GetComponentInChildren<CameraController>();
        }

        public void SetupPlayerSpecialAbility(ActiveGlobalEffectInfo activeGlobalEffectInfo = null)
        {
            ActiveGlobalEffectInfo specialAbilityInfo = null;

            if (activeGlobalEffectInfo == null)
                specialAbilityInfo = this.characterInfo.SpecialAbility;
            else
                specialAbilityInfo = activeGlobalEffectInfo;

            SpecialAbility = BaseActiveGlobalEffect.GetActiveGlobalEffectInstance(specialAbilityInfo, this);
        }

        public void Pay(int value, bool useChips)
        {
            if (useChips)
                chips = Mathf.Max(0, chips - value);
            else
                money = Mathf.Max(0, money - value);

            CurencyChange();
        }

        protected virtual void ResetPlayerStats()
        {
            atk = characterInfo.DefaultAtk;
            def = characterInfo.DefaultDef;
        }

        protected virtual void NormalizeHp()
        {
            hp = Mathf.Min(hp, currentMaxHp);
            HpChange();
        }

        public void GainMoney(int value, bool withChips)
        {
            if (withChips)
                chips += value;
            else
                money += value;

            CurencyChange();
        }

        public virtual void GetDamage(int value)
        {
            hp -= value;
            hp = Mathf.Max(hp, 0);

            HpChange();

            if (hp == 0)
                Knockout();
        }

        public virtual void RaiseAtkStat(int value)
        {
            if (value <= 0) return;
            atk += value;

            StatsChange();
        }

        public virtual void RaiseLeftCards(int value)
        {
            if (value <= 0) return;
            leftCards += value;

            LeftEffectCardsChangeEvent?.Invoke();
        }

        private void Knockout()
        {
            Dead();
            /*IsTemporarilyImmortal = true;*/
            PassiveEffectHandler.RemoveAllEffects();

            if (money > 1)
            {
                int lostMoney = money / 2;
                Pay(lostMoney, false);

                var droppedMoneyPrefab = GameManager.Instance.GetDroppedMoneyPrefab;
                var droppedMoneyGO = Instantiate(droppedMoneyPrefab, currentPanel.transform.position, Quaternion.identity);
                var droppedMoneyHandler = droppedMoneyGO.GetComponent<DroppedMoneyHandler>();
                droppedMoneyHandler.ManageDroppedMoney(lostMoney);
                droppedMoneyGO.SetActive(true);
            }

            TurnManager.Instance.EndTurnRequest(this);
        }

        private void Revive()
        {
            DeadOff();
            hp = characterInfo.DefaultHp;
            HpChange();
        }

        public virtual void Heal(int value)
        {
            if (hp == currentMaxHp)
                return;

            if ((hp + value) > currentMaxHp)
            {
                hp = currentMaxHp;
                return;
            }

            hp += value;

            HpChange();
        }

        public IEnumerator StopAnimationSmoothly(float duration)
        {
            float startSpeed = Animator.speed;
            float t = 0;

            while (t < duration)
            {
                Animator.speed = Mathf.Lerp(startSpeed, 0f, t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            Animator.speed = 0f;
        }

        public IEnumerator ResumeAnimationSmoothly(float duration)
        {
            float startSpeed = Animator.speed;
            float targetSpeed = 1f;
            float t = 0f;

            while (t < duration)
            {
                Animator.speed = Mathf.Lerp(startSpeed, targetSpeed, t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            Animator.speed = targetSpeed;
        }

        public void HideEntity()
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponentInChildren<ClickHandler>(true).gameObject.SetActive(false);
        }

        public void ShowEntity()
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponentInChildren<ClickHandler>(true).gameObject.SetActive(true);
        }

        public void SetOutline()
        {
            spriteRenderer.material = outlineSpriteMaterial;
            OnOutlineChanged?.Invoke(true);
        }

        public void RemoveOutline()
        {
            spriteRenderer.material = defaultSpriteMaterial;
            OnOutlineChanged?.Invoke(false);
        }

        public virtual void GetSteps(int value)
        {
            leftSteps = value;
            LeftStepsChangeEvent?.Invoke();
        }

        public virtual IEnumerator Move(IEntity player, Direction direction = Direction.Standart, PanelScript panel = null)
        {
            /*if (IsServer)
            {
                Debug.LogWarning("Server MOVE CALL");
            }*/

            Walk();

            Vector2 currentPos = transform.position;

            if (currentPanel == null)
            {
                Debug.LogWarning("Server CurrentPanel is null");
            }

            (PanelScript, Direction) destinationInfo;

            if (panel != null)
            {
                destinationInfo.Item1 = panel;
                destinationInfo.Item2 = direction;
            }
            else
            {
                destinationInfo = currentPanel.GetNextPanelOrNull(player); //<- то самое место где первый раз отбираются панели
            }

            if (destinationInfo.Item1 == null)
            {
                StopCoroutine(nameof(Move));
                moving = null;
                /*leftSteps -= 1;*/
                yield break;
            }

            Vector2 destination = currentPos;

            if (destinationInfo.Item2 == this.direction)
            {
                destination = destinationInfo.Item1.transform.position;
                Debug.Log("Direction good");
            }
            else
            {
                this.direction = destinationInfo.Item2;
                TurnThePlayer(); // ~
                destination = destinationInfo.Item1.transform.position;
            }

            while (Vector2.Distance(currentPos, destination) > 0.01f)
            {
                currentPos = Vector2.MoveTowards(currentPos, destination, moveSpeed * Time.deltaTime);
                transform.position = currentPos;

                yield return null;
            }

            transform.position = new Vector3(destination.x, destination.y, transform.position.z);

            Debug.LogWarning("Left Steps");

            while (!canMove)
                yield return null;

            leftSteps -= 1;
            LeftStepsChangeEvent?.Invoke();

            if (leftSteps == 0)
            {
                WalkOff();
                MoveEnd();
            }

            moving = null;
        }

        private void TurnThePlayer() // ~Функція повороту гравця у бік напрямку ходьби, заготовлена на майбутнє
        {
            Debug.Log("Turning...");
        }

        public void MoveEnd() => moveEndEvent?.Invoke(this);
        public void HpChange() => HpChangeEvent?.Invoke();
        public void StatsChange() => StatsChangeEvent?.Invoke();
        public void CurencyChange() => CurencyChangeEvent?.Invoke();

        public virtual void StartMove(Direction direction = Direction.Standart, PanelScript panel = null)
        {
            if (panel == null)
            {
                canMove = true;
                Walk();
                return;
            }

            if (direction != (Direction)(-1))
                this.direction = direction;

            /*--leftSteps;*/
            Debug.Log($"Coroutine Started, StepsLeft {leftSteps}");
            moving = StartCoroutine(Move(this, direction, panel));
            canMove = true;
        }

        public virtual void StopMoving()
        {
            canMove = false;
            WalkOff();
        }

        public virtual void SetRandomAvailableDirection()
        {
            if (currentPanel == null)
                return;

            var availableDirections = new List<PanelScript.Pos>();

            for (int i = 0; i < 4; i++)
            {
                var neighbor = currentPanel.GetNeighborByIndex(i);
                if (neighbor != null)
                {
                    availableDirections.Add((PanelScript.Pos)i);
                }
            }

            if (availableDirections.Contains((PanelScript.Pos)direction))
                return;

            if (availableDirections.Count > 0)
            {
                var randomDir = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
                Debug.Log($"Выбрано направление: {randomDir}");
                direction = (Direction)randomDir;
            }
        }

        public virtual void TurnEntity()
        {
            Debug.Log("Turning...");
        }

        private IEnumerator TryToStartBattle(IEntity Atk, IEntity Def)
        {
            /// Код для валідації початку атаки
            Debug.Log("BattlStart");

            var battleManager = BattleManager.Instance;
            StopMoving();
            Debug.LogWarning($"ATK: {Atk.GetEntityName} DEF: {Def.GetEntityName}");
            battleManager.TryToStartBattle(Atk, Def);
            yield return new WaitUntil(() => !battleManager.IsBattleActive);
            StartMove();
        }

        public virtual IEnumerator OnStepOntoPanel(PanelScript panel)
        {
            currentPanel = panel;

            if (SuppressPanelEffectTrigger)
            {
                SuppressPanelEffectTrigger = false;
                yield break;
            }

            if (panel.GetEffectPanelInfo.IsForceStop && leftSteps > 1)
            {
                StopMoving();
                bool? stayDecision = null;
                PanelEffectsManager.Instance.SuggestForceStop(decision => stayDecision = decision);

                yield return new WaitUntil(() => stayDecision.HasValue);

                if (stayDecision.Value)
                {
                    leftSteps = 1;
                    LeftStepsChangeEvent?.Invoke();
                }

                StartMove();
            }

            IEntity self = this;
            var otherEntities = panel.EntitiesOnPanel
                .Where(e => e != self && e.GetEntityType != EntityType.Player)
                .ToList();

            foreach (var entity in otherEntities)
            {
                switch (entity.GetEntityType)
                {
                    case EntityType.Enemy:
                        yield return StartCoroutine(TryToStartBattle(this, entity));
                        break;

                    default:
                        break;
                }
            }
        }
        public void ManageEffectCardsHandler(PlayerEffectCardsHandler playerEffectCardsHandler) => EffectCardsHandler = playerEffectCardsHandler;
        public void EnableAttacking() => isEventAttack = true;

        public void ResetEffectCardsUsages()
        {
            leftCards = characterInfo.DefaultCardUsages;
            LeftEffectCardsChangeEvent?.Invoke();
        }

        public void DecreaseEffectCardsUsages()
        {
            --leftCards;
            LeftEffectCardsChangeEvent?.Invoke();
        }

        public string GetEntityName => characterName;
        public EntityType GetEntityType => characterInfo.EntityType;
        public PanelScript GetCurrentPanel => currentPanel;
        public int GetEntityDef => def;
        public int GetEntityHp => hp;
        public int GetEntityMaxHp => currentMaxHp;
        public int GetEntityMoney => money;
        public int GetEntityChips => chips;
        public int GetEntityAtk => atk;
        public int GetEntityLeftCards => leftCards;
        public int GetEntityDefaultCardUsages => defaultCardUsages;
        public int GetEntityLeftSteps => leftSteps;
        public bool GetEntityAttackAccess => isEventAttack;
        public string GetEntitySuit => cardSuit;
        public Direction GetEntityDirection => direction;
    }

    public enum CharacterType
    {
        TimeStopper = 0
    }

    public class MoveCard
    {
        public Sprite frontSide { get; private set; }
        public Sprite backSide {  get; private set; }

        public MoveCard(Sprite frontSidePrint)
        {
            frontSide = frontSidePrint;
        }

        public void PrintBackSide(Sprite backSidePrint) => backSide = backSidePrint;

        public int GetSteps(bool fromFrontSide)
        {
            string spriteName = string.Empty;

            if (fromFrontSide)
                spriteName = frontSide.name;
            else
                spriteName = backSide.name;

            return int.Parse(spriteName.Substring(spriteName.Length - 2));
        }

        public int GetSteps(Sprite sprite)
        {
            string spriteName = sprite.name;

            if (sprite != backSide && sprite != frontSide)
            {
                Debug.Log("Thats isn`t sprite from this moveCard");
                return 0;
            }

            return GetScoreFromString(spriteName);
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
    }
}
