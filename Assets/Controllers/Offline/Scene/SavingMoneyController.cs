using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class SavingMoneyController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentMoneyText;

        [SerializeField] private Button saveAllBtn;
        [SerializeField] private Button saveHalfBtn;
        [SerializeField] private Button cancelBtn;

        [SerializeField] private GameObject background;

        private Action<int> onMoneySaveCallback;

        private BasePlayerController player;

        public void ManageForPlayer(BasePlayerController player) => this.player = player;

        public void StartSavingInteraction(Action<int> onSave)
        {
            onMoneySaveCallback = onSave;
            int playerMoney = player.GetEntityMoney;

            if (playerMoney == 0)
            {
                onSave?.Invoke(0);
                return;
            }

            background.SetActive(true);
            ShowCurrentMoney(playerMoney);
            EnableAvailableInteractions(playerMoney);
        }

        private void ShowCurrentMoney(int playerMoney) => currentMoneyText.text = $"You have {playerMoney}$";

        private void EnableAvailableInteractions(int playerMoney)
        {
            saveAllBtn.interactable = true;
            cancelBtn.interactable= true;

            if (playerMoney > 1)
                saveHalfBtn.interactable = true;
        }

        private void EndSavingInteraction()
        {
            onMoneySaveCallback = null;
            background.SetActive(false);

            currentMoneyText.text = "";
            saveAllBtn.interactable = false;
            cancelBtn.interactable = false;
            saveHalfBtn.interactable = false;
        }

        public void OnSaveAllBtnClick()
        {
            onMoneySaveCallback?.Invoke(player.GetEntityMoney);
            EndSavingInteraction();
        }

        public void OnSaveHalfBtnClick()
        {
            onMoneySaveCallback?.Invoke(player.GetEntityMoney / 2);
            EndSavingInteraction();
        }

        public void OnSaveCancelBtnClick()
        {
            onMoneySaveCallback?.Invoke(0);
            EndSavingInteraction();
        }
    }
}