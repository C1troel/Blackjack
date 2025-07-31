using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class PlayerHUDManager : MonoBehaviour
    {
        private const string ROUND_PREFIX = "Round ";
        private const string LEFT_CARDS_PREFIX = "Draw cards: ";
        private const string LEFT_STEPS_PREFIX = "Steps left: ";

        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private TextMeshProUGUI playerMoneyText;
        [SerializeField] private TextMeshProUGUI playerChipsText;

        [SerializeField] private TextMeshProUGUI roundCountText;
        [SerializeField] private TextMeshProUGUI leftCardsText;
        [SerializeField] private TextMeshProUGUI stepsLeftText;

        [SerializeField] private Button specialAbilityBtn;
        [SerializeField] private Button cardDrawBtn;

        [SerializeField] private PlayerEffectCardsHandler playerEffectCardsHandler;
        [SerializeField] private SavingMoneyController savingMoneyController;
        [SerializeField] private ShoppingController shoppingController;

        private BasePlayerController managedPlayer;

        public void ManagePlayerHud(IEntity player)
        {
            managedPlayer = player as BasePlayerController;
            managedPlayer.CurencyChangeEvent += OnPlayerCurrencyChange;
            managedPlayer.HpChangeEvent += OnPlayerHpChange;
            managedPlayer.StatsChangeEvent += OnPlayerStatsChange;
            managedPlayer.SpecialAbility.GlobalEffectStateEvent += OnPlayerSpecialAbilityStateChange;

            managedPlayer.ManageEffectCardsHandler(playerEffectCardsHandler);
            savingMoneyController.ManageForPlayer(managedPlayer);

            managedPlayer.LeftEffectCardsChangeEvent += OnLeftEffectCardsChange;
            managedPlayer.LeftStepsChangeEvent += OnLeftStepsChange;
            TurnManager.Instance.OnNewRoundStarted += OnNewRoundStart;

            UpdateAllHud();
        }

        private void UpdateAllHud()
        {
            OnPlayerHpChange();
            OnPlayerStatsChange();
            OnPlayerCurrencyChange();
            OnPlayerSpecialAbilityStateChange();
        }

        private void OnLeftStepsChange() => stepsLeftText.text = LEFT_STEPS_PREFIX + managedPlayer.GetEntityLeftSteps;

        private void OnLeftEffectCardsChange() => leftCardsText.text = 
            $"{LEFT_CARDS_PREFIX}{managedPlayer.GetEntityLeftCards}/{managedPlayer.GetEntityDefaultCardUsages}";

        private void OnNewRoundStart() => roundCountText.text = ROUND_PREFIX + TurnManager.Instance.CurrentRound;

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

        public SavingMoneyController GetSavingMoneyController => savingMoneyController;
        public ShoppingController GetShoppingController => shoppingController;
    }
}