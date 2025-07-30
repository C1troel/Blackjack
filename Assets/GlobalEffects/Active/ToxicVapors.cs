using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class ToxicVapors : BaseActiveGlobalEffect
        {
            public ToxicVapors(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
            {}

            public override void TryToActivate()
            {
                if (CooldownRounds > 0)
                {
                    Debug.Log($"Ability {this} isn`t ready for activation, wait {CooldownRounds} turns more");
                    return;
                }

                CooldownRounds = this.ActiveGlobalEffectInfo.CooldownRounds;

                List<GameObject> randomPanels = MapManager.Instance.panels
                    .OrderBy(x => Random.value)
                    .Take(6)
                    .ToList();

                var toxicGasPrefab = GlobalEffectsManager.Instance.GetToxicGasPrefab;

                foreach (var panel in randomPanels)
                {
                    var spawnedGasGO = GlobalEffectsManager.Instantiate(toxicGasPrefab, panel.transform.position, Quaternion.identity);
                    var spawnedGasController = spawnedGasGO.GetComponent<ToxicGasController>();
                    spawnedGasController.Initialize(ActiveGlobalEffectInfo.TurnDuration);
                }
            }
        }
    }
}