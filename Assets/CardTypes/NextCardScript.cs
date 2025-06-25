using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NextCardScript : MonoBehaviour
{
    public bool isFacedDown { get; private set; } = true;
    public bool isAppended { get; set; }

    public int handNumber { get; set; } = -1;

    [SerializeField]private Animator animator;

    public void FlipCard(bool isAtacker = true)
    {
        animator.SetTrigger("Flip");

        if (isAtacker)
            gameObject.transform.Find("AddCardContainerToRight").gameObject.SetActive(true);
        else
            gameObject.transform.Find("AddCardContainerToLeft").gameObject.SetActive(true);
    }

    private void OnCardFlip()
    {
        transform.Find("1Side").SetSiblingIndex(1);
        isFacedDown = false;
    }


}