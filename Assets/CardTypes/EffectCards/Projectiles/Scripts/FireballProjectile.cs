using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class FireballProjectile : MonoBehaviour, IProjectile
    {
        private const string EXPLOSION_TRIGGER = "Explode";
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Action<IProjectile> OnProjectileActivityEnd;

        private IEntity targetEntity;
        private PanelScript landingPanel;
        private EffectCardInfo effectCardInfo;
        private int damage;

        void Start()
        {}

        void Update()
        {}

        public void Initialize(IEntity targetEntity, PanelScript landingPanel, int damage , EffectCardInfo effectCardInfo)
        {
            this.targetEntity = targetEntity;
            this.effectCardInfo = effectCardInfo;
            this.damage = damage;
            this.landingPanel = landingPanel;
            animator.speed = 0;
            animator.enabled = true;

            if (GlobalEffectsManager.Instance.isTimeStopped)
            {
                ProjectileManager.Instance.AddAvaitingProjectile(this);
                return;
            }

            StartProjectileActivity(null);
        }

        public void StartProjectileActivity(Action<IProjectile> callback)
        {
            FlowToTarget();
        }

        private void FlowToTarget()
        {
            animator.enabled = true;
            var targetPositon = landingPanel.transform.position;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animationLength = stateInfo.length; // ƒлительность текущей анимации в секундах
            float normalizedTime = stateInfo.normalizedTime % 1f; // ѕрогресс от 0 до 1 (если looping, берем модуль)

            float timeLeft = animationLength * (1f - normalizedTime); // —колько времени осталось анимации

            float distanceLeft = Vector3.Distance(transform.position, targetPositon);

            float speed = distanceLeft / timeLeft;

            StartCoroutine(MoveToTarget(speed, timeLeft));
        }

        IEnumerator MoveToTarget(float speed, float timeLeft)
        {
            animator.speed = 1;
            float timer = 0f;
            var targetPositon = landingPanel.transform.position;
            Vector3 startPosition = transform.position;

            while (timer < timeLeft)
            {
                timer += Time.deltaTime;
                float t = timer / timeLeft;
                transform.position = Vector3.Lerp(startPosition, targetPositon, t);
                yield return null;
            }

            transform.position = targetPositon;
        }

        public void OnAnimationFlyEnd()
        {
            animator.speed = 0;
            TryToDamageTarget();
        }

        private void Explode()
        {
            animator.speed = 1;
            animator.SetTrigger(EXPLOSION_TRIGGER);
        }

        public void OnExplosionEnd()
        {
            DeleteProjectile();
        }

        private void DeleteProjectile()
        {
            Debug.Log("Delete projectile call");
            OnProjectileActivityEnd?.Invoke(this);
            Destroy(gameObject);
        }

        private void TryToDamageTarget()
        {
            if (!landingPanel.EntitiesOnPanel.Contains(targetEntity))
            {
                Debug.Log($"Entity {targetEntity.GetEntityName} escaped from landed projectile panel {landingPanel.name}");
                Explode();
                return;
            }

            List<IEffectCardLogic> possibleCounterCards = null;

            switch (targetEntity.GetEntityType)
            {
                case EntityType.Player:
                    var player = targetEntity as BasePlayerController;
                    possibleCounterCards = player.EffectCardsHandler.GetCounterCards(effectCardInfo.EffectCardDmgType);
                    break;

                case EntityType.Enemy:
                    Debug.Log($"Checking for possible counter cards in enemy {targetEntity.GetEntityName}");
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            if (possibleCounterCards == null || possibleCounterCards.Count == 0)
                DamageTarget();
            else
            {
                switch (targetEntity.GetEntityType)
                {
                    case EntityType.Player:
                        var player = targetEntity as BasePlayerController;
                        player.ShowCounterCardOptions(possibleCounterCards, OnCounterCardUsed);
                        break;

                    case EntityType.Enemy:
                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }
            }
        }

        private void OnCounterCardUsed(IEffectCardLogic usedEddectCard)
        {
            if (usedEddectCard == null)
            {
                Debug.Log("Player is skipped countering incoming projectile");
                DamageTarget();
                return;
            }

            OnProjectileCountered();
        }

        private void OnProjectileCountered()
        {
            DeleteProjectile();
        }

        private void DamageTarget()
        {
            Debug.Log($"Entity {targetEntity.GetEntityName} health before dealing damage: {targetEntity.GetEntityHp}");
            Explode();
            GameManager.Instance.DealDamage(targetEntity, damage, false);
            Debug.Log($"Entity {targetEntity.GetEntityName} health after dealing damage: {targetEntity.GetEntityHp}");
        }
    }
}