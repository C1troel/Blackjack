using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Singleplayer
{
    public abstract class BaseBossController : BaseEnemy
    {
        protected virtual int KillersCallAmount => 1;
        protected virtual int BodyguardsCallAmount => 2;
        protected virtual int HenchmensCallResetValue => 3;
        protected int henchmensCallCounter;

        protected int pursuitCounter;
        protected virtual int EffectCardCounterResetValue => 3;
        protected int effectCardCounter;
        protected int pregamePlayerChips;

        protected bool IsBeingTriggered = false;

        public override void SetupEnemy(EnemyInfo enemyInfo)
        {
            BlackjackManager.Instance.OnBlackjackGameStart += OnBlackjackGameStart;
            BlackjackManager.Instance.OnBlackjackGameEnd += OnBlackjackGameEnd;

            base.SetupEnemy(enemyInfo);

            RestoreEffectCards();
        }

        public override void OnNewTurnStart()
        {
            StartCoroutine(ProcessBossTurnStart());
        }

        public override void StartTurn()
        {
            if (hp == 0)
            {
                Revive();
                TurnManager.Instance.EndTurnRequest(this);
                return;
            }

            base.StartTurn();
        }

        protected virtual void Revive()
        {
            DeadOff();
            hp = enemyInfo.DefaultHp;
        }

        protected virtual IEnumerator ProcessBossTurnStart()
        {
            effectCardCounter++;

            if (effectCardCounter == EffectCardCounterResetValue)
            {
                effectCardCounter = 0;

                RestoreEffectCards();
            }

            if (IsBeingTriggered)
            {
                IsBeingTriggered = false;
                yield return StartCoroutine(CallHenchmens());
            }

            base.OnNewTurnStart();
        }

        protected virtual IEnumerator CallHenchmens()
        {
            var entitySpawnManager = EntitySpawnManager.Instance;
            var mapManager = MapManager.Instance;

            if (henchmensCallCounter == HenchmensCallResetValue)
            {
                for (int i = 0; i < KillersCallAmount; i++)
                {
                    var randomPanel = mapManager.panels[Random.Range(0, mapManager.panels.Count)];
                    var calledKiller = entitySpawnManager.SpawnEnemy(randomPanel.transform.position, EnemyType.Killer);
                    yield return null;
                    calledKiller.SetRandomAvailableDirection();
                }
            }

            for (int i = 0; i < BodyguardsCallAmount; i++)
            {
                var randomPanel = mapManager.panels[Random.Range(0, mapManager.panels.Count)];
                var calledBodyguard = entitySpawnManager.SpawnEnemy(randomPanel.transform.position, EnemyType.Bodyguard);
                yield return null;
                calledBodyguard.SetRandomAvailableDirection();
            }

            yield break;
        }

        public override void GetDamage(int value)
        {
            IsBeingTriggered = true;
            pursuitCounter++;
            base.GetDamage(value);
        }

        protected override void Knockout()
        {
            pursuitCounter += 2;

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

            pursuitCounter++;
            IsBeingTriggered = true;
        }

        protected virtual void RestoreEffectCards()
        {
            foreach (var effectCard in EnemyEffectCardsHandler.effectCardsList)
                EnemyEffectCardsHandler.RemoveEffectCard(effectCard);

            EffectCardDealer.Instance.DealRandomEffectCard(this);
        }
    }
}