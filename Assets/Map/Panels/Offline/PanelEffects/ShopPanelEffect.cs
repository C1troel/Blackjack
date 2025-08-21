using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class ShopPanelEffect : IPanelEffect
    {
        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if (entity.GetEntityType != EntityType.Player)
            {
                EffectCardDealer.Instance.DealRandomEffectCard(entity);
                onComplete?.Invoke();
                yield break;
            }

            var shoppingController = GameManager.Instance.GetShoppingController;
            shoppingController.StartShopping();

            yield return new WaitUntil(() => shoppingController.IsPlayerShopping == false);

            onComplete?.Invoke();
        }
    }
}