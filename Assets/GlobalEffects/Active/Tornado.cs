using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    namespace ActiveEffects
    {
        public class Tornado : BaseActiveGlobalEffect
        {
            public Tornado(ActiveGlobalEffectInfo activeGlobalEffectInfo, IEntity entityOwner = null) : base(activeGlobalEffectInfo, entityOwner)
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

                if (entities.Count <= 1)
                    return;

                List<Vector2> originalPositions = entities
                    .Select(e => new Vector2(((MonoBehaviour)e).transform.position.x, ((MonoBehaviour)e).transform.position.y))
                    .ToList();

                List<Vector2> shuffledPositions = new List<Vector2>(originalPositions);
                ShuffleWithoutFixedPoints(shuffledPositions, originalPositions);

                for (int i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];

                    entity.SuppressPanelEffectTrigger = true;

                    gameManager.TeleportEntity(shuffledPositions[i], entity, null);
                }

                OnGlobalEffectStateChange();
            }

            private void ShuffleWithoutFixedPoints<T>(List<T> list, List<T> original)
            {
                if (list.Count <= 1)
                    return;

                System.Random rng = new System.Random();
                int n = list.Count;

                bool hasFixedPoint;

                do
                {
                    for (int i = n - 1; i > 0; i--)
                    {
                        int k = rng.Next(i + 1);
                        (list[i], list[k]) = (list[k], list[i]);
                    }

                    hasFixedPoint = false;
                    for (int i = 0; i < n; i++)
                    {
                        if (EqualityComparer<T>.Default.Equals(list[i], original[i]))
                        {
                            hasFixedPoint = true;
                            break;
                        }
                    }

                } while (hasFixedPoint);
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
