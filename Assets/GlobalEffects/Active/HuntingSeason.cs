using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class HuntingSeason : BaseActiveGlobalEffect
        {
            public HuntingSeason(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
            {}

            public override void TryToActivate()
            {
                if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                var entities = GameManager.Instance.GetEntitiesList();
                var donorFineNoticePrefab = GlobalEffectsManager.Instance.GetDonorFineNoticePrefab;

                foreach (var entity in entities)
                {
                    var targetPanel = entity.GetCurrentPanel;
                    GlobalEffectsManager.Instantiate(donorFineNoticePrefab, targetPanel.transform.position, Quaternion.identity);
                }

                OnGlobalEffectStateChange();
            }
        }
    }
}