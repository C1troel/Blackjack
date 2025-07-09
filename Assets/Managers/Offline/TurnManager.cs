using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class TurnManager : MonoBehaviour
    {
        private Queue<IEntity> turnQueue = new Queue<IEntity>();
        private IEntity currentTurnEntity;

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
            while (true)
            {
                if (turnQueue.Count == 0) yield break;

                currentTurnEntity = turnQueue.Dequeue();
                Debug.Log($"Now it's {currentTurnEntity.GetEntityName}'s turn");

                isTurnActive = true;

                switch (currentTurnEntity.GetEntityType)
                {
                    case EntityType.Player:

                        GameManager.Instance.StartPlayerTurn(currentTurnEntity as BasePlayerController);
                        // ќч≥куЇмо, поки гравець завершить х≥д
                        yield return new WaitUntil(() => isTurnActive == false);

                        break;

                    case EntityType.Enemy:

                        var enemy = currentTurnEntity as BaseEnemy;
                        enemy.PerformAction();
                        yield return new WaitUntil(() => isTurnActive == false);

                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }

                turnQueue.Enqueue(currentTurnEntity);
            }
        }

        private IEnumerator HandleEnemyTurn(IEntity enemy)
        {
            // “ут лог≥ка дл€ ходу бота
            yield return new WaitForSeconds(1f);
            Debug.Log($"{enemy.GetEntityName} finished turn");
            isTurnActive = false;
        }

        public void EndTurnRequest(IEntity requester)
        {
            if (requester != currentTurnEntity)
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
            if (currentTurnEntity.GetEntityType == EntityType.Player)
            {
                GameManager.Instance.ToggleInputBlock(true);
            }

            isTurnActive = false; // сигнал корутин≥, що можна переходити до наступного
        }
    }
}