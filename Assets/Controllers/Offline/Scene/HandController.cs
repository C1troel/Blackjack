using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private GameObject cardsContainer;
        [SerializeField] private TextMeshProUGUI handScoreText;
        [SerializeField] private float closingHandXSpacing;
        [SerializeField] private float handClosingSpeed;

        public bool isDoubled { get; private set; }

        public bool hasBlackjack { get; private set; }

        public bool isInsured { get; private set; }

        public int handBet { get; private set; }

        public void ManageHandForPlayer(int playerBet)
        {
            handBet = playerBet;
            gameObject.SetActive(true);
        }

        public bool CheckForBlackjack()
        {
            if (GetCardsCount() == 2 && GetHandScores() == 21)
            {
                hasBlackjack = true;
                return true;
            }
            else
            {
                hasBlackjack = false;
                return false;
            }
        }

        public void Insure()
        {
            handBet /= 2;
            isInsured = true;
        }

        public void DoubleDown()
        {
            handBet *= 2;
            isDoubled = true;
        }

        public int GetCardsCount()
        {
            var firstCard = GetHandFirstCard();
            if (firstCard == null)
            {
                Debug.Log("Try to get cardsCount from empty hand");
                return 0;
            }

            int cardsCount = 1;
            return cardsCount += firstCard.GetAddCardsContainer.transform.childCount;
        }

        public BlackjackCard GetHandFirstCard()
        {
            return cardsContainer.GetComponentInChildren<BlackjackCard>();
        }

        public int GetHandScores()
        {
            var firstCard = GetHandFirstCard();

            if (firstCard == null)
            {
                handScoreText.text = "";
                return 0;
            }

            int handScore = firstCard.GetComponent<BlackjackCard>().GetScores();

            handScoreText.text = handScore.ToString();

            return handScore;
        }

        public void ResetScores() => handScoreText.text = "";

        public IEnumerator CloseHand()
        {
            var firstCard = cardsContainer.transform.GetChild(0);
            var closingCardsSpacing = new Vector2(closingHandXSpacing, 0);
            var firstCardAddContainer = firstCard.transform.GetChild(3).GetComponent<GridLayoutGroup>();

            while (firstCardAddContainer.spacing != closingCardsSpacing)
            {
                firstCardAddContainer.spacing = Vector2.MoveTowards(
                    firstCardAddContainer.spacing,
                    closingCardsSpacing,
                    Time.deltaTime * handClosingSpeed
                );

                yield return null;
            }
        }

        public GameObject GetCardsContainer => cardsContainer;
    }
}