using Multiplayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EffectCards
{
    public class EffectCardApplier : MonoBehaviour, IDropHandler
    {
        [SerializeField] private GameObject inputBlock;
        public void OnDrop(PointerEventData eventData)
        {
            /*inputBlock.SetActive(true);*/
            Debug.LogWarning("OnDrop");
            GameObject usedCard = eventData.pointerDrag;
            EffectCardHandler effectCard = usedCard.GetComponent<EffectCardHandler>();
            /*effectCard.parentAfterDrag = transform;
            effectCard.UseCard();*/

            MapManager.Instance.UseCardServerRpc(effectCard.GetEffect);

            Destroy(usedCard);
        }
    }
}
