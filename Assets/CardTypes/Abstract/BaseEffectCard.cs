using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Singleplayer
{
    public abstract class BaseEffectCard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IEffectCard    
    {
        [HideInInspector] public Transform parentAfterDrag;

        protected Image _image;

        protected Animator _animator;

        protected EffectCardInfo effectCardInfo;
        protected IEffectCardLogic effectCardLogic;

        protected bool canUse = true;

        private void Start()
        {
            _image = GetComponent<UnityEngine.UI.Image>();
            _animator = GetComponent<Animator>();
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
            if (!canUse)
                return;

            Debug.Log("EndDrag");

            if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<IDropHandler>() == null)
            {
                transform.SetParent(parentAfterDrag);
                transform.localPosition = Vector3.zero;
            }

            _image.raycastTarget = true;
        }

        public virtual void UseCard()
        {
            _animator.SetTrigger("CardUsage");
        }

        private void OnAnimationEnd()
        {
            ApplyEffect();
            Destroy(gameObject);
        }

        public void ApplyEffect(IEntity entityInit = null)
        {
            effectCardLogic.ApplyEffect(entityInit);
        }

        public void SetupEffectCard(EffectCardInfo effectCardInfo)
        {
            this.effectCardInfo = effectCardInfo;

            string typeName = $"{this.GetType().Namespace}.{effectCardInfo.EffectCardType}";
            var logicType = Type.GetType(typeName);

            if (logicType == null)
            {
                Debug.LogError($"Unknown logic type: {effectCardInfo.EffectCardType}");
                return;
            }

            effectCardLogic = Activator.CreateInstance(logicType) as IEffectCardLogic;
            effectCardLogic.SetupEffectCardLogic(effectCardInfo);
        }

        public List<EffectCardMaterial> GetCardMaterials => effectCardInfo.EffectCardMaterials;
        public EffectCardDmgType GetCardDmgType => effectCardInfo.EffectCardDmgType;
    }

    public enum EffectCardType
    {
        SmallMedicine
    }
}