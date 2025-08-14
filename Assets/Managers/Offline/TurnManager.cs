using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class TurnManager : MonoBehaviour
    {
        private Queue<IEntity> turnQueue = new Queue<IEntity>();
        private bool isTurnActive = false;
        private Coroutine currentTurnHandling;
        public IEntity CurrentTurnEntity { get; private set; }

        public event Action OnNewRoundStarted;
        public int CurrentRound {  get; private set; }

        public static TurnManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GameManager.Instance.OnEntityListChange += OnEntitiesListChange;
        }

        public void InitializeTurnOrder()
        {
            StartCoroutine(HandleTurns());
        }

        private IEnumerator HandleTurns()
        {
            yield return null;

            while (true)
            {
                yield return null;
                if (turnQueue.Count == 0) yield break;

                if (BattleManager.Instance.IsBattleActive)
                    yield return null;

                CurrentTurnEntity = turnQueue.Dequeue();
                Debug.Log($"Now it's {CurrentTurnEntity.GetEntityName}'s turn");

                isTurnActive = true;

                switch (CurrentTurnEntity.GetEntityType)
                {
                    case EntityType.Player:
                        Debug.Log("=== New Round Started ===");
                        OnNewRoundStart();

                        currentTurnHandling = StartCoroutine(HandlePlayerTurn());
                        yield return new WaitUntil(() => currentTurnHandling == null);
                        yield return new WaitUntil(() => isTurnActive == false);
                        break;

                    case EntityType.Enemy:
                        currentTurnHandling = StartCoroutine(HandleEnemyTurn());
                        Debug.Log(currentTurnHandling == null ? "Coroutine is null" : "Coroutine started");
                        yield return new WaitUntil(() => currentTurnHandling == null);
                        yield return new WaitUntil(() => isTurnActive == false);
                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }

                turnQueue.Enqueue(CurrentTurnEntity);
            }
        }

        private void OnEntitiesListChange(IEntity entity)
        {
            var currentEntitiesList = GameManager.Instance.GetEntitiesList();

            if (currentEntitiesList.Contains(entity))
                turnQueue.Enqueue(entity);
            else
            {
                turnQueue = new Queue<IEntity>(turnQueue.Where(e => e != entity));

                if (CurrentTurnEntity == entity)
                    EndTurnRequest(entity);
            }
        }

        private void OnNewRoundStart()
        {
            CurrentRound++;
            OnNewRoundStarted?.Invoke();
        }

        private IEnumerator HandleEnemyTurn()
        {
            yield return null;

            if (!isTurnActive) yield break;

            var enemy = CurrentTurnEntity as BaseEnemy;
            enemy.OnNewTurnStart();

            if (!isTurnActive) yield break;

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0
                   && !GlobalEffectsManager.Instance.isTimeStopped
                   && isTurnActive)
            {
                yield return null;
            }

            if (!isTurnActive) yield break;

            if (IsFrozenDuringTimeStop(enemy))
            {
                HandleTimeStopCounters(enemy);
                yield break;
            }

            if (!isTurnActive) yield break;

            enemy.StartTurn();

            while (isTurnActive)
                yield return null;
        }

        private IEnumerator HandlePlayerTurn()
        {
            yield return null;

            if (!isTurnActive) yield break;

            var player = CurrentTurnEntity as BasePlayerController;
            player.OnNewTurnStart();

            if (!isTurnActive) yield break;

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0
                   && !GlobalEffectsManager.Instance.isTimeStopped
                   && isTurnActive)
            {
                yield return null;
            }

            if (!isTurnActive) yield break;

            if (IsFrozenDuringTimeStop(player))
            {
                HandleTimeStopCounters(player);
                yield break;
            }

            if (!isTurnActive) yield break;

            player.StartTurn();

            while (isTurnActive)
                yield return null;
        }

        private void HandleTimeStopCounters(IEntity entity)
        {
            List<IEffectCardLogic> possibleCounterCards = null;

            switch (entity.GetEntityType)
            {
                case EntityType.Player:
                    var player = entity as BasePlayerController;
                    possibleCounterCards = player.EffectCardsHandler.GetCounterCards(PassiveEffectType.TimeStop);
                    break;

                case EntityType.Enemy:
                    var enemy = entity as BaseEnemy;
                    possibleCounterCards = enemy.EnemyEffectCardsHandler.GetCounterCards(PassiveEffectType.TimeStop);
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            if (possibleCounterCards == null || possibleCounterCards.Count == 0)
                EndTurnRequest(entity);
            else
            {
                switch (entity.GetEntityType)
                {
                    case EntityType.Player:
                        var player = entity as BasePlayerController;
                        player.ShowCounterCardOptions(possibleCounterCards, OnCounterCardUsed);
                        break;

                    case EntityType.Enemy:
                        HandleEnemyCounterCardUsage(entity, possibleCounterCards[0]);
                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }
            }
        }

        private void HandleEnemyCounterCardUsage(IEntity entity, IEffectCardLogic effectCard)
        {
            var enemy = entity as BaseEnemy;

            effectCard.TryToUseCard(isUsed =>
            {
                enemy.EnemyEffectCardsHandler.RemoveEffectCard(effectCard);
                OnCounterCardUsed(effectCard);
            }, enemy);
        }

        private void OnCounterCardUsed(IEffectCardLogic usedEddectCard)
        {
            if (usedEddectCard == null)
            {
                Debug.Log("Player is skipped countering incoming projectile");
                EndTurnRequest(CurrentTurnEntity);
                return;
            }

            CurrentTurnEntity.StartTurn();
        }

        private bool IsFrozenDuringTimeStop(IEntity entity)
        {
            return GlobalEffectsManager.Instance.isTimeStopped &&
                !entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Chronomaster);
        }

        public void EndTurnRequest(IEntity requester)
        {
            if (requester != CurrentTurnEntity)
            {
                Debug.LogWarning($"Entity {requester.GetEntityName} tried to end turn, but it's not their turn!");
                return;
            }

            Debug.Log($"Turn ended for {requester.GetEntityName}");
            EndTurn(); // внутр≥шн≥й метод, €кий переключаЇ х≥д
        }

        private void EndTurn()
        {
            isTurnActive = false;
            // якщо гравець Ч ховаЇмо HUD
            if (CurrentTurnEntity.GetEntityType == EntityType.Player)
            {
                GameManager.Instance.TogglePlayerHudButtons(false);
            }

            if (currentTurnHandling != null)
            {
                Debug.Log("currentTurnHandling is active");
                StopCoroutine(currentTurnHandling);
            }

            currentTurnHandling = null; // сигнал корутин≥, що можна переходити до наступного
        }
    }
}