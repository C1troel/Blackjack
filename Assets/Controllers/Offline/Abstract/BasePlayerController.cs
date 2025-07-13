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
    public abstract class BasePlayerController : MonoBehaviour,IEntity
    {
        public event Action<IEntity> moveEndEvent;

        public event Action hpChangeEvent, statsChangeEvent, curencyChangeEvent;

        public Animator anim;

        protected CharacterInfo characterInfo;

        protected Camera playerCamera;

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
            initialZ = transform.position.z; // Сохраняем начальную координату z
            SetupMoveCardsDeckPlayerSuit();
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

        public abstract void ActivateAbility();

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

            anim = gameObject.GetComponent<Animator>();

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

        public void Pay(int value, bool useChips)
        {
            if (useChips)
                chips -= value;
            else
                money -= value;

            CurencyChange();
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
            if (value <= 0)
            {
                hp -= 1;
                return;
            }

            hp -= value;

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

        public virtual void GetSteps(int value)
        {
            leftSteps = value;
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

        public void MoveEnd() => moveEndEvent?.Invoke(this);
        public void HpChange() => hpChangeEvent?.Invoke();
        public void StatsChange() => statsChangeEvent?.Invoke();
        public void CurencyChange() => curencyChangeEvent?.Invoke();

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
            Walk();
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

        private void TryToStartBattle(IEntity Atk, IEntity Def)
        {
            Debug.Log("BattlStart");

            Debug.LogWarning($"ATK: {Atk.GetEntityName} DEF: {Def.GetEntityName}");
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
                TryToStartBattle(this, collision.GetComponent<IEntity>());
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
