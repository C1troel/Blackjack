using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer
{
    public class TestPlayerInfoHandler : MonoBehaviour
    {
        public ulong PlayerId { get; set; }

        private Button playerChooseButton;

        private void Start()
        {
            playerChooseButton = GetComponent<Button>();
        }

        public void ActivateButton() => playerChooseButton.interactable = true;

        private void DisableAllButtons()
        {
            foreach (Transform button in transform.parent.transform)
                button.gameObject.GetComponent<Button>().interactable = false;
        }

        public void OnPlayerClick()
        {
            DisableAllButtons();
            PanelEffectsManager.Instance.RequestForPlayerChooseServerRpc(PlayerId);
        }
    }
}