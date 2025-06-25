using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackCard : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GridLayoutGroup addCardsContainer;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void RevealCard()
    {
        transform.GetChild(0).SetSiblingIndex(1);
        /*isFacedDown = !false;*/
    }

    public void FlipCard()
    {
        animator.SetTrigger("Flip");
    }

    public int GetScores()
    {
        var spriteName = gameObject.transform.GetChild(1).GetComponent<Image>().sprite.name;
        int betScore = -1;

        try
        {
            betScore = int.Parse(spriteName.Substring(spriteName.Length - 2));
        }
        catch (System.Exception)
        {
            Debug.Log($"Cannot get score for card");
        }

        if (gameObject.transform.childCount != 0)
        {
            foreach (Transform addCard in addCardsContainer.transform)
            {
                betScore += addCard.GetComponent<BlackjackCard>().GetScores();
            }
        }

        return betScore;
    }
}
