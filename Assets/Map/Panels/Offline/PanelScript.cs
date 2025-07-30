using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class PanelScript : MonoBehaviour, IPointerClickHandler
    {
        private const string EFFECT_PANEL_SCRIPTS_MARKER = "PanelEffect";
        public enum Pos
        {
            None = -1,
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3,
        };

        // Нижнє поле повинно бути приватним! (без сериалайза)?
        [SerializeField] private List<GameObject> _posOfPanels = new List<GameObject>(Enumerable.Repeat<GameObject>(null, 4));
        [SerializeField] private GameObject directionArrowPrefab;
        /*[SerializeField] private EffectPanelInfoSingleplayer _effectPanelInfo;*/
        [SerializeField] private IPanelEffect panelEffect;

        private EffectPanelInfoSingleplayer effectPanelInfo;
        private SpriteRenderer panelSprite;

        private List<IEntity> entitiesOnPanel = new List<IEntity>();
        public IReadOnlyList<IEntity> EntitiesOnPanel => entitiesOnPanel;

        private List<GameObject> arrowsList = new List<GameObject>();

        private bool isClickableForTeleportation = false;

        /*private void Start()
        {
            _sidePanels = new List<GameObject>(Enumerable.Repeat<GameObject>(null, 4));
        }*/

        private void Start()
        {
            panelSprite = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {

        }

        public IEnumerator TriggerPanelEffect(IEntity entityInit, Action onComplete)
        {
            yield return StartCoroutine(panelEffect.Execute(entityInit, onComplete));
        }

        public void AttachPanelEffect(EffectPanelInfoSingleplayer effectPanelInfo)
        {
            panelEffect = GetPanelEffectInstance(effectPanelInfo);
            this.effectPanelInfo = effectPanelInfo;

            if (effectPanelInfo.Sprite != null)
                panelSprite.sprite = effectPanelInfo.Sprite;
        }

        private IPanelEffect GetPanelEffectInstance(EffectPanelInfoSingleplayer effectPanelInfo)
        {
            string typeName = $"{this.GetType().Namespace}.{effectPanelInfo.Effect}{EFFECT_PANEL_SCRIPTS_MARKER}";
            var effectType = Type.GetType(typeName);

            if (effectType == null)
            {
                Debug.LogError($"Unknown panel effect type: {effectPanelInfo.Effect}");
                return null;
            }

            IPanelEffect panelEffect = Activator.CreateInstance(effectType) as IPanelEffect;

            return panelEffect;
        }

        public void RemoveHighlight()
        {
            // Місце для майбутнього коду коли помітка про те що дана панель є кінцем шляху буде зроблена нормально
            Debug.Log($"Removing highlight for panel: {gameObject.name}");
        }

        public void EnableTeleportation()
        {
            isClickableForTeleportation = true;
        }

        public void HighlightAsPathEnder()
        {
            GetComponent<SpriteRenderer>().sprite = SpriteLoadManager.Instance.GetPathEnderSprite();
        }

        public EffectPanelInfoSingleplayer GetEffectPanelInfo => effectPanelInfo;
        public int GetNeighboursCount => _posOfPanels.Count(obj => obj != null);

        public List<PanelScript> GetAvailableNearPanelsOrNull(IEntity entity, List<PanelScript> exceptionsPanels = null, PanelScript exceptionPanel = null, bool isDirectionMatter = false)
        {
            List<PanelScript> nearPanels = new List<PanelScript>();

            for (int i = 0; i < _posOfPanels.Count; i++)
            {
                if (_posOfPanels[i] == null)
                {
                    continue;
                }
                else if (exceptionsPanels != null && exceptionsPanels.Contains(_posOfPanels[i].GetComponent<PanelScript>()))
                {
                    /*exceptionsPanels.Remove(_posOfPanels[i].GetComponent<PanelScript>());*/ // видаляємо панель, яка вже прошла в пошуку
                    continue;
                }
                else if (exceptionPanel != null && _posOfPanels[i].GetComponent<PanelScript>() == exceptionPanel)
                {
                    continue;
                }
                else if (isDirectionMatter && (Pos)i == GetOppositePosOrNone((Pos)entity.GetEntityDirection))
                {
                    continue;
                }

                nearPanels.Add(_posOfPanels[i].GetComponent<PanelScript>());
            }

            if (nearPanels.Count == 0)
                return new List<PanelScript>();

            return nearPanels;
        }

        public List<(PanelScript panel, Direction dir)> GetAvailableForwardPanels(Direction fromDirection)
        {
            var result = new List<(PanelScript panel, Direction dir)>();

            for (int i = 0; i < _posOfPanels.Count; i++)
            {
                var neighborObj = _posOfPanels[i];
                if (neighborObj == null)
                    continue;

                var directionToNeighbor = (Direction)i;

                // Исключаем обратное направление
                if (directionToNeighbor == MapManager.GetOppositeDirection(fromDirection))
                    continue;

                var neighborPanel = neighborObj.GetComponent<PanelScript>();
                if (neighborPanel != null)
                    result.Add((neighborPanel, directionToNeighbor));
            }

            return result;
        }

        public PanelScript GetNeighborByIndex(int index)
        {
            if (index < 0 || index >= _posOfPanels.Count)
                return null;

            var obj = _posOfPanels[index];
            return obj != null ? obj.GetComponent<PanelScript>() : null;
        }

        public (PanelScript, Direction) GetNextPanelOrNull(IEntity entity)
        {
            int availablePanelsCount = _posOfPanels.Count(panel => panel != null);
            Direction playerDirection = entity.GetEntityDirection;

            /*if (availablePanelsCount == 1) // устаревший
            {
                if (_posOfPanels[(int)playerDirection] != null)
                {
                    return (_posOfPanels[(int)playerDirection].GetComponent<PanelScript>(), playerDirection);
                }
                else
                {
                    for (int i = 0; i < _posOfPanels.Count; i++)
                    {
                        var panel = _posOfPanels[i];

                        if (panel != null)
                        {
                            return (panel.GetComponent<PanelScript>(), (PlayerController.Direction)i);
                        }
                    }
                }
            }*/

            if (availablePanelsCount > 1)
            {
                List<GameObject> tempPanelList = new List<GameObject>();
                tempPanelList.AddRange(_posOfPanels);

                for (int i = 0; i < tempPanelList.Count; i++)
                {
                    if (tempPanelList[i] != null && (Pos)i == GetOppositePosOrNone((Pos)playerDirection))
                    {
                        tempPanelList[i] = null;
                        --availablePanelsCount;
                    }
                }

                if (availablePanelsCount == 1)
                {
                    if (tempPanelList[(int)playerDirection] != null)
                    {
                        return (tempPanelList[(int)playerDirection].GetComponent<PanelScript>(), playerDirection);
                    }
                    else
                    {
                        for (int i = 0; i < tempPanelList.Count; i++)
                        {
                            var panel = tempPanelList[i];

                            if (panel != null)
                            {
                                return (panel.GetComponent<PanelScript>(), (Direction)i);
                            }
                        }
                    }
                }

                switch (entity.GetEntityType)
                {
                    case EntityType.Player:

                        entity.StopMoving();

                        for (int panelI = 0; panelI < _posOfPanels.Count; panelI++)
                        {
                            if (_posOfPanels[panelI] == null)
                                continue;

                            var currentPanelScript = _posOfPanels[panelI].GetComponent<PanelScript>();

                            if ((Pos)panelI == GetOppositePosOrNone((Pos)playerDirection))
                                continue;

                            SpawnDirectionArrows(panelI, entity as BasePlayerController);
                        }

                        break;

                    case EntityType.Enemy: // Покищо заглушка побудована на рандомі(хоча для переміщення боса підходить)

                        GameObject nextPanel = null;
                        int randomIterator = 0;

                        do
                        {
                            randomIterator = Random.Range(0, tempPanelList.Count);
                            nextPanel = tempPanelList[randomIterator];
                        }
                        while (nextPanel == null);

                        return (nextPanel.GetComponent<PanelScript>(), (Direction)randomIterator);

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }
            }

            return (null, playerDirection);
        }

        public void AssignSideOfPanel(GameObject panel, Pos side)
        {
            if (side == Pos.None || panel == null)
                return;

            var oppositePos = GetOppositePosOrNone(side);
            if (oppositePos == Pos.None)
                return;

            var otherPanelScript = panel.GetComponent<PanelScript>();
            var oppositePosOfPanels = otherPanelScript._posOfPanels;

            if ((int)oppositePos >= 0 && (int)oppositePos < oppositePosOfPanels.Count)
            {
                if (oppositePosOfPanels[(int)oppositePos] == null)
                {
                    oppositePosOfPanels[(int)oppositePos] = gameObject;
                }
            }

            if ((int)side >= 0 && (int)side < _posOfPanels.Count)
            {
                if (_posOfPanels[(int)side] == null)
                {
                    _posOfPanels[(int)side] = panel;
                }
            }
        }


        private void SpawnDirectionArrows(int pos, BasePlayerController player)
        {
            Pos actualPos = (Pos)pos;

            UnityEngine.Quaternion spawnRotation = new UnityEngine.Quaternion();
            UnityEngine.Vector2 spawnCord = new UnityEngine.Vector2();

            Bounds panelBounds = gameObject.GetComponent<SpriteRenderer>().bounds;

            bool flip = false;

            switch (actualPos)
            {
                case Pos.None:
                    break;

                case Pos.Left:
                    {
                        spawnCord = new UnityEngine.Vector2(panelBounds.min.x, panelBounds.center.y);

                        spawnRotation = UnityEngine.Quaternion.identity;

                        flip = true;
                    }
                    break;

                case Pos.Top:
                    {
                        spawnCord = new UnityEngine.Vector2(panelBounds.center.x, panelBounds.max.y);

                        float rotation = 90f;

                        spawnRotation = UnityEngine.Quaternion.Euler(0, 0, rotation);

                    }
                    break;

                case Pos.Right:
                    {
                        spawnCord = new UnityEngine.Vector2(panelBounds.max.x, panelBounds.center.y);

                        spawnRotation = UnityEngine.Quaternion.identity;
                    }
                    break;

                case Pos.Bottom:
                    {
                        spawnCord = new UnityEngine.Vector2(panelBounds.center.x, panelBounds.min.y);

                        float rotation = -90f;
                        spawnRotation = UnityEngine.Quaternion.Euler(0, 0, rotation);
                    }
                    break;

                default:
                    break;
            }

            var arrow = Instantiate(directionArrowPrefab, spawnCord, spawnRotation);
            arrowsList.Add(arrow);

            var ArrowHandler = arrow.GetComponent<DirectionArrowHandler>();
            ArrowHandler.chooseDirection += OnPlayerDirectionChoose;
            ArrowHandler.HoldPanel(_posOfPanels[pos].GetComponent<PanelScript>(), player, actualPos);

            if (flip)
                arrow.GetComponent<SpriteRenderer>().flipX = true;
        }

        private void OnPlayerDirectionChoose(Pos pos, BasePlayerController initiator, PanelScript panel)
        {
            initiator.StartMove((Direction)pos, panel);
            DeleteAllArrows();
        }

        private void DeleteAllArrows()
        {
            foreach (var arrow in arrowsList)
            {
                Destroy(arrow);
            }
            arrowsList.Clear();
        }

        private Pos GetOppositePosOrNone(Pos pos)
        {
            switch (pos)
            {
                case Pos.None:
                    return Pos.None;

                case Pos.Left:
                    return Pos.Right;

                case Pos.Top:
                    return Pos.Bottom;

                case Pos.Right:
                    return Pos.Left;

                case Pos.Bottom:
                    return Pos.Top;

                default:
                    return Pos.None;
            }
        }

        public static Pos GetRandomSide()
        {
            return (Pos)Random.Range(0, 4);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            #region Логіка для генератора мапи
            /*var panel = collision.gameObject;
            var side = GetPosRelativelyFromOtherPanelOrNone(panel);

            if (collision.tag.EndsWith("Panel"))
            {
                var oppositePosOfPanels = panel.GetComponent<PanelScript>()._posOfPanels;

                if (oppositePosOfPanels[((int)(GetOppositePosOrNone(side))) - 1] == null)
                {
                    oppositePosOfPanels[((int)(GetOppositePosOrNone(side))) - 1] = gameObject;
                }

                if (side == Pos.None)
                    return;

                if (panel != null && _posOfPanels[((int)side) - 1] == null)
                {
                    _posOfPanels[((int)side) - 1] = panel;
                }
            }

            if (IsServer)
            {
                Debug.LogWarning($"Server call: this: {this.gameObject.name}, other: {collision.transform.name}");
            }
            else
                Debug.LogWarning($"Client call: this: {this.gameObject.name}, other: {collision.transform.name}");*/
            #endregion 

            if (!collision.gameObject.TryGetComponent<IEntity>(out var entity)) return;

            if (!entity.SuppressPanelEffectTrigger)
                entity.moveEndEvent += OnEntityStay;

            if (!entitiesOnPanel.Contains(entity))
            {
                entitiesOnPanel.Add(entity);
                StartCoroutine(entity.OnStepOntoPanel(this));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isClickableForTeleportation)
            {
                isClickableForTeleportation = false;
                Debug.Log($"{gameObject.name} was clicked for teleportation!");
                Teleportation();
            }

            Debug.Log($"{gameObject.name} was clicked!");
        }

        private void Teleportation()
        {
            // код для перевірки того чи гравець взагалі може телепортуватися
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);
            GameManager.Instance.TeleportEntity(gameObject.transform.position, player, this);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var a = this;

            if (!collision.gameObject.TryGetComponent<IEntity>(out var entity)) return;

            entity.moveEndEvent -= OnEntityStay;

            if (entitiesOnPanel.Contains(entity))
                entitiesOnPanel.Remove(entity);
        }

        private void OnEntityStay(IEntity entity)
        {
            if (effectPanelInfo == null)
            {
                Debug.Log("effectPanelInfo is null");
                return;
            }

            Debug.Log("OnPlayerStay");
            StartCoroutine(PanelEffectsManager.Instance
                .TriggerPanelEffect(this, entity));
        }

        private Pos GetPosRelativelyFromOtherPanelOrNone(GameObject otherPanel)
        {
            UnityEngine.Vector3 difference = gameObject.transform.position - otherPanel.transform.position;

            #region XLogicIfThisX<0
            if (difference.x > 0 && gameObject.transform.position.x < 0)
            {
                return Pos.Left;
            }
            else if (difference.x < 0 && gameObject.transform.position.x < 0)
            {
                return Pos.Right;
            }
            #endregion

            #region XLogicIfThisX>0
            if (difference.x < 0 && gameObject.transform.position.x > 0)
            {
                return Pos.Left;
            }
            else if (difference.x > 0 && gameObject.transform.position.x > 0)
            {
                return Pos.Right;
            }
            #endregion

            #region XLogicIfThisY<0
            if (difference.y < 0 && gameObject.transform.position.y < 0)
            {
                return Pos.Top;
            }
            else if (difference.y > 0 && gameObject.transform.position.y < 0)
            {
                return Pos.Bottom;
            }
            #endregion

            #region XLogicIfThisY>0
            if (difference.y > 0 && gameObject.transform.position.y > 0)
            {
                return Pos.Top;
            }
            else if (difference.y < 0 && gameObject.transform.position.y > 0)
            {
                return Pos.Bottom;
            }
            #endregion

            return Pos.None;
        }
    }

    public enum PanelEffect
    {
        None = -1,
        Portal,
        Pursuit,
        Shop,
        Event,
        Backstab,
        Recharge,
        VIPClub,
        Dealing,
        Payoff,
        Fate,
        BadBeat,
        Casino,
        IllegalCasino,
        Spawn
    }
}
