using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace Singleplayer
{
    public class BlackjackCard : MonoBehaviour
    {
        private Animator animator;
        [SerializeField] private GridLayoutGroup addCardsContainer;

        public GridLayoutGroup GetAddCardsContainer => addCardsContainer;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
        }

        private void RevealCard()
        {
            transform.GetChild(1).gameObject.SetActive(false);
            /*isFacedDown = !false;*/
        }

        public void FlipCard()
        {
            animator.SetTrigger("Flip");
        }

        public int GetCardValue()
        {
            var spriteName = gameObject.transform.GetChild(0).GetComponent<Image>().sprite.name;
            int cardValue = int.Parse(spriteName.Substring(spriteName.Length - 2));

            return cardValue switch
            {
                1 => 11,
                11 => 10,
                12 => 10,
                13 => 10,
                _ => cardValue,
            };
        }

        public int GetScores()
        {
            int totalScore = 0;
            int aceCount = 0;

            int cardValue = GetCardValue();

            if (cardValue == 11) aceCount++;
            totalScore += cardValue;

            foreach (Transform addCard in addCardsContainer.transform)
            {
                var card = addCard.GetComponent<BlackjackCard>();
                int value = card.GetCardValue();

                if (value == 11) aceCount++;
                totalScore += value;
            }

            while (totalScore > 21 && aceCount > 0)
            {
                totalScore -= 10;
                aceCount--;
            }

            return totalScore;
        }

    }
}