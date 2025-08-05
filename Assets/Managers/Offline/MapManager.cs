using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private GameObject parentOfAllPanels; // на даний момент використовується для помічення кожної панелі при телепортації
        [SerializeField] private EffectRevealCard usedCard;

        private GameManager gameManager;

        private List<PanelScript> highlightedPathEnders = new List<PanelScript>();
        private Action currentEffectRevealHandler;

        private Coroutine waitingForPlayer;

        private List<int> stepsDrawnThisTurn = new();

        public event Action<ulong> playerMoveEnd;
        public event Action<IEffectCardLogic> OnEffectCardPlayedEvent;

        public List<PanelScript> panels { get; private set; }
        public bool IsPossiblePlayerTeleportation { get; private set; } = false;
        public static MapManager Instance { get; private set; }

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

        public void RandomlyAssignEffectPanels()
        {
            var allPanels = parentOfAllPanels.GetComponentsInChildren<PanelScript>(includeInactive: true).ToList();
            var totalPanelCount = allPanels.Count;

            var allEffectInfos = InfosLoadManager.Instance.GetAllEffectPanlesInfo().ToList();

            List<EffectPanelInfoSingleplayer> chosenEffects = new();

            foreach (var info in allEffectInfos)
            {
                for (int i = 0; i < info.Amount; i++)
                {
                    chosenEffects.Add(info);
                }
            }

            if (chosenEffects.Count > totalPanelCount)
            {
                Debug.LogError("Not enough panels on the map to satisfy all minimum Amount requirements.");
                return;
            }

            int remaining = totalPanelCount - chosenEffects.Count;
            var allowedDuplicates = allEffectInfos.Where(e => e.AllowDuplicates).ToList();

            for (int i = 0; i < remaining; i++)
            {
                if (allowedDuplicates.Count == 0)
                {
                    Debug.LogWarning("No effects allow duplicates, leaving remaining panels empty.");
                    break;
                }

                var randomEffect = allowedDuplicates[Random.Range(0, allowedDuplicates.Count)];
                chosenEffects.Add(randomEffect);
            }

            Shuffle(chosenEffects);

            for (int i = 0; i < allPanels.Count; i++)
            {
                allPanels[i].AttachPanelEffect(chosenEffects[i]);
            }

            panels = allPanels;

            Debug.Log("Panels successfully randomized and initialized.");
            GameManager.Instance.StartGame();
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public List<PanelScript> GetAllPanelsOfType(PanelEffect panelEffect)
        {
            List<PanelScript> resultPanels = new List<PanelScript>();

            var appropriatePanels = panels.FindAll(panel => panel.GetEffectPanelInfo.Effect == panelEffect);

            foreach (var panel in appropriatePanels)
            {
                resultPanels.Add(panel.GetComponent<PanelScript>());
            }

            return resultPanels;
        }

        public void TempResetMapValuesInfo() // дана функція повинна взагалі спрацьовувати, коли передається хід іншому гравцеві ігровим менеджером
        {
            stepsDrawnThisTurn.Clear();
        }

        public void MakeADraw(IEntity entity)
        {
            int tempSteps = 1;

            switch (entity.GetEntityType)
            {
                case EntityType.Player:

                    /*HandlePlayerMovement(entity as BasePlayerController);*/

                    entity.GetSteps(tempSteps);
                    //PathBuilding(entity.GetCurrentPanel, tempSteps, entity);
                    HighlightReachablePanels(entity.GetCurrentPanel, tempSteps, entity);
                    entity.StartMove();
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
                usedCard.MoveCardValueSelectedEvent -= AwaitPlayerMoveCardChoosing;
                choosedPlayerSteps = choosedSteps;
                Debug.Log($"Player choosed: {choosedSteps}");

                stepsDrawnThisTurn.Add(choosedPlayerSteps);

                if (stepsDrawnThisTurn.Count == 2 && stepsDrawnThisTurn.Sum() == 21)
                {
                    EnableTeleportMode(player);
                    return;
                }

                player.GetSteps(choosedPlayerSteps);
                PathBuilding(player.GetCurrentPanel, choosedPlayerSteps, player);
                player.StartMove();
            }

            usedCard.MoveCardValueSelectedEvent += AwaitPlayerMoveCardChoosing;

            usedCard.RevealMoveCard(moveCard);
        }

        private void EnableTeleportMode(BasePlayerController player)
        {
            var OutlinedPanels = new List<PanelScript>();

            foreach (Transform panelTransform in parentOfAllPanels.transform)
            {
                var panelScript = panelTransform.GetComponent<PanelScript>();
                if (panelScript == null)
                    continue;

                panelScript.OnPanelClicked += OnTeleportPanelClicked;
                OutlinedPanels.Add(panelScript);

                panelScript.SetOutline(true);
            }

            void OnTeleportPanelClicked(PanelScript clickedPanel)
            {
                foreach (var panel in OutlinedPanels)
                {
                    panel.OnPanelClicked -= OnTeleportPanelClicked;
                    panel.SetOutline(false);
                }
                OutlinedPanels.Clear();

                GameManager.Instance.TeleportEntity(clickedPanel.transform.position, player, clickedPanel);
            }
        }

        private void HandleEnemyMovement(BaseEnemy enemy)
        {
            List<Sprite> tempBasicCardsList = new List<Sprite>(GameManager.Instance.BasicCardsList);

            Regex regex = new Regex(@"(11|12|13)$");
            tempBasicCardsList.RemoveAll(card => regex.IsMatch(card.name));

            var randomDrawedCard = tempBasicCardsList[Random.Range(0, tempBasicCardsList.Count)];
            MoveCard enemyMoveCard = new MoveCard(randomDrawedCard);

            void AwaitEnemyMoveCardRevealEnd(int steps)
            {
                usedCard.MoveCardValueSelectedEvent -= AwaitEnemyMoveCardRevealEnd;
                enemy.GetSteps(steps);
                enemy.StartMove();
            }

            usedCard.MoveCardValueSelectedEvent += AwaitEnemyMoveCardRevealEnd;
            usedCard.RevealMoveCard(enemyMoveCard);
        }

        private void AccessPlayerToTeleport(IEntity entity)
        {
            IsPossiblePlayerTeleportation = true;

            AccessPlayerTeleportation();
        }

        public void SubscribeToEntityMoveEndEvent(IEntity entity)
        {
            entity.moveEndEvent += OnPlayerMoveEnd;
        }

        public void UnsubscribeToPlayerMoveEndEvent(IEntity entity)
        {
            entity.moveEndEvent -= OnPlayerMoveEnd;
        }

        private void OnPlayerMoveEnd(IEntity entity)
        {
            if (highlightedPathEnders.Count != 0 && entity.GetEntityType == EntityType.Player)
            {
                foreach (var panel in highlightedPathEnders)
                    panel.RemoveHighlight();
            }
        }

        public void StartChoosingPanel(Action<PanelScript> callback, List<PanelScript> allowedPanels = null)
        {
            Debug.Log("Start choosing panel");

            PanelScript chosenPanel = null;

            allowedPanels ??= GetAllPanelsOnMap();

            foreach (var panel in allowedPanels)
            {
                panel.OnPanelClicked += OnPanelChosen;
                panel.SetOutline(true);
            }

            void OnPanelChosen(PanelScript panel)
            {
                chosenPanel = panel;

                foreach (var p in allowedPanels)
                {
                    p.OnPanelClicked -= OnPanelChosen;
                    p.SetOutline(false);
                }

                callback?.Invoke(panel);
            }
        }

        private List<PanelScript> GetAllPanelsOnMap()
        {
            List<PanelScript> panels = new List<PanelScript>();
            foreach (Transform child in parentOfAllPanels.transform)
            {
                var panel = child.GetComponent<PanelScript>();
                if (panel != null)
                    panels.Add(panel);
            }
            return panels;
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
                            Application.Quit();
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

        private void HighlightReachablePanels(PanelScript startPanel, int stepCount, IEntity entity)
        {
            // Если количество шагов = 0, текущая панель и есть конечная точка
            if (stepCount == 0)
            {
                startPanel.HighlightAsPathEnder();
                highlightedPathEnders = new List<PanelScript> { startPanel };
                return;
            }

            // Для отслеживания посещённых состояний: (панель, предыдущая панель, шаг)
            var visited = new HashSet<(PanelScript, PanelScript, int)>();
            var endPanels = new HashSet<PanelScript>();
            var queue = new Queue<(PanelScript current, PanelScript prev, int steps)>();

            // Начальное состояние
            queue.Enqueue((startPanel, null, 0));
            visited.Add((startPanel, null, 0));

            while (queue.Count > 0)
            {
                var (current, prev, steps) = queue.Dequeue();

                // Если достигли нужного количества шагов - фиксируем конечную панель
                if (steps == stepCount)
                {
                    endPanels.Add(current);
                    continue;
                }

                // Получаем доступных соседей с учётом направления
                List<PanelScript> neighbors;
                if (steps == 0) // Первый шаг: учитываем направление движения
                {
                    neighbors = current.GetAvailableNearPanelsOrNull(
                        entity,
                        null,
                        null,
                        true
                    );
                }
                else // Последующие шаги: исключаем возврат
                {
                    neighbors = current.GetAvailableNearPanelsOrNull(
                        entity,
                        null,
                        prev,
                        false
                    );
                }

                // Обрабатываем всех доступных соседей
                foreach (var neighbor in neighbors)
                {
                    // Пропускаем нулевые и уже посещённые состояния
                    if (neighbor == null) continue;

                    var state = (neighbor, current, steps + 1);
                    if (visited.Contains(state)) continue;

                    visited.Add(state);
                    queue.Enqueue(state);
                }
            }

            // Подсвечиваем конечные точки
            foreach (var panel in endPanels)
            {
                panel.HighlightAsPathEnder();
            }

            highlightedPathEnders = endPanels.ToList();
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

            bool ignoreOppositeOnStart = entity.IgnoreDirectionOnce;
            entity.IgnoreDirectionOnce = false;

            Queue<((PanelScript panel, Direction direction) state, bool ignoreOppositeOnce)> queue =
                new Queue<((PanelScript, Direction), bool)>();

            Dictionary<(PanelScript, Direction), (PanelScript, Direction)> cameFrom =
                new Dictionary<(PanelScript, Direction), (PanelScript, Direction)>();

            HashSet<(PanelScript, Direction)> visited = new HashSet<(PanelScript, Direction)>();

            Direction startDirection = entity.GetEntityDirection;
            var startState = (start, startDirection);

            queue.Enqueue((startState, ignoreOppositeOnStart));
            visited.Add(startState);
            cameFrom[startState] = (null, Direction.Standart);

            while (queue.Count > 0)
            {
                var ((currentPanel, currentDirection), ignoreOpposite) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    PanelScript neighbor = currentPanel.GetNeighborByIndex(i);
                    if (neighbor == null)
                        continue;

                    Direction newDirection = (Direction)i;

                    if (!ignoreOpposite && newDirection == GetOppositeDirection(currentDirection))
                        continue;

                    var nextState = (neighbor, newDirection);
                    if (visited.Contains(nextState))
                        continue;

                    visited.Add(nextState);
                    cameFrom[nextState] = (currentPanel, currentDirection);
                    queue.Enqueue((nextState, false));

                    if (neighbor == target && neighbor != start)
                    {
                        List<PanelScript> path = new List<PanelScript>();
                        var current = nextState;

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

            foreach (var kvp in cameFrom.Keys)
            {
                if (kvp.Item1 == target && kvp.Item1 == start)
                {
                    List<PanelScript> fallbackPath = new List<PanelScript>();
                    var current = kvp;

                    while (current.Item1 != null)
                    {
                        fallbackPath.Add(current.Item1);
                        current = cameFrom[current];
                    }

                    fallbackPath.Reverse();

                    if (fallbackPath.Count > 1)
                        return fallbackPath;
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
            List<IEntity> foundEntities = new();

            if (start == null || targetDistance < 0)
                return foundEntities;

            Queue<(PanelScript panel, int distance)> queue = new();
            HashSet<PanelScript> visited = new();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (currentPanel, distance) = queue.Dequeue();

                if (distance > targetDistance)
                    continue;

                Collider2D[] colliders = Physics2D.OverlapCircleAll(currentPanel.transform.position, 0.1f);
                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent<IEntity>(out var entity))
                    {
                        if (!foundEntities.Contains(entity))
                            foundEntities.Add(entity);
                    }
                }

                if (distance < targetDistance)
                {
                    foreach (var neighbor in currentPanel.GetAvailableNearPanelsOrNull(null))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, distance + 1));
                        }
                    }
                }
            }

            return foundEntities;
        }

        public static List<PanelScript> GetPanelsInRadius(PanelScript start, int radius)
        {
            List<PanelScript> panelsInRadius = new List<PanelScript>();

            if (start == null || radius < 0)
                return panelsInRadius;

            Queue<(PanelScript panel, int distance)> queue = new Queue<(PanelScript, int)>();
            HashSet<PanelScript> visited = new HashSet<PanelScript>();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (currentPanel, distance) = queue.Dequeue();

                if (distance > radius)
                    continue;

                panelsInRadius.Add(currentPanel);

                if (distance < radius)
                {
                    var neighbors = currentPanel.GetAvailableNearPanelsOrNull(null);
                    if (neighbors == null)
                        continue;

                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, distance + 1));
                        }
                    }
                }
            }

            return panelsInRadius;
        }


        #endregion

        public void OnEffectCardPlayed(BaseEffectCard effectCard)
        {
            if (BattleManager.Instance.isBattleActive)
            {
                effectCard.ApplyEffect();
                return;
            }

            currentEffectRevealHandler = () => OnEffectCardRevealEnd(effectCard);
            usedCard.EffectRevealEvent += currentEffectRevealHandler;

            if (effectCard != null)
                usedCard.RevealEffect(effectCard.GetEffectSprite());
        }

        private void OnEffectCardRevealEnd(BaseEffectCard effectCard)
        {
            if (currentEffectRevealHandler != null)
                usedCard.EffectRevealEvent -= currentEffectRevealHandler;

            OnEffectCardPlayedEvent?.Invoke(effectCard.EffectCardLogic);
            currentEffectRevealHandler = null;
            effectCard.ApplyEffect();
        }

        public void OnEffectCardPlayedByEntity(Action onCardRevealed, BaseEffectCardLogic effectCardLogic)
        {
            currentEffectRevealHandler = () => OnEntityEffectCardRevealEnd(onCardRevealed, effectCardLogic);
            usedCard.EffectRevealEvent += currentEffectRevealHandler;

            if (effectCardLogic != null)
                usedCard.RevealEffect(effectCardLogic.EffectCardInfo.EffectCardSprite);
        }

        private void OnEntityEffectCardRevealEnd(Action onCardRevealed, BaseEffectCardLogic effectCardLogic)
        {
            usedCard.EffectRevealEvent -= currentEffectRevealHandler;
            currentEffectRevealHandler = null;
            OnEffectCardPlayedEvent?.Invoke(effectCardLogic);

            onCardRevealed?.Invoke();
        }

        private void AccessPlayerTeleportation()
        {
            foreach (Transform panel in parentOfAllPanels.transform)
            {
                var effectPanel = panel.GetComponent<PanelScript>();
                effectPanel.HighlightAsPathEnder();
                effectPanel.EnableTeleportation();
            }
        }
    }
}
