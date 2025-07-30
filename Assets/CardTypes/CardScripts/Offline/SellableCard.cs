using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class SellableCard : MonoBehaviour
    {
        private const string FLIP_CARD_TRIGGER_NAME = "Flip";
        [SerializeField] private Animator animator;

        private EffectCardType sellableEffectCard;

        public void FlipCard()
        {
            animator.SetTrigger(FLIP_CARD_TRIGGER_NAME);
        }

        public void SetupSellableCard(Sprite sprite, EffectCardType sellableEffectCard)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            this.sellableEffectCard = sellableEffectCard;
        }

        public EffectCardType GetSellableEffectCard()
        {
            return sellableEffectCard;
        }

        private void RevealCard()
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}