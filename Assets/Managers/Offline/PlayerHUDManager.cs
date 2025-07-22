using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class PlayerHUDManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private TextMeshProUGUI playerMoneyText;
        [SerializeField] private TextMeshProUGUI playerChipsText;

        [SerializeField] private Button specialAbilityBtn;
        [SerializeField] private Button cardDrawBtn;

        [SerializeField] private PlayerEffectCardsHandler playerEffectCardsHandler;

        private BasePlayerController managedPlayer;

        public void ManagePlayerHud(IEntity player)
        {
            managedPlayer = player as BasePlayerController;
            managedPlayer.CurencyChangeEvent += OnPlayerCurrencyChange;
            managedPlayer.HpChangeEvent += OnPlayerHpChange;
            managedPlayer.StatsChangeEvent += OnPlayerStatsChange;
            managedPlayer.SpecialAbility.GlobalEffectStateEvent += OnPlayerSpecialAbilityStateChange;
            managedPlayer.ManageEffectCardsHandler(playerEffectCardsHandler);

            UpdateAllHud();
        }

        private void UpdateAllHud()
        {
            OnPlayerHpChange();
            OnPlayerStatsChange();
            OnPlayerCurrencyChange();
            OnPlayerSpecialAbilityStateChange();
        }

        public void TogglePlayerHudButtons(bool isActive)
        {
            EffectCardApplier.Instance.gameObject.SetActive(isActive);
            cardDrawBtn.interactable = isActive;
            specialAbilityBtn.interactable = isActive;
        }

        private void OnPlayerSpecialAbilityStateChange()
        {
            bool isAbilityReady = managedPlayer.SpecialAbility.CooldownRounds == 0;
            bool isPlayersTurn = TurnManager.Instance.CurrentTurnEntity == (managedPlayer as IEntity);

            specialAbilityBtn.interactable = isAbilityReady && isPlayersTurn;
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