using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Panel
{
    public class DirectionArrowHandler : NetworkBehaviour
    {
        private PanelScript holdedPanel; // �����, ��� ���������� �����, �� ��� �������� �� ������
        private PanelScript.Pos pos;
        private PlayerController playerInit; // �����, ��� ���������� Id ������, ��� ����� �'������� ������
        public event Action<PanelScript.Pos, PlayerController, PanelScript> chooseDirection;

        public void HoldPanel(PanelScript panel, PlayerController initiator, PanelScript.Pos pos)
        {
            holdedPanel = panel;
            playerInit = initiator;
            this.pos = pos;
        }

        private void OnMouseDown()
        {
            Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} is clicked");

            /*if (NetworkManager.Singleton.LocalClientId != playerInit.GetPlayerId)
                return;*/

            if (!IsServer || IsHost)
            {
                Debug.Log("ServerRequest");
                RequestForDirectionChooseServerRpc(); // !���� ���� ������ �������� �� ������
            }
            /*ChooseDirection();*/
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestForDirectionChooseServerRpc(ServerRpcParams serverRpcParams = default) // !���� ���� ������ �������� �� ������
        {
            if (serverRpcParams.Receive.SenderClientId != playerInit.GetPlayerId)
            {
                return;
            }
            else
                ChooseDirection();
        }

        private void ChooseDirection()
        {
            chooseDirection?.Invoke(pos, playerInit, holdedPanel);
        }
    }
}
