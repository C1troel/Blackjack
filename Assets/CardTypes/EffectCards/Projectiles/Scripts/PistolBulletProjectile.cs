using Singleplayer;
using Singleplayer.ActiveEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class PistolBulletProjectile : MonoBehaviour, IProjectile
    {
        private const int DAMAGE = 20;
        private const string EXPLOSION_TRIGGER = "Explode"; // заглушка
        private const string FLY_ANIMATION_NAME = "FireballFly"; // заглушка

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Action<IProjectile> projectileActivityEndCallback;
        private Action entityOnCompleteCallback;

        private IEntity targetEntity;
        private PanelScript landingPanel;

        public EffectCardInfo effectCardInfo { get; private set; }
        public IEntity entityOwner { get; private set; }

        void Start()
        { }

        void Update()
        { }

        public void Initialize(Action onComplete, IEntity targetEntity, IEntity entityOwner, PanelScript landingPanel, EffectCardInfo effectCardInfo)
        {
            this.targetEntity = targetEntity;
            this.entityOwner = entityOwner;
            this.effectCardInfo = effectCardInfo;
            this.landingPanel = landingPanel;
            animator.speed = 0;
            animator.enabled = true;
            entityOnCompleteCallback = onComplete;

            ProjectileManager.Instance.AddAvaitingProjectile(this);

            if (GlobalEffectsManager.Instance.IsTimeStopped)
            {
                entityOnCompleteCallback?.Invoke();
                entityOnCompleteCallback = null;
                return;
            }

            /*StartProjectileActivity(null);*/
        }

        public void StartProjectileActivity(Action<IProjectile> callback)
        {
            projectileActivityEndCallback = callback;
            FlowToTarget();
        }

        private void FlowToTarget()
        {
            animator.enabled = true;

            AnimationClip targetClip = animator.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == FLY_ANIMATION_NAME);

            if (targetClip == null)
            {
                Debug.LogError($"Animation clip '{FLY_ANIMATION_NAME}' not found!");
                return;
            }

            float animationLength = targetClip.length;

            var targetPosition = landingPanel.transform.position;
            float distanceLeft = Vector3.Distance(transform.position, targetPosition);

            float speed = distanceLeft / animationLength;

            StartCoroutine(MoveToTarget(speed, animationLength));
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

        private void Explode() // пул€ не взриваЇтьс€ особо, але може щось придумати, на даний момент заглушка
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
            entityOnCompleteCallback?.Invoke();
            projectileActivityEndCallback?.Invoke(this);
            Destroy(gameObject);
        }

        /*private void TryToDamageTarget()
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
                    var enemy = targetEntity as BaseEnemy;
                    possibleCounterCards = enemy.EnemyEffectCardsHandler.GetCounterCards(effectCardInfo.EffectCardDmgType);
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
                        HandleEnemyCounterCardUsage(targetEntity, possibleCounterCards[0]);
                        break;

                    case EntityType.Ally:
                        break;

                    default:
                        break;
                }
            }
        }

        private void HandleEnemyCounterCardUsage(IEntity entity, IEffectCardLogic effectCard)
        {
            var enemy = entity as BaseEnemy;

            effectCard.TryToUseCard(isUsed =>
            {
                enemy.EnemyEffectCardsHandler.RemoveEffectCard(effectCard);
                OnCounterCardUsed(effectCard);
            }, enemy);
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
        }*/

        private void TryToDamageTarget()
        {
            if (!landingPanel.EntitiesOnPanel.Contains(targetEntity))
            {
                Debug.Log($"Entity {targetEntity.GetEntityName} escaped from landed projectile panel {landingPanel.name}");
                Explode();
                return;
            }

            List<IEffectCardLogic> possibleCounterCards = null;
            BaseActiveGlobalEffect targetAbility = null;
            bool showAbilityButton = false;
            bool hasStrongAbility = false;

            switch (targetEntity.GetEntityType)
            {
                case EntityType.Player:
                    {
                        var player = targetEntity as BasePlayerController;
                        possibleCounterCards = player.EffectCardsHandler.GetCounterCards(effectCardInfo.EffectCardDmgType);
                        targetAbility = player.SpecialAbility;

                        if (targetAbility != null)
                        {
                            var abilityVulnerabilities = targetAbility.ActiveGlobalEffectInfo.Vulnerabilities;
                            var abilityInteractionStrength = CombatInteractionEvaluator.Evaluate(
                                abilityVulnerabilities,
                                effectCardInfo.EffectCardDmgType
                            );

                            hasStrongAbility = abilityInteractionStrength == CombatInteractionEvaluator.InteractionStrength.Strong
                                               && targetAbility.CooldownRounds == 0;

                            showAbilityButton = hasStrongAbility; // дл€ UI у игрока
                        }
                        break;
                    }

                case EntityType.Enemy:
                    {
                        var enemy = targetEntity as BaseEnemy;
                        possibleCounterCards = enemy.EnemyEffectCardsHandler.GetCounterCards(effectCardInfo.EffectCardDmgType);

                        if (enemy.SpecialAbility != null)
                        {
                            var abilityVulnerabilities = enemy.SpecialAbility.ActiveGlobalEffectInfo.Vulnerabilities;
                            var interactionStrength = CombatInteractionEvaluator.Evaluate(
                                abilityVulnerabilities,
                                effectCardInfo.EffectCardDmgType
                            );

                            hasStrongAbility = interactionStrength == CombatInteractionEvaluator.InteractionStrength.Strong
                                               && enemy.SpecialAbility.CooldownRounds == 0;
                        }
                        break;
                    }

                case EntityType.Ally:
                    // пока нет логики дл€ союзников
                    break;

                default:
                    break;
            }

            bool hasCards = possibleCounterCards != null && possibleCounterCards.Count > 0;

            // если нет защиты ни картами, ни способностью Ч наносим урон
            if (!hasCards && !hasStrongAbility)
            {
                DamageTarget();
                return;
            }

            if (targetEntity is BasePlayerController playerTarget)
            {
                // игроку показываем UI с кнопками карт и/или абилки
                playerTarget.ShowCounterOptions(
                    possibleCounterCards,
                    OnCounterCardUsed,
                    OnAbilityCounterUsedProjectile,
                    showAbilityButton
                );
            }
            else if (targetEntity is BaseEnemy enemyTarget)
            {
                // враг сам решает Ч карта или абилка
                IEffectCardLogic cardToUse = hasCards ? possibleCounterCards[0] : null;
                HandleEnemyCounterCardUsage(enemyTarget, cardToUse, hasStrongAbility);
            }
        }

        private void HandleEnemyCounterCardUsage(BaseEnemy enemy, IEffectCardLogic effectCard, bool strongAbility)
        {
            if (effectCard != null)
            {
                effectCard.TryToUseCard(isUsed =>
                {
                    enemy.EnemyEffectCardsHandler.RemoveEffectCard(effectCard);
                    OnCounterCardUsed(effectCard);
                }, enemy);

                return;
            }

            if (strongAbility)
                enemy.SpecialAbility.TryToActivate();
        }

        // коллбек дл€ карты
        private void OnCounterCardUsed(IEffectCardLogic usedEffectCard)
        {
            if (usedEffectCard == null)
            {
                Debug.Log("Player skipped countering incoming projectile with card");
                return;
            }

            OnProjectileCountered();
        }

        // коллбек дл€ абилки
        private void OnAbilityCounterUsedProjectile(bool usedAbility)
        {
            if (!usedAbility)
            {
                Debug.Log("Player skipped countering incoming projectile with ability");
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
            GameManager.Instance.DealDamage(targetEntity, DAMAGE, false, effectCardInfo.EffectCardDmgType);
            Debug.Log($"Entity {targetEntity.GetEntityName} health after dealing damage: {targetEntity.GetEntityHp}");
        }

        public Action ReflectProjectile()
        {
            var returnedCallback = entityOnCompleteCallback;
            entityOnCompleteCallback = null;
            return returnedCallback;
        }
    }
}