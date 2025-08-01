using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Singleplayer
{
    public class BattleEffectCardApplier : MonoBehaviour, IDropHandler
    {
        private Image background;

        private void Start()
        {
        }

        public void ToggleBattleCardApplier(bool isActive)
        {
            this.gameObject.SetActive(isActive);

            if (background == null)
                background = gameObject.GetComponent<Image>();
        }

        public void OnCardBeginDrag()
        {
            background.raycastTarget = true;
        }

        public void OnCardEndDrag()
        {
            background.raycastTarget = false;
        }
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnEffectCardDrop");
            GameObject usedCard = eventData.pointerDrag;
            BaseEffectCard effectCard = usedCard.GetComponent<BaseEffectCard>();

            effectCard.UseCardInBattle();
        }
    }
}