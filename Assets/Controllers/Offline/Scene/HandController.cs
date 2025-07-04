using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandController : MonoBehaviour
{
    [SerializeField] private GameObject cardsContainer;
    [SerializeField] private TextMeshProUGUI handScoreText;
    [SerializeField] private float closingHandXSpacing;
    [SerializeField] private float handClosingSpeed;

    public bool isDoubled { get; private set; }

    public int handBet { get; private set; }

    public bool isSplitted { get; private set; }
    public ulong handPlayerId { get; private set; } = 22; // Означає, що рука належить дилеру

    public void ManageHandForPlayer(ulong playerId, int playerBet)
    {
        handPlayerId = playerId;
        handBet = playerBet;
        gameObject.SetActive(true);
    }

    public int GetPlayerHandScores()
    {
        var firstCard = cardsContainer.transform.GetChild(0);

        int handScore = firstCard.GetComponent<BlackjackCard>().GetScores();

        handScoreText.text = handScore.ToString();

        return handScore;
    }

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
}
