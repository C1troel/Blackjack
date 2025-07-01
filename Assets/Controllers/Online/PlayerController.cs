using UnityEngine;
using System.Collections;
using Unity.Netcode;
using TMPro;
using Panel;
using System;
using Multiplayer.Panel;

namespace Multiplayer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : NetworkBehaviour
    {
        public enum Direction
        {
            Standart = -1,
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3,
        };

        private int defaultHp = 100;

        public event Action<ulong> moveEndEvent;
        public Animator anim;

        private Camera playerCamera;

        private PanelScript currentPanel;

        private Coroutine moving;

        private ulong Id;

        private int hp;
        private int money;
        private int chips;
        private int atk;
        private int def;
        private int leftSteps;
        private int leftCards;

        private float previousCordY = -1170f;
        private float initialZ;

        private float moveSpeed = 300f;

        private bool canMove = false;
        private bool isEventAttack = false;

        private string cardSuit = "";

        private Direction direction;

        void Start()
        {
            initialZ = transform.position.z; // Сохраняем начальную координату z
        }

        public override void OnNetworkSpawn()
        {
            AssignPrivateVariables();
            /*SetTestName();*/
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

        void Update()
        {
            if (IsServer)
            {
                if (leftSteps > 0 && moving == null && canMove)
                {
                    Debug.Log($"Update Coroutine Started, StepsLeft {leftSteps}");
                    moving = StartCoroutine(Move(this));
                }
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

        public void AddEffectCardsUsages() => ++leftCards;

        public void DecreaseEffectCardsUsages() => --leftCards;

        private void MoveEnd()
        {
            if (moveEndEvent != null)
            {
                moveEndEvent(Id);
            }
        }

        public void StopMoving()
        {
            canMove = false;
            WalkOff();
        }
        /*public void ResumeMove(PanelScript.Pos pos = PanelScript.Pos.)
        {
            canMove = true;
            Walk();
        }*/

        private void AssignPrivateVariables()
        {
            Id = OwnerClientId;
            hp = 100;
            direction = Direction.Right;

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

        /*private void SetTestName()
        {
            var text = playerHUD.transform.Find("RoundCount");
            text.GetComponent<TextMeshProUGUI>().text = $"Player ID: {OwnerClientId}";
        }*/

        /*private void SetupCanvasCamera()
        {
            playerHUD.worldCamera = playerCamera;
            playerHUD.transform.SetParent(null);
        }*/

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsServer)
            {
                if (collision.transform.name.StartsWith("Panel"))
                {
                    currentPanel = collision.GetComponent<PanelScript>();
                }
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

            if (collision.transform.CompareTag("Player"))
            {
                if (!IsServer) // Працює
                    return;

                if (leftSteps == 0 && isEventAttack == false)
                {
                    Debug.Log($"Returning of ID: {this.Id}");
                    Debug.Log($"LeftSteps: {leftSteps} ID: {this.Id}");
                    return;
                }

                isEventAttack = false;
                Debug.Log($"LeftSteps: {leftSteps} ID: {this.Id}");
                StopMoving();
                RequestStartBattleServerRpc(this.Id, collision.GetComponent<PlayerController>().Id);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestStartBattleServerRpc(ulong AtkId, ulong DefId)
        {
            if (IsServer)
            {
                Debug.Log("BattleServerCall");

                Debug.LogWarning($"ATK: {AtkId} DEF: {DefId}");
                BattleManager.Instance.StartBattle(TestPlayerSpawner.Instance.GetPlayerWithId(AtkId), TestPlayerSpawner.Instance.GetPlayerWithId(DefId));
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

        public void Heal(int value)
        {
            if (hp == defaultHp)
                return;

            if ((hp + value) > defaultHp)
            {
                hp = defaultHp;
                return;
            }

            hp += value;
        }

        public PlayerInfo GetPlayerInfo()
        {
            return new PlayerInfo(Id, hp, money, chips, atk, def, GetComponent<Animator>().runtimeAnimatorController.name, cardSuit);
        }

        public void GetSteps(int value)
        {
            leftSteps = value;
        }

        public void StartMove(Direction direction = (Direction)(-1), PanelScript panel = null)
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

        public IEnumerator Move(PlayerController player, Direction direction = (Direction)(-1), PanelScript panel = null)
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

            if (destinationInfo.Item2 == direction)
            {
                destination = destinationInfo.Item1.transform.position;
                Debug.Log("Direction good");
            }
            else
            {
                direction = destinationInfo.Item2;
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

        public void EnableAttacking() => isEventAttack = true;

        /*public void DisableAttacking() => isEventAttack = false;*/

        public ulong GetPlayerId => Id;
        public int GetPlayerHp => hp;
        public int GetPlayerMaxHp => defaultHp;
        public int GetPlayerMoney => money;
        public int GetPlayerChips => chips;
        public int GetPlayerAtk => atk;
        public int GetPlayerDef => def;
        public int GetPlayerLeftCards => leftCards;
        public bool GetPlayerAttackAccess => isEventAttack;
        public string GetPlayerSuit => cardSuit;
        public Direction GetPlayerDirection => direction;
        public PanelScript GetCurrentPanel => currentPanel;
    }

    public struct PlayerInfo : INetworkSerializable
    {
        public ulong Id;

        public int hp;
        public int money;
        public int chips;
        public int atk;
        public int def;
        public string cardSuit;

        public string contorllerName;

        public PlayerInfo(ulong id, int hp, int money, int chips, int atk, int def, string contorllerName, string cardSuit)
        {
            Id = id;
            this.hp = hp;
            this.money = money;
            this.chips = chips;
            this.atk = atk;
            this.def = def;
            this.contorllerName = contorllerName;
            this.cardSuit = cardSuit;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref hp);
            serializer.SerializeValue(ref money);
            serializer.SerializeValue(ref chips);
            serializer.SerializeValue(ref atk);
            serializer.SerializeValue(ref def);
        }
    }
}