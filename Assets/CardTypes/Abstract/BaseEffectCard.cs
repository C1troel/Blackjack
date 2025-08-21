using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Singleplayer
{
    public abstract class BaseEffectCard : MonoBehaviour, IEffectCard, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [HideInInspector] public Transform parentAfterDrag;
        private int originalSiblingIndex;

        public event Action<BaseEffectCard> OnEffectCardUsed;
        public IEffectCardLogic EffectCardLogic { get; private set; }

        protected Image _image;
        protected Material deafaultCardMaterial;
        protected Material outlineMaterial;

        protected Animator _animator;

        protected EffectCardInfo effectCardInfo;

        protected BasePlayerController player;

        private void Start()
        {
            _image = GetComponent<Image>();
            _animator = GetComponent<Animator>();
            deafaultCardMaterial = _image.material;
            outlineMaterial = EffectCardDealer.Instance.GetEffectCardOutlineMaterial;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!EffectCardLogic.CanUse || EffectCardLogic.TargetEnemiesList == null || GameManager.Instance.IsChoosing)
                return;

            foreach (var entity in EffectCardLogic.TargetEnemiesList)
                entity.SetOutline();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!EffectCardLogic.CanUse || EffectCardLogic.TargetEnemiesList == null || GameManager.Instance.IsChoosing)
                return;

            foreach (var entity in EffectCardLogic.TargetEnemiesList)
                entity.RemoveOutline();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            EffectCardApplier.Instance.OnCardBeginDrag();
            GameManager.Instance.PlayerCamera.ToggleSwipes(false);

            if (BattleManager.Instance.IsBattleActive)
                BattleManager.Instance.GetBattlePlayerEffectCardsApplier.OnCardBeginDrag();

            Debug.Log("BeginDrag");
            parentAfterDrag = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            var parentOfParent = transform.parent.transform.parent;
            transform.SetParent(parentOfParent);
            _image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("EndDrag");

            if (!EffectCardLogic.CanCounter && 
                (player.GetEntityLeftCards == 0 || !EffectCardLogic.CanUse) && 
                !BattleManager.Instance.IsBattleActive)
            {
                transform.SetParent(parentAfterDrag);
                transform.SetSiblingIndex(originalSiblingIndex);
                transform.localPosition = Vector3.zero;

                _image.raycastTarget = true;
                EffectCardApplier.Instance.OnCardEndDrag();
                GameManager.Instance.PlayerCamera.ToggleSwipes(true);

                if (BattleManager.Instance.IsBattleActive)
                    BattleManager.Instance.GetBattlePlayerEffectCardsApplier.OnCardEndDrag();

                return;
            }

            if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<IDropHandler>() == null)
            {
                transform.SetParent(parentAfterDrag);
                transform.SetSiblingIndex(originalSiblingIndex);
                transform.localPosition = Vector3.zero;
            }

            _image.raycastTarget = true;
            EffectCardApplier.Instance.OnCardEndDrag();
            GameManager.Instance.PlayerCamera.ToggleSwipes(true);

            if (BattleManager.Instance.IsBattleActive)
                BattleManager.Instance.GetBattlePlayerEffectCardsApplier.OnCardEndDrag();
        }


        public virtual void TryToUseCard()
        {
            if (!EffectCardLogic.CanCounter && (player.GetEntityLeftCards == 0 || !EffectCardLogic.CanUse))
                return;

            /*player.DecreaseEffectCardsUsages();*/
            GameManager.Instance.TogglePlayerHudButtons(false);
            RemoveCardOutline();
            _animator.SetTrigger("CardUsage");
        }

        public virtual void UseCardInBattle()
        {
            BattleManager.Instance.ToggleBattleHudControls(false);
            RemoveCardOutline();
            _animator.SetTrigger("CardUsage");
        }

        private void OnAnimationEnd()
        {
            _animator.enabled = false;
            _image.enabled = false;

            bool isBattlePurpose = EffectCardLogic.EffectCardInfo.EffectCardPurposes
                .Any(p => p == EffectCardPurpose.BattleAttack || p == EffectCardPurpose.BattleDefense);

            bool isBattleAndPurpose = isBattlePurpose && BattleManager.Instance.IsBattleActive;

            if (!EffectCardLogic.CanCounter || !isBattleAndPurpose)
            {
                player.DecreaseEffectCardsUsages();
            }

            MapManager.Instance.OnEffectCardPlayed(this);
        }

        public void ApplyEffect(IEntity entityInit = null)
        {
            EffectCardDealer.Instance.StartCoroutine(EffectCardLogic.ApplyEffect(() =>
            OnEffectCardApplyEnd(),
            entityInit));
        }

        private void OnEffectCardApplyEnd()
        {
            OnEffectCardUsed?.Invoke(this);
            GameManager.Instance.TogglePlayerHudButtons(true);

            var battleManager = BattleManager.Instance;

            if (battleManager.IsBattleActive)
                battleManager.ToggleBattleHudControls(true);
        }

        public void CheckIfCanBeUsed(IEntity entityOwner)
        {
            if (EffectCardLogic.CheckIfCanBeUsed(entityOwner))
                OutlineCard();
        }

        public void OutlineCard() => _image.material = outlineMaterial;

        public void RemoveCardOutline() => _image.material = deafaultCardMaterial;

        public void SetupEffectCard(EffectCardInfo effectCardInfo)
        {
            this.effectCardInfo = effectCardInfo;

            var effectCardImage = GetComponent<Image>();

            if (effectCardInfo.EffectCardSprite != null)
                effectCardImage.sprite = effectCardInfo.EffectCardSprite;

            string typeName = $"{this.GetType().Namespace}.{effectCardInfo.EffectCardType}";
            var logicType = Type.GetType(typeName);

            if (logicType == null)
            {
                Debug.LogError($"Unknown logic type: {effectCardInfo.EffectCardType}");
                return;
            }

            EffectCardLogic = Activator.CreateInstance(logicType) as IEffectCardLogic;
            EffectCardLogic.SetupEffectCardLogic(effectCardInfo);
            player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;
        }

        public List<EffectCardMaterial> GetCardMaterials => effectCardInfo.EffectCardMaterials;
        public EffectCardDmgType GetCardDmgType => effectCardInfo.EffectCardDmgType;

        public Sprite GetEffectSprite() => effectCardInfo.EffectCardSprite;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                Debug.Log("UI Double click!");
                OnDoubleClick();
            }
        }

        private void OnDoubleClick()
        {
            GameManager.Instance.GetEffectCardInfoController.ShowUpEffectCardInfo(effectCardInfo);
        }
    }
}