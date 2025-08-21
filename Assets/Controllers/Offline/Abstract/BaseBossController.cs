using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Singleplayer
{
    public abstract class BaseBossController : BaseEnemy
    {
        protected virtual int KillersCallAmount => 1;
        protected virtual int BodyguardsCallAmount => 2;
        protected virtual int HenchmensCallResetValue => 3;
        protected virtual int PunishmentConuterResetValue => 10;

        protected int henchmensCallCounter;

        protected int punishmentCounter;
        protected virtual int EffectCardCounterResetValue => 3;
        protected int effectCardCounter;
        protected int pregamePlayerChips;

        protected bool CanPursuit = false;
        protected bool CanSummon = false;

        public override void SetupEnemy(EnemyInfo enemyInfo)
        {
            BlackjackManager.Instance.OnBlackjackGameStart += OnBlackjackGameStart;
            BlackjackManager.Instance.OnBlackjackGameEnd += OnBlackjackGameEnd;
            DealerUIContoller.Instance.dealerInteractionStart += OnDealerInteractionStart;

            base.SetupEnemy(enemyInfo);

            RestoreEffectCards();
        }

        private void OnDealerInteractionStart()
        {
            punishmentCounter++;
        }

        public override void OnNewTurnStart()
        {
            if (punishmentCounter >= PunishmentConuterResetValue)
            {
                CanPursuit = true;
                CanSummon = true;
                punishmentCounter -= PunishmentConuterResetValue;
            }

            effectCardCounter++;

            if (effectCardCounter == EffectCardCounterResetValue)
            {
                effectCardCounter = 0;

                RestoreEffectCards();
            }

            base.OnNewTurnStart();
        }

        public override void StartTurn()
        {
            if (hp == 0)
            {
                Revive();
                TurnManager.Instance.EndTurnRequest(this);
                return;
            }

            StartCoroutine(ProcessBossTurnStart());
        }

        public override void StartMove(Direction direction = Direction.Standart, PanelScript panel = null)
        {
            if (leftSteps == 0)
            {
                MoveEnd();
                return;
            }

            if (CanPursuit)
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

            Debug.Log($"Coroutine Started, StepsLeft {leftSteps}");
            moving = StartCoroutine(Move(this, direction, panel));
            canMove = true;
        }

        protected override IEnumerator TryToStartBattle(IEntity Atk, IEntity Def)
        {
            if (!CanPursuit)
                yield break;

            Debug.Log("BattlStart");

            var battleManager = BattleManager.Instance;
            StopMoving();
            Debug.LogWarning($"ATK: {Atk.GetEntityName} DEF: {Def.GetEntityName}");
            battleManager.TryToStartBattle(Atk, Def);
            yield return new WaitUntil(() => !battleManager.IsBattleActive);
            CanPursuit = false;
            StartMove();
        }

        protected virtual void Revive()
        {
            DeadOff();
            hp = enemyInfo.DefaultHp;
        }

        protected virtual IEnumerator ProcessBossTurnStart()
        {
            if (CanSummon)
            {
                CanSummon = false;
                yield return StartCoroutine(CallHenchmens());
            }

            base.StartTurn();
        }

        protected virtual IEnumerator CallHenchmens()
        {
            var gameManager = GameManager.Instance;
            var mapManager = MapManager.Instance;

            if (henchmensCallCounter == HenchmensCallResetValue)
            {
                henchmensCallCounter = 0;
                for (int i = 0; i < KillersCallAmount; i++)
                {
                    var randomPanel = mapManager.panels[Random.Range(0, mapManager.panels.Count)];
                    yield return StartCoroutine(gameManager.SpawnEnemy(EnemyType.Killer, randomPanel.transform.position));
                }

                yield break;
            }

            henchmensCallCounter++;

            for (int i = 0; i < BodyguardsCallAmount; i++)
            {
                var randomPanel = mapManager.panels[Random.Range(0, mapManager.panels.Count)];
                yield return StartCoroutine(gameManager.SpawnEnemy(EnemyType.Bodyguard, randomPanel.transform.position));
            }

            yield break;
        }

        public override void GetDamage(int value)
        {
            punishmentCounter += 4;
            base.GetDamage(value);
        }

        protected override void Knockout()
        {
            punishmentCounter += 10;

            Dead();
            DropMoney();
        }

        protected override void DropMoney()
        {
            var gameManager = GameManager.Instance;
            int droppedMoneyAmount = (enemyInfo.DefaultHp / 10) + money;

            var droppedMoneyPrefab = gameManager.GetDroppedMoneyPrefab;
            var droppedMoneyGO = Instantiate(droppedMoneyPrefab, currentPanel.transform.position, Quaternion.identity);
            var droppedMoneyHandler = droppedMoneyGO.GetComponent<DroppedMoneyHandler>();
            droppedMoneyHandler.ManageDroppedMoney(droppedMoneyAmount);
            droppedMoneyGO.SetActive(true);
        }

        protected virtual void OnBlackjackGameStart()
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            pregamePlayerChips = player.GetEntityChips;
            player.CurencyChangeEvent += OnPlayerCurrencyChange;
        }

        protected virtual void OnBlackjackGameEnd()
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            player.CurencyChangeEvent -= OnPlayerCurrencyChange;
            pregamePlayerChips = 0;
        }

        protected virtual void OnPlayerCurrencyChange()
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;

            if (player.GetEntityChips == pregamePlayerChips)
                return;

            punishmentCounter += 2;
        }

        protected virtual void RestoreEffectCards()
        {
            foreach (var effectCard in EnemyEffectCardsHandler.effectCardsList.ToList())
                EnemyEffectCardsHandler.RemoveEffectCard(effectCard);

            EffectCardDealer.Instance.DealRandomEffectCard(this);
            /*EffectCardDealer.Instance.DealEffectCardOfType(this, EffectCardType.Hourglass);*/
        }
    }
}