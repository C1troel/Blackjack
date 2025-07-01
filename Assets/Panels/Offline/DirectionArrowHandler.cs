using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Singleplayer
{
    public class DirectionArrowHandler : MonoBehaviour
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
            ChooseDirection();
        }

        private void ChooseDirection()
        {
            chooseDirection?.Invoke(pos, playerInit, holdedPanel);
        }
    }
}
