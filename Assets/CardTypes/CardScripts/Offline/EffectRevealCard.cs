using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class EffectRevealCard : MonoBehaviour
    {
        private const string CARD_FLIP_TRIGGER_NAME = "Flip";

        [SerializeField] private Animator animator;
        [SerializeField] private Image revealedCardSide;
        [SerializeField] private GameObject moveCardFlipUI;

        [SerializeField] private Button keepFrontBtn;
        [SerializeField] private Button flipCardBtn;
        [SerializeField] private Button altValueBtn;

        public event Action EffectRevealEvent;
        public event Action<int> MoveCardValueSelectedEvent;

        private Sprite revealedMoveCardSprite;
        private MoveCard revealedMoveCard;

        private void Start()
        {
            SetupMoveCardUIBtns();
        }

        private void SetupMoveCardUIBtns()
        {
            keepFrontBtn.onClick.AddListener(OnKeepFrontBtnClick);
            flipCardBtn.onClick.AddListener(OnFlipCardBtnClick);
            altValueBtn.onClick.AddListener(OnAltCardValueBtnClick);
        }

        public void RevealEffect(Sprite revealedEffectSprite)
        {
            revealedCardSide.sprite = revealedEffectSprite;
            gameObject.SetActive(true);
            animator.SetTrigger(CARD_FLIP_TRIGGER_NAME);
        }

        public void RevealMoveCard(MoveCard moveCard)
        {
            revealedMoveCard = moveCard;
            revealedMoveCardSprite = moveCard.frontSide;
            gameObject.SetActive(true);
            animator.SetTrigger(CARD_FLIP_TRIGGER_NAME);
        }

        private void RevealCard()
        {
            if (revealedMoveCardSprite != null)
                revealedCardSide.sprite = revealedMoveCardSprite;

            transform.GetChild(1).gameObject.SetActive(false);
        }

        private void OnRevealEnd()
        {
            if (revealedMoveCard == null)
            {
                EndReveal();
                return;
            }
            else if (revealedMoveCard.GetSteps(revealedMoveCardSprite) != 11 && revealedMoveCard.backSide == null)
            {
                int moveCardSteps = revealedMoveCard.GetSteps(revealedMoveCardSprite);
                SelectMoveCardValue(moveCardSteps);
                EndReveal();
                return;
            }

            ActivateHudDependsOnMoveCard();
        }

        private void OnKeepFrontBtnClick()
        {
            DeactivateMoveCardUI();

            int pickedMoveCardSteps = revealedMoveCard.GetSteps(revealedMoveCardSprite);
            SelectMoveCardValue(pickedMoveCardSteps);

            EndReveal();
        }

        private void OnFlipCardBtnClick()
        {
            DeactivateMoveCardUI();

            if (revealedMoveCardSprite == revealedMoveCard.frontSide)
                revealedMoveCardSprite = revealedMoveCard.backSide;
            else
                revealedMoveCardSprite = revealedMoveCard.frontSide;

            animator.SetTrigger(CARD_FLIP_TRIGGER_NAME);
        }

        private void OnAltCardValueBtnClick()
        {
            DeactivateMoveCardUI();

            int pickedMoveCardSteps = 1; // бо тільки туз має 2 значення (1/11)
            SelectMoveCardValue(pickedMoveCardSteps);

            EndReveal();
        }

        private void DeactivateMoveCardUI()
        {
            moveCardFlipUI.SetActive(false);

            keepFrontBtn.gameObject.SetActive(false);
            flipCardBtn.gameObject.SetActive(false);
            altValueBtn.gameObject.SetActive(false);
        }

        private void ActivateHudDependsOnMoveCard()
        {
            keepFrontBtn.gameObject.SetActive(true);

            if (revealedMoveCard.backSide != null)
                flipCardBtn.gameObject.SetActive(true);

            if (revealedMoveCard.GetSteps(revealedMoveCardSprite) == 11)
                altValueBtn.gameObject.SetActive(true);

            moveCardFlipUI.SetActive(true);
        }

        private void EndReveal()
        {
            gameObject.SetActive(false);
            DeactivateMoveCardUI();
            transform.GetChild(1).gameObject.SetActive(true);
            revealedCardSide.sprite = null;
            revealedMoveCard = null;

            RevealEffect();

        }

        private void SelectMoveCardValue(int moveCardValue) => MoveCardValueSelectedEvent?.Invoke(moveCardValue);
        private void RevealEffect() => EffectRevealEvent?.Invoke();
    }
}