using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class DonorFineNoticeController : MonoBehaviour, IMapObject
    {
        [SerializeField] private Animator animator;
        private const string FINE_SIGNING_TRIGGER = "Sign";
        private const int DAMAGE = 30;

        private PanelScript currentPanel;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PanelScript>(out var panel)) return;

            currentPanel = panel;
        }

        private void ApplyDamage(IEntity entity)
        {
            animator.SetTrigger(FINE_SIGNING_TRIGGER);
            GameManager.Instance.DealDamage(entity, DAMAGE, false);
        }

        private void OnFineSigningEnd()
        {
            animator.enabled = false;
            currentPanel.TryToRemoveMapObject(gameObject);
            Destroy(gameObject);
        }

        public void OnEntityStay(Action onCompleteCallback, IEntity stayedEntity)
        {
            ApplyDamage(stayedEntity);
            onCompleteCallback?.Invoke();
        }
    }
}