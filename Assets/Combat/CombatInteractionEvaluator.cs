using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    public static class CombatInteractionEvaluator
    {
        public enum InteractionStrength
        {
            Strong,
            Weak,
            Neutral
        }

        public static InteractionStrength Evaluate(List<VulnerabilityDescriptor> descriptors, PassiveEffectType target)
        {
            foreach (var d in descriptors)
            {
                if (d.Matches(target))
                    return d.Kind == VulnerabilityKind.Strong ? InteractionStrength.Strong : InteractionStrength.Weak;
            }
            return InteractionStrength.Neutral;
        }

        public static InteractionStrength Evaluate(List<VulnerabilityDescriptor> descriptors, EffectCardDmgType target)
        {
            foreach (var d in descriptors)
            {
                if (d.Matches(target))
                    return d.Kind == VulnerabilityKind.Strong ? InteractionStrength.Strong : InteractionStrength.Weak;
            }
            return InteractionStrength.Neutral;
        }

        public static InteractionStrength Evaluate(List<VulnerabilityDescriptor> descriptors, EffectCardMaterial target)
        {
            foreach (var d in descriptors)
            {
                if (d.Matches(target))
                    return d.Kind == VulnerabilityKind.Strong ? InteractionStrength.Strong : InteractionStrength.Weak;
            }
            return InteractionStrength.Neutral;
        }
    }
}