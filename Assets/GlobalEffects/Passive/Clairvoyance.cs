using Singleplayer.PassiveEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    public class Clairvoyance : BasePassiveGlobalEffect
    {
        public Clairvoyance(PassiveGlobalEffectInfo passiveGlobalEffectInfo) : base(passiveGlobalEffectInfo)
        {
            BattleManager.Instance.OnEntityDrawCard += OnFacedDownBattleCardDraw;
            BlackjackManager.Instance.OnEntityDrawCard += OnFacedDownBlackjackCardDraw;
        }

        public Clairvoyance(int turns) : base(turns)
        {
            BattleManager.Instance.OnEntityDrawCard += OnFacedDownBattleCardDraw;
            BlackjackManager.Instance.OnEntityDrawCard += OnFacedDownBlackjackCardDraw;
        }

        public override void HandlePassiveEffect(IEntity entityOwner)
        {
            Debug.Log($"{GetType().Name} passive effect is active");

            TurnsRemaining--;
        }

        private void OnFacedDownBattleCardDraw(NextCardScript takenCard, IEntity entityInit)
        {
            if (entityInit != entityOwner)
                return;

            var img = takenCard.gameObject.transform.Find("2Side").GetComponent<Image>();
            Color c = img.color;
            c.a = 100f/255f;
            img.color = c;
        }

        private void OnFacedDownBlackjackCardDraw(BlackjackCard takenCard, IEntity entityInit)
        {
            if (entityInit != entityOwner)
                return;

            var img = takenCard.gameObject.transform.Find("2Side").GetComponent<Image>();
            Color c = img.color;
            c.a = 100f / 255f;
            img.color = c;
        }

        public override void EndPassiveEffect(IEntity entityInit)
        {
            BattleManager.Instance.OnEntityDrawCard -= OnFacedDownBattleCardDraw;
            BlackjackManager.Instance.OnEntityDrawCard -= OnFacedDownBlackjackCardDraw;
            base.EndPassiveEffect(entityInit);
        }
    }
}