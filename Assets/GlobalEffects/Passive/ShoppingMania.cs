using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    namespace PassiveEffects
    {
        public class ShoppingMania : BasePassiveGlobalEffect
        {
            private const int SHOP_CARDS_RAISE_AMOUNT = 3;
            public ShoppingMania(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
            {}

            public ShoppingMania(int turns) : base(turns)
            {}

            public override void HandlePassiveEffect(IEntity entityOwner)
            {
                if (entityOwner != null)
                {
                    Debug.LogWarning($"ShoppingMania effect accidentally being apply to entity: {entityOwner.GetEntityName}");
                    TurnsRemaining = 0;
                    return;
                }

                GameManager.Instance.GetShoppingController.RaiseSellableCardsAmount(SHOP_CARDS_RAISE_AMOUNT);
            }

            public override void EndPassiveEffect(IEntity entityInit)
            {
                GameManager.Instance.GetShoppingController.RaiseSellableCardsAmount(0);
            }
        }
    }
}