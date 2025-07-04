using Singleplayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class PlayerController : MonoBehaviour,IEntity
    {

        public event Action<IEntity> moveEndEvent;
        public Animator anim;

        private CharacterInfo characterInfo;

        private Camera playerCamera;

        private PanelScript currentPanel; // потрібна реалізація PanelScript, тільки офлайнова

        private Coroutine moving;

        private int hp;
        private int currentMaxHp;
        private int money;
        private int chips;
        private int atk;
        private int def;
        private int leftSteps;
        private int leftCards;
        private int defaultCardUsages;

        private float previousCordY = -1170f;
        private float initialZ;

        private float moveSpeed = 300f;

        private bool canMove = false;
        private bool isEventAttack = false;

        private string cardSuit = "";
        private string characterName = "";

        private Direction direction;

        void Start()
        {
            initialZ = transform.position.z; // Сохраняем начальную координату z
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

        #region PlayerActions
        public void Jump()
        {
            anim.SetBool("Jump", true);
        }

        public void JumpOff()
        {
            anim.SetBool("Jump", false);
        }

        public void Dead()
        {
            anim.SetBool("Dead", true);
        }

        public void DeadOff()
        {
            anim.SetBool("Dead", false);
        }
        public void Walk()
        {
            anim.SetBool("Walk", true);
        }

        public void WalkOff()
        {
            anim.SetBool("Walk", false);
        }
        public void Run()
        {
            anim.SetBool("Run", true);
        }
        public void RunOff()
        {
            anim.SetBool("Run", false);
        }
        public void Attack()
        {
            anim.SetBool("Attack", true);
        }
        public void AttackOff()
        {
            anim.SetBool("Attack", false);
        }
        #endregion

        public void SetupPlayer(string characterName)
        {
            characterInfo = InfosLoadManager.Instance.GetCharacterInfo(characterName);

            hp = characterInfo.DefaultHp;
            money = characterInfo.DefaultMoney;
            chips = characterInfo.DefaultChips;
            atk = characterInfo.DefaultAtk;
            def = characterInfo.DefaultDef;
            defaultCardUsages = characterInfo.DefaultCardUsages;
            direction = Direction.Right;
            //direction = ??? // код для визначення можливої траекторії руху після спавну гравця

            foreach (Transform child in transform)
            {
                switch (child.tag)
                {
                    case "Camera":
                        playerCamera = child.GetComponent<Camera>();
                        break;

                    /*case "HUD":
                        playerHUD = child.GetComponent<Canvas>();
                        break;*/

                    default:
                        break;
                }
            }
        }

        public void GetDamage(int value)
        {
            if (value <= 0)
            {
                hp -= 1;
                return;
            }

            hp -= value;
        }

        public void GetSteps(int value)
        {
            leftSteps = value;
        }

        public void Heal(int value)
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

        public IEnumerator Move(IEntity player, Direction direction = Direction.Standart, PanelScript panel = null)
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
                destinationInfo = currentPanel.GetNextPanelOrNull(player);

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
            leftSteps -= 1;

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

        public void MoveEnd()
        {
            if (moveEndEvent != null)
            {
                moveEndEvent(this);
            }
        }

        public void StartMove(Direction direction = Direction.Standart, PanelScript panel = null)
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
            Walk();
        }

        public void StopMoving()
        {
            canMove = false;
            WalkOff();
        }

        public void TurnEntity()
        {
            Debug.Log("Turning...");
        }

        private void StartBattle(IEntity Atk, IEntity Def) // PlayerController Def - це заглушка, поки коду для ворогів немає
        {
            Debug.Log("BattlStart");

            Debug.LogWarning($"ATK: {Atk.GetEntityName} DEF: {Def.GetEntityName}");
            // Потрібно написати менеджер битви для одиночної гри:
            BattleManager.Instance.StartBattle(Atk, Def);
        }

        private void OnTriggerEnter2D(Collider2D collision) // метод при сутичці з іншою сутністю(у даному випадку ворогом або панеллю)
        {
            if (collision.transform.name.StartsWith("Panel"))
            {
                currentPanel = collision.GetComponent<PanelScript>();
            }
            /*else
            {
                if (collision.transform.CompareTag("Player"))
                {
                    if (leftSteps == 0)
                    {
                        Debug.Log($"Returning of ID: {this.Id}");
                        Debug.Log($"LeftSteps: {leftSteps} ID: {this.Id}");
                        return;
                    }

                    Debug.Log($"LeftSteps: {leftSteps} ID: {this.Id}");
                    StopMoving();
                    RequestStartBattleServerRpc(this.Id, collision.GetComponent<PlayerController>().Id);
                }
            }*/

            if (collision.transform.CompareTag("Entity"))
            {
                if (leftSteps == 0 && isEventAttack == false)
                {
                    Debug.Log($"Returning of ID: {this.characterName}");
                    Debug.Log($"LeftSteps: {leftSteps}");
                    return;
                }

                isEventAttack = false;
                Debug.Log($"LeftSteps: {leftSteps}");
                StopMoving();
                // Далі повинен бути код початку бою з ворогом:
                StartBattle(this, collision.GetComponent<IEntity>());
            }
        }

        public void EnableAttacking() => isEventAttack = true;

        public void ResetEffectCardsUsages() => leftCards = characterInfo.DefaultCardUsages;

        public void DecreaseEffectCardsUsages() => --leftCards;

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
        public bool GetEntityAttackAccess => isEventAttack;
        public string GetEntitySuit => cardSuit;
        public Direction GetEntityDirection => direction;
    }
}
