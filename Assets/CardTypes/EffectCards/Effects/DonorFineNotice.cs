using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class DonorFineNotice : BaseEffectCardLogic
    {
        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            PanelScript targetPanel = null;
            var mapManager = GameManager.Instance;

            entityInit ??= GameManager.Instance.GetEntityWithType(EntityType.Player);

            var panelsInRadius = MapManager.GetPanelsInRadius(entityInit.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels);

            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                    MapManager.Instance.StartChoosingPanel(choosedPanel =>
                    {
                        targetPanel = choosedPanel;
                    }, panelsInRadius);
                    break;

                case EntityType.Enemy:
                    targetPanel = panelsInRadius[Random.Range(0, panelsInRadius.Count)];
                    break;
            }

            yield return new WaitUntil(() => targetPanel != null);

            Debug.Log($"{GetType().Name} target: {targetPanel.name}");
            LeaveDonorNotice(targetPanel);

            onComplete?.Invoke();
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }

        public override bool CheckIfCanBeUsed(IEntity entityOwner)
        {
            CanUse = true;
            return true;
        }

        private void LeaveDonorNotice(PanelScript targetPanel)
        {
            var donorFineNoticePrefab = GlobalEffectsManager.Instance.GetDonorFineNoticePrefab;
            var donorFineNoticeGO = MapManager.Instantiate(donorFineNoticePrefab, targetPanel.transform.position, Quaternion.identity);
        }
    }
}