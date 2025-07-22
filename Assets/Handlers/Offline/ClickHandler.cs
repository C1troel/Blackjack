using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Singleplayer
{
    public class ClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnEntityClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnEntityClickEvent?.Invoke();
        }
    }
}