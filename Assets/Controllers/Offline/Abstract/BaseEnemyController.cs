using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public abstract class BaseEnemy : MonoBehaviour, IEntity
    {
        protected int hp, maxHp, atk, def, money, leftCards, defaultCardUsages, leftSteps, currentMaxHp;
        protected float moveSpeed = 300f; // �������!(�������� ���������� � �������)
        protected float previousCordY = -1170f; // �������!
        protected float initialZ;

        protected bool isEventAttack, isBoss, canTriggerPanels, canMove;

        protected Direction direction;

        protected PanelScript currentPanel;
        protected Coroutine moving;
        protected EnemyInfo enemyInfo;
        protected List<IEffectCardLogic> effectCardsList;

        public event Action<IEntity> moveEndEvent;
        public Animator animator;

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
            currentMaxHp = enemyInfo.DefaultHp;
            money = enemyInfo.DefaultMoney;
            atk = enemyInfo.DefaultAtk;
            def = enemyInfo.DefaultDef;
            defaultCardUsages = enemyInfo.DefaultCardUsages;

            animator = gameObject.GetComponent<Animator>();
            
            direction = Direction.Right; // Left
            //direction = ??? // ��� ��� ���������� ������� �������� ���� ���� ������ ������
        }

        #region ز ����������

        public virtual void PerformAction()
        {
            
        }

        #endregion

        #region ���������� �� ����
        public virtual void GetSteps(int value) => leftSteps = value;

        public virtual void StartMove(Direction direction = Direction.Standart, PanelScript panel = null)
        {
            if (!isBoss)
                StartMoveToPlayer();

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

            #region DebugInfo

            Debug.Log("Found enemy path to player:");
            foreach (var panel in foundPathToPlayer)
            {
                Debug.Log(panel.name);
            }

            #endregion

            moving = StartCoroutine(MoveToPlayer(foundPathToPlayer));
            canMove = true;
            Walk();
        }

        public virtual IEnumerator MoveToPlayer(List<PanelScript> pathToPlayer)
        {
            Walk();

            foreach (var panel in pathToPlayer)
            {
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

                Debug.Log($"Direction before stand on panel {panel.name} is {this.direction}");
                this.direction = MapManager.GetDirectionFromTo(currentStayedPanel, panel);
                this.currentPanel = panel;
                Debug.Log($"Direction after stand on panel {panel.name} is {this.direction}");

                leftSteps -= 1;
                Debug.LogWarning($"Left Steps: {leftSteps}");

                if (leftSteps == 0)
                    break;
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

        public virtual void TurnEntity()
        {
            Debug.Log("Turning...");
        }

        public void MoveEnd() => moveEndEvent?.Invoke(this);
        #endregion

        #region ����� �� �������
        public virtual void GetDamage(int value)
        {
            if (value <= 0)
            {
                hp -= 1;
                return;
            }

            hp -= value;
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
        }

        public void EnableAttacking() => isEventAttack = true;
        #endregion

        #region ���������� � ��������� �������
        public void ResetEffectCardsUsages() => leftCards = enemyInfo.DefaultCardUsages;
        public void DecreaseEffectCardsUsages() => --leftCards;

        public void ReceiveEffectCard(IEffectCardLogic card)
        {
            if (effectCardsList.Count == enemyInfo.MaxEffectCards)
            {
                Debug.Log($"Cannot add more cards for entity with name: {enemyInfo.CharacterName}" +
                    $", cuz maxEffectCards are {enemyInfo.MaxEffectCards} and now he has {effectCardsList.Count}");

                return;
            }

            effectCardsList.Add(card);
        }
        #endregion

        #region ������� � ����� �������
        private void TryToStartBattle(IEntity Atk, IEntity Def) // PlayerController Def - �� ��������, ���� ���� ��� ������ ����
        {
            /// ��� ��� �������� ������� �����
            Debug.Log("BattlStart");

            Debug.LogWarning($"ATK: {Atk.GetEntityName} DEF: {Def.GetEntityName}");
            BattleManager.Instance.StartBattle(Atk, Def);
        }

        #endregion

        #region �������
        private void OnTriggerEnter2D(Collider2D collision) // ����� ��� ������� � ����� �������(� ������ ������� ������� ��� �������)
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
                // ��� ������� ���� ��� ������� ��� � �������:
                TryToStartBattle(this, collision.GetComponent<BasePlayerController>());
            }
        }

        private void OnMouseDown()
        {
            Debug.Log($"Entity with name: {this.GetEntityName} was clicked");
            PanelEffectsManager.Instance.TryToChooseEntity(this);
        }
        #endregion

        #region ������
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

        #region TempEnemyActions
        public void Jump()
        {
            animator.SetBool("Jump", true);
        }

        public void JumpOff()
        {
            animator.SetBool("Jump", false);
        }

        public void Dead()
        {
            animator.SetBool("Dead", true);
        }

        public void DeadOff()
        {
            animator.SetBool("Dead", false);
        }
        public void Walk()
        {
            animator.SetBool("Walk", true);
        }

        public void WalkOff()
        {
            animator.SetBool("Walk", false);
        }
        public void Run()
        {
            animator.SetBool("Run", true);
        }
        public void RunOff()
        {
            animator.SetBool("Run", false);
        }
        public void Attack()
        {
            animator.SetBool("Attack", true);
        }
        public void AttackOff()
        {
            animator.SetBool("Attack", false);
        }
        #endregion
    }
}