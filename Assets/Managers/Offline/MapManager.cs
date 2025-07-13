using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> panels;
        [SerializeField] private GameObject parentOfAllPanels; // на даний момент використовується для помічення кожної панелі при телепортації
        [SerializeField] private EffectRevealCard usedCard;

        private GameManager gameManager;
        public static MapManager Instance { get; private set; }

        private List<PanelScript> highlightedPathEnders = new List<PanelScript>();

        public event Action<ulong> playerMoveEnd;

        private Coroutine waitingForPlayer;

        public int GetLastPlayerStepsCount { get; private set; }

        public bool IsPossiblePlayerTeleportation { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
        }

        private IEnumerator WaitingForPlayerAction()
        {
            // стандартний вихід з циклу повинен бути реалізований по таймеру!
            while (true)
            {
                Debug.Log("MapManager is waiting for player action end...");
                yield return null;
            }
        }

        #region Звичайні функції

        public void StopWaiting()
        {
            Debug.Log("Stop waiting");

            if (waitingForPlayer == null)
                return;

            StopCoroutine(waitingForPlayer);

            waitingForPlayer = null;
        }

        public List<PanelScript> GetAllPanelsOfType(PanelEffect panelEffect)
        {
            List<PanelScript> resultPanels = new List<PanelScript>();

            var appropriatePanels = panels.FindAll(panel => panel.GetComponent<PanelScript>().GetEffectPanelInfo.effect == panelEffect);

            foreach (var panel in appropriatePanels)
            {
                resultPanels.Add(panel.GetComponent<PanelScript>());
            }

            return resultPanels;
        }

        public void TempResetMapValuesInfo() // дана функція повинна взагалі спрацьовувати, коли передається хід іншому гравцеві ігровим менеджером
        {
            GetLastPlayerStepsCount = 0;
        }

        public void MakeADraw(IEntity entity)
        {
            int steps = UnityEngine.Random.Range(0, 12); // змінна яка повинна використовуватися замість tempSteps

            int tempSteps = 2;

            GetLastPlayerStepsCount += tempSteps;

            Debug.Log($"GetLastPlayerStepsCount is {GetLastPlayerStepsCount}");

            if (GetLastPlayerStepsCount == 21)
            {
                switch (entity.GetEntityType)
                {
                    case EntityType.Player:
                        AccessPlayerToTeleport(entity);
                        break;

                    case EntityType.Enemy:
                        var player = gameManager.GetEntityWithType(EntityType.Player);
                        gameManager.TeleportEntity(((MonoBehaviour)player).transform.position, entity, player.GetCurrentPanel);
                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }

                return;
            }

            switch (entity.GetEntityType)
            {
                case EntityType.Player:

                    HandlePlayerMovement(entity as BasePlayerController);

                    /*entity.GetSteps(tempSteps);
                    PathBuilding(entity.GetCurrentPanel, tempSteps, entity);
                    *//*HighlightReachablePanels(entity.GetCurrentPanel, tempSteps, entity);*//*
                    entity.StartMove();*/
                    break;

                case EntityType.Enemy:
                    entity.GetSteps(tempSteps);
                    /*HandleEnemyMovement(entity as BaseEnemy);*/
                    entity.StartMove();
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }

        private void HandlePlayerMovement(BasePlayerController player)
        {
            var moveCard = player.GetNextMoveCard();

            int choosedPlayerSteps = 0;

            void AwaitPlayerMoveCardChoosing(int choosedSteps)
            {
                choosedPlayerSteps = choosedSteps;
                Debug.Log($"Player choosed: {choosedSteps}");
                player.GetSteps(choosedPlayerSteps);
                PathBuilding(player.GetCurrentPanel, choosedPlayerSteps, player);
                player.StartMove();

                usedCard.MoveCardValueSelectedEvent -= AwaitPlayerMoveCardChoosing;
            }

            usedCard.MoveCardValueSelectedEvent += AwaitPlayerMoveCardChoosing;

            usedCard.RevealMoveCard(moveCard);
        }

        /*private void HandleEnemyMovement(BaseEnemy enemy)
        {
            enemy.StartMoveToPlayer();
        }*/

        private void AccessPlayerToTeleport(IEntity entity)
        {
            IsPossiblePlayerTeleportation = true;
            waitingForPlayer = StartCoroutine(WaitingForPlayerAction());

            AccessPlayerTeleportationClientRpc();
        }

        public void SubscribeToEntityMoveEndEvent(IEntity entity)
        {
            entity.moveEndEvent += OnPlayerMoveEnd;
        }

        /*public void UnsubscribeToPlayerMoveEndEvent(PlayerController player)
        {
            player.moveEndEvent -= OnPlayerMoveEnd;
        }*/

        private void OnPlayerMoveEnd(IEntity entity)
        {
            /*if (playerMoveEnd != null)
            {
                playerMoveEnd(Id);
            }*/

            if (highlightedPathEnders.Count != 0 && entity.GetEntityType == EntityType.Player)
            {
                foreach (var panel in highlightedPathEnders)
                    panel.RemoveHighlight();
            }
        }

        #region Пошуки шляху
        private void PathBuilding(PanelScript startPanel, int stepCount, IEntity entity)
        {
            var pathEnders = new List<PanelScript>();

            var startNearPanels = startPanel.GetAvailableNearPanelsOrNull(entity, null, null, true);

            var alreadyPickedPanels = new List<PanelScript>();

            var passedPanels = new List<PanelScript>(); // спробую сюди ж додавати панелі, які 
            PanelScript passedPanel = new PanelScript();
            PanelScript tempPassedPanel = new PanelScript();

            var nearPanels = new List<PanelScript>();

            if (startNearPanels == null)
            {
                Debug.LogWarning("nearPanels IS NULL!!!");
                return;
            }

            for (int i = 1; i <= stepCount;)
            {
                if (startNearPanels.Count != 0 && i == 1)
                {
                    alreadyPickedPanels.Clear();
                    nearPanels.Clear();

                    passedPanels.Clear();
                    passedPanels.Add(entity.GetCurrentPanel);
                    passedPanels.Add(startNearPanels[0]);

                    passedPanel = entity.GetCurrentPanel;

                    tempPassedPanel = startNearPanels[0];
                    nearPanels.AddRange(startNearPanels[0].GetAvailableNearPanelsOrNull(entity, null, passedPanel));

                    startNearPanels.Remove(startNearPanels[0]);
                    i++;

                    passedPanels.Add(nearPanels[0]);
                    passedPanel = tempPassedPanel;
                    continue;
                }
                else if (startNearPanels.Count == 0 && i == 1) // фікс пошуку шляху(01.07.2025)
                {
                    if (nearPanels.Count != 0)
                    {
                        var accidentallyMissedPanels = nearPanels[0].GetAvailableNearPanelsOrNull(entity, alreadyPickedPanels, tempPassedPanel, true);
                        if (accidentallyMissedPanels.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Log("є пропущені панелі");
                            // нічого
                        }
                    }
                }
                else if (startNearPanels.Count == 0 && i == 0 && passedPanel == entity.GetCurrentPanel) // фікс пошуку шляху(04.07.2025)
                {
                    break;
                }

                if (i == stepCount)
                {
                    pathEnders.Add(nearPanels[0]);

                    if (!alreadyPickedPanels.Contains(nearPanels[0]))
                        alreadyPickedPanels.Add(nearPanels[0]);

                    PanelScript currentPanel = new PanelScript();

                    for (int j = passedPanels.Count - 1; j > 0; --j)
                    {
                        currentPanel = passedPanels[i];
                        passedPanels.RemoveAt(i);
                        --i;

                        if (currentPanel.GetNeighboursCount > 2)
                        {
                            if (i == stepCount - 1)
                            {
                                currentPanel = passedPanels[i];
                                passedPanels.RemoveAt(i);
                                --i;
                                continue;
                            }

                            nearPanels.Clear();
                            nearPanels.Add(currentPanel);
                            break;
                        }

                        nearPanels.Clear();
                        nearPanels.Add(currentPanel);
                    }

                    i++;
                    passedPanels.Add(nearPanels[0]);

                    if (passedPanels.Count > 1)
                    {
                        passedPanel = passedPanels[passedPanels.Count - 2];
                    }
                    else
                    {
                        passedPanel = passedPanels[passedPanels.Count - 1];
                    }

                    continue;
                }

                if (nearPanels.Count > 1)
                {
                    var tempPanel = nearPanels[0];

                    tempPassedPanel = nearPanels[0];
                    nearPanels = (nearPanels[0].GetAvailableNearPanelsOrNull(entity, null, passedPanel));
                    i++;
                    passedPanels.Add(nearPanels[0]);
                    passedPanel = tempPassedPanel;
                }
                else if (nearPanels.Count != 0)
                {
                    int NeighboursCount = nearPanels[0].GetNeighboursCount;

                    tempPassedPanel = nearPanels[0];

                    if (NeighboursCount > 2)
                        nearPanels = nearPanels[0].GetAvailableNearPanelsOrNull(entity, alreadyPickedPanels);
                    else
                        nearPanels = nearPanels[0].GetAvailableNearPanelsOrNull(entity, null, passedPanel);

                    nearPanels.RemoveAll(panel => panel == passedPanel);

                    if (NeighboursCount > 2 && nearPanels.Count != 0)
                        alreadyPickedPanels.Add(nearPanels[0]);

                    if (nearPanels.Count == 0)
                    {
                        PanelScript currentPanel = new PanelScript();

                        for (int j = passedPanels.Count; j > 0; --j)
                        {
                            currentPanel = passedPanels[i];
                            passedPanels.RemoveAt(i);
                            --i;

                            if (currentPanel.GetNeighboursCount > 2)
                            {
                                PanelScript currentPanel2 = new PanelScript();

                                for (int k = passedPanels.Count - 1; k > 0; --k)
                                {
                                    currentPanel2 = passedPanels[i];
                                    passedPanels.RemoveAt(i);

                                    if (currentPanel2.GetNeighboursCount > 2)
                                    {
                                        nearPanels.Clear();
                                        nearPanels.Add(currentPanel2);
                                        --i;
                                        break;
                                    }

                                    nearPanels.Clear();
                                    nearPanels.Add(currentPanel2);
                                    --i;
                                }

                                break;
                            }

                            nearPanels.Clear();
                            nearPanels.Add(currentPanel);
                        }

                        i++;
                        /*if (nearPanels.Count != 0)*/ // не пойдёт
                        passedPanels.Add(nearPanels[0]); 

                        if (passedPanels.Count > 1)
                        {
                            passedPanel = passedPanels[passedPanels.Count - 2];
                        }
                        else
                        {
                            passedPanel = passedPanels[passedPanels.Count - 1];
                        }

                        continue;
                    }

                    i++;
                    passedPanels.Add(nearPanels[0]);
                    passedPanel = tempPassedPanel;
                    continue;
                }
            }

            foreach (var panel in pathEnders)
                panel.HighlightAsPathEnder();

            highlightedPathEnders = pathEnders;
        }

        private void HighlightReachablePanels(PanelScript startPanel, int maxSteps, IEntity entity)
        {
            if (startPanel == null || entity == null || maxSteps <= 0)
                return;

            HashSet<(PanelScript, Direction)> visited = new();
            HashSet<PanelScript> result = new();

            Queue<(PanelScript panel, Direction dir, int stepsLeft)> queue = new();
            Direction startDirection = entity.GetEntityDirection;

            queue.Enqueue((startPanel, startDirection, maxSteps));
            visited.Add((startPanel, startDirection));

            while (queue.Count > 0)
            {
                var (currentPanel, currentDir, stepsLeft) = queue.Dequeue();

                if (stepsLeft == 0)
                {
                    result.Add(currentPanel);
                    continue;
                }

                var forwardPanels = currentPanel.GetAvailableForwardPanels(currentDir);

                foreach (var (neighbor, newDir) in forwardPanels)
                {
                    var key = (neighbor, newDir);
                    if (visited.Contains(key))
                        continue;

                    visited.Add(key);
                    queue.Enqueue((neighbor, newDir, stepsLeft - 1));
                }
            }

            // Подсветка результата
            foreach (var panel in result)
                panel.HighlightAsPathEnder();

            highlightedPathEnders = result.ToList();
        }





        public static int FindDistanceBetweenPanels(PanelScript start, PanelScript target)
        {
            if (start == target)
                return 0;

            Queue<(PanelScript panel, int distance)> queue = new();
            HashSet<PanelScript> visited = new();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (currentPanel, distance) = queue.Dequeue();

                foreach (var neighbor in currentPanel.GetAvailableNearPanelsOrNull(null))
                {
                    if (neighbor == target)
                        return distance + 1;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
                }
            }

            return -1;
        }

        public static List<PanelScript> FindShortestPathConsideringDirection(PanelScript start, PanelScript target, IEntity entity)
        {
            if (start == null || target == null || entity == null)
                return null;

            // Удаляем прежнюю проверку "если панели равны", чтобы можно было найти обходной путь
            // if (start == target) return new List<PanelScript> { start };

            Queue<(PanelScript panel, Direction direction)> queue = new Queue<(PanelScript, Direction)>();
            Dictionary<(PanelScript, Direction), (PanelScript, Direction)> cameFrom = new Dictionary<(PanelScript, Direction), (PanelScript, Direction)>();
            HashSet<(PanelScript, Direction)> visited = new HashSet<(PanelScript, Direction)>();

            Direction startDirection = entity.GetEntityDirection;

            queue.Enqueue((start, startDirection));
            visited.Add((start, startDirection));
            cameFrom[(start, startDirection)] = (null, Direction.Standart);

            while (queue.Count > 0)
            {
                var (currentPanel, currentDirection) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    PanelScript neighbor = currentPanel.GetNeighborByIndex(i);
                    if (neighbor == null)
                        continue;

                    Direction newDirection = (Direction)i;

                    // Запрещаем двигаться назад (в противоположную сторону)
                    if (newDirection == GetOppositeDirection(currentDirection))
                        continue;

                    var state = (neighbor, newDirection);
                    if (visited.Contains(state))
                        continue;

                    visited.Add(state);
                    cameFrom[state] = (currentPanel, currentDirection);
                    queue.Enqueue(state);

                    // Условие выхода: мы пришли на ту же панель, где стоит цель, но с другой стороны
                    if (neighbor == target && neighbor != start)
                    {
                        List<PanelScript> path = new List<PanelScript>();
                        var current = state;

                        while (current.Item1 != null)
                        {
                            path.Add(current.Item1);
                            current = cameFrom[current];
                        }

                        path.Reverse();
                        return path;
                    }
                }
            }

            return null;
        }


        public static Direction GetDirectionFromTo(PanelScript from, PanelScript to)
        {
            for (int i = 0; i < 4; i++)
            {
                var neighbor = from.GetNeighborByIndex(i); // сделай такой метод или напрямую обращайся к _posOfPanels[i]
                if (neighbor == to)
                    return (Direction)i;
            }

            return Direction.Standart;
        }

        public static Direction GetOppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                case Direction.Top: return Direction.Bottom;
                case Direction.Bottom: return Direction.Top;
                default: return Direction.Standart;
            }
        }

        public static List<IEntity> FindEntitiesAtDistance(PanelScript start, int targetDistance)
        {
            List<IEntity> foundEntities = new List<IEntity>();

            if (start == null || targetDistance < 0)
                return foundEntities;

            Queue<(PanelScript panel, int distance)> queue = new();
            HashSet<PanelScript> visited = new();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (currentPanel, distance) = queue.Dequeue();

                if (distance == targetDistance)
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(currentPanel.transform.position, 0.1f);
                    foreach (var collider in colliders)
                    {
                        if (collider.TryGetComponent<IEntity>(out var entity))
                        {
                            if (!foundEntities.Contains(entity))
                                foundEntities.Add(entity);
                        }
                    }

                    continue;
                }

                foreach (var neighbor in currentPanel.GetAvailableNearPanelsOrNull(null))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
                }
            }

            return foundEntities;
        }


        #endregion

        /*private void UseEffect(EffectCardHandler.Effect effect, IEntity entity)
        {
            switch (effect)
            {
                case EffectCardHandler.Effect.DecreaseHP:
                    entity.GetDamage(20);
                    *//*gameManager.UpdateAllPlayersScrollViewInfo();
                    gameManager.UpdatePlayerHUD(entity);*//*
                    break;

                default:
                    break;
            }
        }*/
        #endregion

        /*[ClientRpc]
        private void HighlightPathEndersClientRpc(List<PanelScript> pathEnders)
        {

        }*/

        /*public void UseCardServerRpc(EffectCardHandler.Effect cardEffect, IEntity entity)
        {
            if (entity.GetEntityLeftCards > 0)
            {
                RevealEffectCardClientRpc();
                UseEffect(cardEffect, entity);
            }
        }*/

        public void OnEffectCardPlayed(BaseEffectCard effectCard)
        {
            usedCard.EffectRevealEvent += () => OnEffectCardRevealEnd(effectCard);
            usedCard.RevealEffect(effectCard.GetEffectSprite());
        }

        private void OnEffectCardRevealEnd(BaseEffectCard effectCard)
        {
            usedCard.EffectRevealEvent -= () => OnEffectCardRevealEnd(effectCard);

            effectCard.ApplyEffect();
        }

        private void AccessPlayerTeleportationClientRpc()
        {
            foreach (Transform panel in parentOfAllPanels.transform)
            {
                var effectPanel = panel.GetComponent<PanelScript>();
                effectPanel.HighlightAsPathEnder();
                effectPanel.EnableTeleportation();
            }
        }

        public void ShowUpEventCardClientRpc(string eventName)
        {
            // налаштувати SpriteLoadManager під загрузку спрайту з ім'ям eventName
        }
    }
}
