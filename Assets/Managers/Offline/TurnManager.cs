using Singleplayer.PassiveEffects;
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

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0) yield return null;

            if (IsFrozenDuringTimeStop(enemy))
            {
                EndTurnRequest(enemy);
                yield break;
            }

            enemy.StartTurn();
        }

        private IEnumerator HandlePlayerTurn()
        {
            var player = CurrentTurnEntity as BasePlayerController;
            player.OnNewTurnStart();

            while (ProjectileManager.Instance.avaitingProjectiles.Count > 0) yield return null;

            if (IsFrozenDuringTimeStop(player))
            {
                EndTurnRequest(player);
                yield break;
            }

            player.StartTurn();
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