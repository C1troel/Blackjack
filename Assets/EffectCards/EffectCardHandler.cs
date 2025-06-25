using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EffectCards
{
    public class EffectCardHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public enum Effect
        {
            DecreaseHP = 0,
        }

        [HideInInspector] public Transform parentAfterDrag;

        private UnityEngine.UI.Image _image;

        private Animator _animator;

        private Effect _effect;

        private bool canUse = true;

        private void Start()
        {
            _image = GetComponent<UnityEngine.UI.Image>();
            _animator = GetComponent<Animator>();
            _effect = Effect.DecreaseHP;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canUse)
            {
                Debug.Log("BeginDrag");
                parentAfterDrag = transform.parent;
                var parentOfParent = transform.parent.transform.parent;
                transform.SetParent(parentOfParent);
                _image.raycastTarget = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canUse)
            {
                Debug.Log("Dragging");
                transform.position = Input.mousePosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canUse)
            {
                Debug.Log("EndDrag");
                transform.SetParent(parentAfterDrag);
                _image.raycastTarget = true;
            }
        }

        public void UseCard()
        {
            _animator.SetTrigger("CardUsage");
        }

        private void OnAnimationEnd()
        {
            Destroy(gameObject);
        }

        public Effect GetEffect => _effect;
    }
}
