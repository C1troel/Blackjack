using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class Explosive : BaseEffectCardLogic
    {
        private const int OUTPUT_DAMAGE = 50;
        private const int SELF_DAMAGE = 30;

        public override IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null)
        {
            var gameManager = GameManager.Instance;
            var nearEntities = MapManager
                .FindEntitiesAtDistance(entityInit.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels)
                .Where(e => e != entityInit)
                .ToList();

            foreach (var entity in nearEntities)
                gameManager.DealDamage(entity, OUTPUT_DAMAGE, false, EffectCardInfo.EffectCardDmgType);

            gameManager.DealDamage(entityInit, SELF_DAMAGE, false, EffectCardInfo.EffectCardDmgType);
            yield return null;
            onComplete?.Invoke();
        }

        public override void TryToUseCard(Action<bool> onComplete, IEntity entityInit)
        {
            if (!CheckForAvailableTargets())
            {
                onComplete?.Invoke(false);
                return;
            }

            MapManager.Instance.OnEffectCardPlayedByEntity(() =>
                GameManager.Instance.StartCoroutine(
                    ApplyEffect(() =>
                    {
                        onComplete?.Invoke(true);
                    }, entityInit)),
                this);
        }

        private bool CheckForAvailableTargets()
        {
            return TargetEnemiesList.Any(entity =>
                entity.GetEntityHp > 0 &&
                entity.GetCurrentPanel.GetEffectPanelInfo.Effect != PanelEffect.VIPClub);
        }
    }
}