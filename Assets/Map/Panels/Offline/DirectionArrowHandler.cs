using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Singleplayer
{
    public class DirectionArrowHandler : MonoBehaviour, IPointerClickHandler
    {
        private PanelScript holdedPanel; // зм≥нна, дл€ збереженн€ панел≥, на €ку переводе ц€ стр≥лка
        private PanelScript.Pos pos;
        private BasePlayerController playerInit; // зм≥нна, дл€ збереженн€ Id гравц€, дл€ €кого з'€вилас€ стр≥лка
        public event Action<PanelScript.Pos, BasePlayerController, PanelScript> chooseDirection;

        public void HoldPanel(PanelScript panel, BasePlayerController initiator, PanelScript.Pos pos)
        {
            holdedPanel = panel;
            playerInit = initiator;
            this.pos = pos;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ChooseDirection();
        }

        private void ChooseDirection()
        {
            chooseDirection?.Invoke(pos, playerInit, holdedPanel);
        }
    }
}
