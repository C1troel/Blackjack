using Multiplayer.EffectCards;
using Multiplayer.Panel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Multiplayer
{
    public class MapManager : NetworkBehaviour
    {
        [SerializeField] private List<GameObject> panels;
        [SerializeField] private GameObject parentOfAllPanels; // на даний момент використовується для помічення кожної панелі при телепортації
        [SerializeField] private GameObject usedCard;

        public static MapManager Instance { get; private set; }

        private List<PanelScript> highlightedPathEnders = new List<PanelScript>();

        public event Action<ulong> playerMoveEnd;

        private Coroutine waitingForPlayer;

        public int GetLastPlayerStepsCount { get; private set; }

        public bool IsPossiblePlayerTeleportation { get; private set; } = false;

        #region Стандартні мережеві функції
        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

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

        public void MakeADraw(PlayerController player)
        {
            int steps = UnityEngine.Random.Range(0, 12); // змінна яка повинна використовуватися замість tempSteps

            int tempSteps = 3;

            GetLastPlayerStepsCount += tempSteps;

            Debug.Log($"GetLastPlayerStepsCount is {GetLastPlayerStepsCount}");

            if (GetLastPlayerStepsCount == 21)
            {
                AccessPlayerToTeleport(player.GetPlayerId);
                return;
            }

            player.GetSteps(tempSteps);
            PathBuilding(player.GetComponent<PlayerController>().GetCurrentPanel, tempSteps, player);
            player.StartMove();
        }

        private void AccessPlayerToTeleport(ulong playerId)
        {
            IsPossiblePlayerTeleportation = true;
            waitingForPlayer = StartCoroutine(WaitingForPlayerAction());

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerId }
                }
            };

            AccessPlayerTeleportationClientRpc(clientRpcParams);
        }

        public void SubscribeToPlayerMoveEndEvent(PlayerController player)
        {
            player.moveEndEvent += OnPlayerMoveEnd;
        }

        /*public void UnsubscribeToPlayerMoveEndEvent(PlayerController player)
        {
            player.moveEndEvent -= OnPlayerMoveEnd;
        }*/

        private void OnPlayerMoveEnd(ulong Id)
        {
            /*if (playerMoveEnd != null)
            {
                playerMoveEnd(Id);
            }*/

            if (highlightedPathEnders.Count != 0)
            {
                foreach (var panel in highlightedPathEnders)
                    panel.RemoveHighlightClientRpc();
            }
        }

        private void PathBuilding(PanelScript startPanel, int stepCount, PlayerController player)
        {
            var pathEnders = new List<PanelScript>();

            var startNearPanels = startPanel.GetAvailableNearPanelsOrNull(player, null, null, true);

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
                    passedPanels.Add(player.GetCurrentPanel);
                    passedPanels.Add(startNearPanels[0]);

                    passedPanel = player.GetCurrentPanel;

                    tempPassedPanel = startNearPanels[0];
                    nearPanels.AddRange(startNearPanels[0].GetAvailableNearPanelsOrNull(player, null, passedPanel));

                    startNearPanels.Remove(startNearPanels[0]);
                    i++;

                    passedPanels.Add(nearPanels[0]);
                    passedPanel = tempPassedPanel;
                    continue;
                }
                else if (startNearPanels.Count == 0 && i == 1)
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
                    nearPanels = (nearPanels[0].GetAvailableNearPanelsOrNull(player, null, passedPanel));
                    i++;
                    passedPanels.Add(nearPanels[0]);
                    passedPanel = tempPassedPanel;
                }
                else if (nearPanels.Count != 0)
                {
                    int NeighboursCount = nearPanels[0].GetNeighboursCount;

                    tempPassedPanel = nearPanels[0];

                    if (NeighboursCount > 2)
                        nearPanels = nearPanels[0].GetAvailableNearPanelsOrNull(player, alreadyPickedPanels);
                    else
                        nearPanels = nearPanels[0].GetAvailableNearPanelsOrNull(player, null, passedPanel);

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

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.GetPlayerId }
                }
            };

            foreach (var panel in pathEnders)
                panel.HighlightAsPathEnderClientRpc(clientRpcParams);

            highlightedPathEnders = pathEnders;
        }

        private void UseEffect(EffectCardHandler.Effect effect, PlayerController player)
        {
            switch (effect)
            {
                case EffectCardHandler.Effect.DecreaseHP:
                    player.GetDamage(20);
                    TestPlayerSpawner.Instance.UpdateAllPlayersScrollViewInfo();
                    TestPlayerSpawner.Instance.UpdatePlayerHUD(player);
                    break;

                default:
                    break;
            }
        }
        #endregion

        /*[ClientRpc]
        private void HighlightPathEndersClientRpc(List<PanelScript> pathEnders)
        {

        }*/

        #region ServerRpc
        [ServerRpc(RequireOwnership = false)]
        public void UseCardServerRpc(EffectCardHandler.Effect cardEffect, ServerRpcParams serverRpcParams = default)
        {
            var player = TestPlayerSpawner.Instance.GetPlayerWithId(serverRpcParams.Receive.SenderClientId);

            if (TestPlayerSpawner.Instance.GetPlayerWithId(serverRpcParams.Receive.SenderClientId).GetPlayerLeftCards > 0)
            {
                RevealEffectCardClientRpc();
                UseEffect(cardEffect, player);
            }
        }
        #endregion

        #region ClientRpc
        [ClientRpc]
        private void RevealEffectCardClientRpc()
        {
            usedCard.SetActive(true);
        }

        [ClientRpc]
        private void AccessPlayerTeleportationClientRpc(ClientRpcParams clientRpcParams = default)
        {
            foreach (Transform panel in parentOfAllPanels.transform)
            {
                var effectPanel = panel.GetComponent<PanelScript>();
                effectPanel.HighlightAsPathEnder();
                effectPanel.EnableTeleportation();
            }
        }

        [ClientRpc]
        public void ShowUpEventCardClientRpc(string eventName)
        {
            // налаштувати SpriteLoadManager під загрузку спрайту з ім'ям eventName
        }
        #endregion
    }
}