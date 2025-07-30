using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class DonorFineNoticeController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private const string FINE_SIGNING_TRIGGER = "Sign";
        private const int DAMAGE = 30;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<IEntity>(out var entity)) return;

            ApplyDamage(entity);
        }

        private void ApplyDamage(IEntity entity)
        {
            animator.SetTrigger(FINE_SIGNING_TRIGGER);
            GameManager.Instance.DealDamage(entity, DAMAGE, false);
        }

        private void OnFineSigningEnd()
        {
            animator.enabled = false;
            Destroy(gameObject);
        }
    }
}