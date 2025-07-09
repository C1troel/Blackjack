using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Singleplayer
{
    public class EffectCardApplier : MonoBehaviour, IDropHandler
    {
        [SerializeField] private GameObject inputBlock;
        public void OnDrop(PointerEventData eventData)
        {
            /*inputBlock.SetActive(true);*/
            Debug.Log("OnEffectCardDrop");
            GameObject usedCard = eventData.pointerDrag;
            BaseEffectCard effectCard = usedCard.GetComponent<BaseEffectCard>();
            /*effectCard.parentAfterDrag = transform;
            effectCard.UseCard();*/

            effectCard.UseCard();
        }
    }
}