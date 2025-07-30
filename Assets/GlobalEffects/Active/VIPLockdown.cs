using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class VIPLockdown : BaseActiveGlobalEffect
        {
            public VIPLockdown(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
            {}

            public override bool IsUsable()
            {
                return CooldownRounds <= 0;
            }

            public override void TryToActivate()
            {
                if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                var gameManager = GameManager.Instance;
                var entities = gameManager.GetEntitiesList();

                var vipClubPanels = MapManager.Instance.GetAllPanelsOfType(PanelEffect.VIPClub);
                var randomVIPClubPanel = vipClubPanels[Random.Range(0, vipClubPanels.Count)];


                foreach (var entity in entities)
                {
                    entity.SuppressPanelEffectTrigger = true;
                    gameManager.TeleportEntity(randomVIPClubPanel.transform.position, entity);
                }
            }

            public override void OnNewTurnStart()
            {
                if (CooldownRounds <= 0)
                {
                    Debug.Log($"Ability {this} is already ready for activation");
                    return;
                }

                CooldownRounds--;
                Debug.Log($"{this} ability cooldown reset in {CooldownRounds} turns");

                if (CooldownRounds == 0)
                {
                    Debug.Log($"Ability {this} is now ready!");
                    OnGlobalEffectStateChange();
                }
            }
        }
    }
}
