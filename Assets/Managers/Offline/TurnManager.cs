using Singleplayer.PassiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class TurnManager : MonoBehaviour
    {
        private Queue<IEntity> turnQueue = new Queue<IEntity>();
        public IEntity CurrentTurnEntity { get; private set; }

        public static TurnManager Instance { get; private set; }

        public event Action OnNewRoundStarted;

        private bool isTurnActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeTurnOrder(List<IEntity> entities)
        {
            turnQueue.Clear();

            foreach (var entity in entities)
            {
                turnQueue.Enqueue(entity);
            }

            StartCoroutine(HandleTurns());
        }

        private IEnumerator HandleTurns()
        {
            yield return null;

            while (true)
            {
                if (turnQueue.Count == 0) yield break;

                CurrentTurnEntity = turnQueue.Dequeue();
                Debug.Log($"Now it's {CurrentTurnEntity.GetEntityName}'s turn");

                isTurnActive = true;

                switch (CurrentTurnEntity.GetEntityType)
                {
                    case EntityType.Player:
                        Debug.Log("=== New Round Started ===");
                        OnNewRoundStarted?.Invoke();

                        StartCoroutine(HandlePlayerTurn());
                        yield return new WaitUntil(() => isTurnActive == false);
                        break;

                    case EntityType.Enemy:
                        StartCoroutine(HandleEnemyTurn());
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

        private IEnumerator HandleEnemyTurn()
        {
            var enemy = CurrentTurnEntity as BaseEnemy;
            enemy.OnNewTurnStart();

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0 
                && !GlobalEffectsManager.Instance.isTimeStopped) yield return null;

            if (IsFrozenDuringTimeStop(enemy))
            {
                HandleTimeStopCounters(enemy);
                yield break;
            }

            enemy.StartTurn();
        }

        private IEnumerator HandlePlayerTurn()
        {
            var player = CurrentTurnEntity as BasePlayerController;
            player.OnNewTurnStart();

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0
                && !GlobalEffectsManager.Instance.isTimeStopped) yield return null;

            if (IsFrozenDuringTimeStop(player))
            {
                HandleTimeStopCounters(player);
                yield break;
            }

            player.StartTurn();
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
            // якщо гравець Ч ховаЇмо HUD
            if (CurrentTurnEntity.GetEntityType == EntityType.Player)
            {
                GameManager.Instance.TogglePlayerHudButtons(false);
            }

            isTurnActive = false; // сигнал корутин≥, що можна переходити до наступного
        }
    }
}