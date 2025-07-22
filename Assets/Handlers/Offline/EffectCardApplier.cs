using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Singleplayer
{
    public class EffectCardApplier : MonoBehaviour, IDropHandler
    {
        [SerializeField] private GameObject inputBlock;

        public Action<BaseEffectCardLogic> OnEffectCardUsed;

        private Image background;

        private IEntity chosenTarget;
        private Coroutine choosingRoutine;

        public static EffectCardApplier Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
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
            /*inputBlock.SetActive(true);*/
            Debug.Log("OnEffectCardDrop");
            GameObject usedCard = eventData.pointerDrag;
            BaseEffectCard effectCard = usedCard.GetComponent<BaseEffectCard>();
            /*effectCard.parentAfterDrag = transform;
            effectCard.UseCard();*/

            effectCard.TryToUseCard();
        }

        public void StartChoosingTarget(Action<IEntity> callback)
        {
            StartCoroutine(ActivateChoosing(callback));
        }

        private IEnumerator ActivateChoosing(Action<IEntity> callback)
        {
            Debug.Log("Choosing a target...");
            IEntity chosen = null;

            choosingRoutine = StartCoroutine(WaitForEntitySelection(entity =>
            {
                chosen = entity;
            }));

            yield return new WaitUntil(() => chosen != null);
            callback?.Invoke(chosen);
        }

        private IEnumerator WaitForEntitySelection(Action<IEntity> callback)
        {
            chosenTarget = null;
            while (chosenTarget == null)
                yield return null;
            callback(chosenTarget);
            choosingRoutine = null;
        }
    }
}