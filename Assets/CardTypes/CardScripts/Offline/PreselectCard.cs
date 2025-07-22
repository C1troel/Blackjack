using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class PreselectCard : MonoBehaviour
    {
        private const string FLIP_CARD_TRIGGER_NAME = "Flip";
        [SerializeField] private Animator animator;

        public void FlipCard()
        {
            animator.SetTrigger(FLIP_CARD_TRIGGER_NAME);
        }

        public void SetupPreselectCard(Sprite sprite)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        }

        public Sprite GetPreselectCardSprite()
        {
            return transform.GetChild(0).GetComponent<Image>().sprite;
        }

        private void RevealCard()
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}