using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardReveal : MonoBehaviour
{
    private bool isFacedDown = true;
    private void RevealCard()
    {
        transform.Find("1Side").SetSiblingIndex(1);
        isFacedDown = false;
    }

    private void OnRevealEnd()
    {
        gameObject.SetActive(false);
        transform.Find("2Side").SetSiblingIndex(1);
        isFacedDown = true;
    }
}
