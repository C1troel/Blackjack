using Singleplayer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Singleplayer
{
    public class PlayerHUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private TextMeshProUGUI playerMoneyText;
        [SerializeField] private TextMeshProUGUI playerChipsText;

        private BasePlayerController managedPlayer;

        public void ManagePlayerHud(IEntity player)
        {
            managedPlayer = player as BasePlayerController;
            managedPlayer.curencyChangeEvent += OnPlayerCurrencyChange;
            managedPlayer.hpChangeEvent += OnPlayerHpChange;
            managedPlayer.statsChangeEvent += OnPlayerStatsChange;

            UpdateAllHud();
        }

        private void UpdateAllHud()
        {
            OnPlayerHpChange();
            OnPlayerStatsChange();
            OnPlayerCurrencyChange();
        }

        private void OnPlayerHpChange()
        {
            playerHpText.text = $"{managedPlayer.GetEntityHp}/{managedPlayer.GetEntityMaxHp}";
        }

        private void OnPlayerStatsChange()
        {
            playerStatsText.text = $"{managedPlayer.GetEntityAtk}/{managedPlayer.GetEntityDef}";
        }

        private void OnPlayerCurrencyChange()
        {
            playerMoneyText.text = $"{managedPlayer.GetEntityMoney}";
            playerChipsText.text = $"{managedPlayer.GetEntityChips}";
        }
    }
}