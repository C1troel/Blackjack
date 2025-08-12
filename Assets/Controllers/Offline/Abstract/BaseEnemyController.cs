using Singleplayer.ActiveEffects;
using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace Singleplayer
{
    public enum EnemyType
    {
        Bodyguard,
        Killer,
        MrBet
    }

    public abstract class BaseEnemy : MonoBehaviour, IEntity
    {
        protected const string DEAD_CLIP_NAME = "Die";

        protected int hp, maxHp, atk, def, money, leftCards, defaultCardUsages, leftSteps;
        protected float moveSpeed = 300f; // хардкод!(швидкість переміщення в просторі)
        protected float previousCordY = -1170f; // хардкод!
        protected float initialZ;

        protected bool isEventAttack, isBoss, canTriggerPanels, canMove;

        protected Direction direction = Direction.Standart;

        protected PanelScript currentPanel;
        protected Coroutine moving;

        protected SpriteRenderer spriteRenderer;
        protected Material defaultSpriteMaterial;
        protected Material outlineSpriteMaterial;

        public EnemyInfo enemyInfo { get; protected set; }
        public EnemyEffectCardsHandler EnemyEffectCardsHandler { get; private set; }
        public IPassiveEffectHandler PassiveEffectHandler {  get; protected set; }
        public BaseActiveGlobalEffect SpecialAbility { get; protected set; }

        public event Action<IEntity> moveEndEvent;
        public event Action<IEntity> OnSelfClickHandled;
        public Animator Animator { get; protected set; }

        public bool SuppressPanelEffectTrigger { get; set; } = true;
        public bool IgnoreDirectionOnce { get; set; } = false;

        private void Start()
        {
            SubscribeToClickEvent();
            PassiveEffectHandler = new PassiveEffectHandler(this);
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
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

            if (Mathf.Abs(transform.position.y - previousCordY) > 10)
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

            transform.position = new Vector3(transform.position.x, transform.position.y, initialZ);
        }

        public virtual void SetupEnemy(EnemyInfo enemyInfo)
        {
            this.enemyInfo = enemyInfo;

            hp = enemyInfo.DefaultHp;
            maxHp = enemyInfo.DefaultHp;
            money = enemyInfo.DefaultMoney;
            atk = enemyInfo.DefaultAtk;
            def = enemyInfo.DefaultDef;
            defaultCardUsages = enemyInfo.DefaultCardUsages;

            Animator = gameObject.GetComponent<Animator>();
            EnemyEffectCardsHandler = new EnemyEffectCardsHandler(this, enemyInfo.MaxEffectCards);
            SetupSpecialAbility();

            //direction = ??? // код для визначення можливої траекторії руху після спавну ворога
        }

        #region ШІ противника

        public virtual void OnNewTurnStart()
        {
            /*GetDamage(hp);*/
            
            ResetEntityStats();
            PassiveEffectHandler.ProcessEffects();
            NormalizeHp();

            EnemyEffectCardsHandler.OnNewTurnStart();
            SpecialAbility?.OnNewTurnStart();
        }

        public virtual void StartTurn()
        {
            Debug.Log($"{enemyInfo.name} starting his turn");

            ResetEffectCardsUsages();
            ProcessEnemyTurn();
        }

        public virtual void ProcessEnemyTurn()
        {
            if (EnemyEffectCardsHandler.TryToUseNextPossibleEffectCard())
            {
                Debug.Log($"Enemy {enemyInfo.CharacterName} is trying to use card");
                return;
            }
            else
                Debug.Log($"Enemy {enemyInfo.CharacterName} isn`t using any card");

            if (SpecialAbility != null && SpecialAbility.IsUsable())
            {
                SpecialAbility.TryToActivate();
                return;
            }

            Debug.Log($"{this} start moving");
            MapManager.Instance.MakeADraw(this);
        }

        #endregion

        #region Переміщення по карті
        public virtual void GetSteps(int value) => leftSteps = value;

        public virtual void StartMove(Direction direction = Direction.Standart, PanelScript panel = null)
        {
            if (!isBoss)
            {
                StartMoveToPlayer();
                return;
            }

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
            Walk();
        }

        public virtual IEnumerator Move(IEntity enemy, Direction direction = Direction.Standart, PanelScript panel = null)
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
                destinationInfo = currentPanel.GetNextPanelOrNull(enemy);

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
                TurnEntity(); // ~
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

            leftSteps -= 1;

            while (!canMove)
                yield return null;

            if (leftSteps == 0)
            {
                WalkOff();
                MoveEnd();
            }

            moving = null;
        }
        
        public virtual void StartMoveToPlayer()
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);

            var foundPathToPlayer = MapManager.FindShortestPathConsideringDirection(currentPanel, player.GetCurrentPanel, this);
            foundPathToPlayer.RemoveAt(0);

            /*#region DebugInfo

            Debug.Log("Found enemy path to player:");
            foreach (var panel in foundPathToPlayer)
            {
                Debug.Log(panel.name);
            }

            #endregion*/

            if (moving != null)
                StopCoroutine(moving);

            moving = StartCoroutine(MoveToPlayer(foundPathToPlayer));
            canMove = true;
        }

        public virtual IEnumerator MoveToPlayer(List<PanelScript> pathToPlayer)
        {
            Walk();

            foreach (var panel in pathToPlayer)
            {
                if (leftSteps <= 0)
                    break;

                var currentStayedPanel = currentPanel;
                var currentPos = transform.position;
                var destination = panel.transform.position;

                while (Vector2.Distance(currentPos, destination) > 0.01f)
                {
                    currentPos = Vector2.MoveTowards(currentPos, destination, moveSpeed * Time.deltaTime);
                    transform.position = currentPos;

                    yield return null;
                }

                transform.position = new Vector3(destination.x, destination.y, transform.position.z);

                this.direction = MapManager.GetDirectionFromTo(currentStayedPanel, panel);
                this.currentPanel = panel;

                leftSteps -= 1;

                Debug.LogWarning($"Left Steps: {leftSteps}");

                if (!canMove)
                    yield break;
            }

            WalkOff();
            MoveEnd();

            moving = null;
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

        public void MoveEnd() => moveEndEvent?.Invoke(this);
        #endregion

        #region Вплив на сутність

        protected virtual void ResetEntityStats()
        {

            atk = enemyInfo.DefaultAtk;
            def = enemyInfo.DefaultDef;
        }

        protected virtual void NormalizeHp() => hp = Mathf.Min(hp, maxHp);

        public virtual void GetDamage(int value)
        {
            hp -= value;
            hp = Mathf.Max(hp, 0);

            if (hp == 0)
                Knockout();
        }

        protected virtual void Knockout()
        {
            ManageKnockoutAnimationHandler();
            Dead();

            CleanUpEnemyData();

            DropMoney();

            GameManager.Instance.RemoveEntityFromGame(this);
        }

        protected virtual void DropMoney()
        {
            var gameManager = GameManager.Instance;
            int droppedMoneyAmount = (enemyInfo.DefaultHp / 20) + money;

            var droppedMoneyPrefab = gameManager.GetDroppedMoneyPrefab;
            var droppedMoneyGO = Instantiate(droppedMoneyPrefab, currentPanel.transform.position, Quaternion.identity);
            var droppedMoneyHandler = droppedMoneyGO.GetComponent<DroppedMoneyHandler>();
            droppedMoneyHandler.ManageDroppedMoney(droppedMoneyAmount);
            droppedMoneyGO.SetActive(true);
        }

        private void OnKnockoutAnimationEnd()
        {
            Destroy(gameObject);
        }

        protected virtual void CleanUpEnemyData()
        {
            PassiveEffectHandler.RemoveAllEffects();

            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            Destroy(gameObject.GetComponent<Rigidbody2D>());

            if (moving != null)
            {
                StopCoroutine(moving);
                moving = null;
            }
            StopAllCoroutines(); // На випадок інших корутин

            var clickHandler = GetComponentInChildren<ClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.OnEntityClickEvent -= OnEntityClickEvent;
            }

            // Очищення власних івентів (для запобігання memory leak)
            moveEndEvent = null;
            OnSelfClickHandled = null;
        }

        public virtual void RaiseAtkStat(int value)
        {
            if (value <= 0) return;
            atk += value;
        }

        public virtual void RaiseLeftCards(int value)
        {
            if (value <= 0) return;
            leftCards += value;
        }

        public virtual void Heal(int value)
        {
            if (hp == maxHp)
                return;

            if ((hp + value) > maxHp)
            {
                hp = maxHp;
                return;
            }

            hp += value;
        }

        public void EnableAttacking() => isEventAttack = true;

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

        public void PickUpMoney(int amount) => money += amount;

        public void SetOutline() => spriteRenderer.material = outlineSpriteMaterial;
        public void RemoveOutline() => spriteRenderer.material = defaultSpriteMaterial;
        #endregion

        #region Маніпуляції з ефектними картами
        public void ResetEffectCardsUsages() => leftCards = enemyInfo.DefaultCardUsages;
        public void DecreaseEffectCardsUsages() => --leftCards;
        #endregion

        #region Сутичка з іншою сутністю
        private IEnumerator TryToStartBattle(IEntity Atk, IEntity Def) // PlayerController Def - це заглушка, поки коду для ворогів немає
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

            IEntity self = this;
            var otherEntities = panel.EntitiesOnPanel
                .Where(e => e != self && e.GetEntityType != EntityType.Enemy)
                .ToList();

            foreach (var entity in otherEntities)
            {
                switch (entity.GetEntityType)
                {
                    case EntityType.Player:
                        yield return StartCoroutine(TryToStartBattle(this, entity));
                        break;

                    case EntityType.Ally:
                        // код для бою між противником та помічником
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion

        #region Тригери
        /*private void OnTriggerEnter2D(Collider2D collision) // метод при сутичці з іншою сутністю(у даному випадку ворогом або панеллю)
        {
            if (collision.transform.name.StartsWith("Panel"))
            {
                currentPanel = collision.GetComponent<PanelScript>();
            }

            if (collision.transform.CompareTag("Entity"))
            {
                if (leftSteps == 0 && isEventAttack == false)
                {
                    Debug.Log($"Atacking enemy: {this.GetEntityName}");
                    Debug.Log($"LeftSteps: {leftSteps}");
                    return;
                }

                isEventAttack = false;
                Debug.Log($"LeftSteps: {leftSteps}");
                StopMoving();
                // Далі повинен бути код початку бою з ворогом:
                TryToStartBattle(this, collision.GetComponent<BasePlayerController>());
            }
        }*/

        private void SubscribeToClickEvent()
        {
            var clickHandler = GetComponentInChildren<ClickHandler>();
            clickHandler.OnEntityClickEvent += OnEntityClickEvent;
        }

        private void OnEntityClickEvent()
        {
            Debug.Log($"Entity {enemyInfo.CharacterName} being clicked");
            OnSelfClickHandled?.Invoke(this);
        }

        /*private void OnMouseDown()
        {
            Debug.Log($"Entity with name: {this.GetEntityName} was clicked");
            PanelEffectsManager.Instance.TryToChooseEntity(this);
        }*/
        #endregion

        #region Гетери
        public string GetEntityName => enemyInfo.name;
        public virtual EntityType GetEntityType => enemyInfo.EntityType;
        public PanelScript GetCurrentPanel => currentPanel;
        public int GetEntityDef => def;
        public int GetEntityHp => hp;
        public int GetEntityMaxHp => maxHp;
        public int GetEntityMoney => money;
        public int GetEntityAtk => atk;
        public int GetEntityLeftCards => leftCards;
        public bool GetEntityAttackAccess => isEventAttack;
        public bool CanTriggerPanels => canTriggerPanels;
        public string GetEntitySuit => ""; // optional
        public Direction GetEntityDirection => direction;
        #endregion

        #region Службові методи
        public void SetupSpecialAbility(ActiveGlobalEffectInfo activeGlobalEffectInfo = null)
        {
            if (activeGlobalEffectInfo == null && this.enemyInfo.SpecialAbility == null)
                return;

            ActiveGlobalEffectInfo specialAbilityInfo = null;

            if (activeGlobalEffectInfo == null)
                specialAbilityInfo = this.enemyInfo.SpecialAbility;
            else
                specialAbilityInfo = activeGlobalEffectInfo;

            SpecialAbility = BaseActiveGlobalEffect.GetActiveGlobalEffectInstance(specialAbilityInfo, this);
        }

        protected void ManageKnockoutAnimationHandler()
        {
            AnimationClip clip = GetAnimationClipByName(Animator, DEAD_CLIP_NAME);
            if (clip == null)
            {
                Debug.LogWarning($"Клип {DEAD_CLIP_NAME} не найден!");
                return;
            }

            AnimationEvent[] events = clip.events;
            if (events.Length == 0)
            {
                Debug.LogWarning($"Клип {clip.name} не содержит событий!");
                return;
            }

            events[0].functionName = nameof(OnKnockoutAnimationEnd);
            clip.events = events;
        }

        protected AnimationClip GetAnimationClipByName(Animator anim, string name)
        {
            foreach (var clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == name)
                    return clip;
            }
            return null;
        }
        #endregion

        #region TempEnemyActions
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
    }
}