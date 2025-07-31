using Singleplayer.PassiveEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class GlobalEffectsManager : MonoBehaviour
    {
        [SerializeField] private GameObject toxicGasPrefab;
        [SerializeField] private GameObject donorFineNoticePrefab;
        private const string TIME_STOP_BYPASS_LAYER = "ColorObject";
        private List<BasePassiveGlobalEffect> activeEffectsList = new List<BasePassiveGlobalEffect>();
        private CameraController mainPlayerCamera;

        public bool isTimeStopped { get; private set; }
        public static GlobalEffectsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            StartCoroutine(AwaitAndCacheMainPlayerCamera());
        }

        void Update()
        {

        }

        private IEnumerator AwaitAndCacheMainPlayerCamera()
        {
            var gameManager = GameManager.Instance;
            yield return new WaitUntil(() => gameManager.GetEntitiesList().Count != 0);

            var player = gameManager.GetEntityWithType(EntityType.Player) as BasePlayerController;
            mainPlayerCamera = player.PlayerCamera.GetComponent<CameraController>();
        }

        public void StopTime(IEntity entityInit)
        {
            if (isTimeStopped)
                { return; }

            isTimeStopped = true;
            mainPlayerCamera.FadeInGrayscale(1);

            TryToFreezeAllInStoppedTime();
        }

        public void TryToResumeTime(IEntity entityInit)
        {
            if (CheckForTimeStoppers(entityInit))
                return;

            isTimeStopped = false;

            mainPlayerCamera.FadeOutGrayscale(1);

            TryToResumeAllFromStoppedTime();
        }

        public bool CheckForTimeStoppers(IEntity entityInit)
        {
            return GameManager.Instance.GetEntitiesList()
                .Any(entity =>
                    entity != entityInit &&
                    entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.TimeStop));
        }

        private void TryToFreezeAllInStoppedTime()
        {
            var entities = GameManager.Instance.GetEntitiesList();

            foreach (var entity in entities)
            {
                if (!entity.PassiveEffectHandler.CheckForActiveEffectType(PassiveEffectType.Chronomaster))
                    StartCoroutine(entity.StopAnimationSmoothly(2));
            }
        }

        private void TryToResumeAllFromStoppedTime()
        {
            var entities = GameManager.Instance.GetEntitiesList();

            foreach (var entity in entities)
            {
                if (entity.Animator.speed < 1)
                    StartCoroutine(entity.ResumeAnimationSmoothly(2));
            }

            ProjectileManager.Instance.OnTimeStopEnd();
        }

        private void AddPassiveEffect(BasePassiveGlobalEffect effect)
        {
            this.AddPassiveEffect(effect);
        }

        private void HandlePassiveEffects()
        {
            foreach (var passiveEffect in activeEffectsList)
            {
                if (passiveEffect.TurnsRemaining == 0)
                    RemovePassiveEffect(passiveEffect);
            }

            foreach (var effect in activeEffectsList)
                effect.HandlePassiveEffect(null);
        }

        private void RemovePassiveEffect(BasePassiveGlobalEffect endedEffect)
        {
            endedEffect.EndPassiveEffect(null);
            activeEffectsList.Remove(endedEffect);
        }

        public GameObject GetToxicGasPrefab => toxicGasPrefab;
        public GameObject GetDonorFineNoticePrefab => donorFineNoticePrefab;
    }
}