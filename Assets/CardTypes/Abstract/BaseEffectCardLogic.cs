using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public enum EffectCardType
    {
        SmallMedicine,
        Fireball,
        BallsOfSteel,
        Hourglass,
        BigAttackPack,
        BigDefensePack,
        SmallAttackPack,
        MediumAttackPack,
        SmallDefensePack,
        MediumDefensePack,
        MediumMedicine,
        PoisonFlask,
        Pistol,
        Explosive,
        Rage,
        BackstabKnife,
        Regroup,
        DonorFineNotice,
        GoldenMagnet,
        UnstableTeleporter,
        Mirror,
        KindlyMirror,
        Magic8Ball,
        BigSwing,
        SplitReach,
        Trick
    }

    public enum EffectCardPurpose
    {
        Action,
        Defensive,
        BattleAttack,
        BattleDefense
    }

    public abstract class BaseEffectCardLogic : IEffectCardLogic
    {
        public List<IOutlinable> TargetObjectsList { get; protected set; }
        public EffectCardInfo EffectCardInfo { get; protected set; }
        public bool CanUse {  get; protected set; }

        public bool CanCounter { get; protected set; }
        /*public List<EffectCardMaterial> EffectCardMaterials { get; protected set; }
        public EffectCardDmgType EffectCardDmgType { get; protected set; }*/

        public virtual void SetupEffectCardLogic(EffectCardInfo info)
        {
            EffectCardInfo = info;
            /*EffectCardMaterials = info.EffectCardMaterials;
            EffectCardDmgType = info.EffectCardDmgType;*/
        }

        public virtual bool CheckIfCanBeUsed(IEntity entityOwner)
        {
            if (EffectCardInfo.EffectiveDistanceInPanels == 0 &&
                EffectCardInfo.EffectCardPurposes.Any(purpose => purpose == EffectCardPurpose.Action))
            {
                CanUse = true;
                return true;
            }
            else if (EffectCardInfo.EffectiveDistanceInPanels == 0)
            {
                CanUse = false;
                return false;
            }

            var entitiesInEffectiveCardRadius = MapManager.FindEntitiesAtDistance(entityOwner.GetCurrentPanel, EffectCardInfo.EffectiveDistanceInPanels);

            entitiesInEffectiveCardRadius.RemoveAll(entity => entity.GetEntityHp <= 0);

            switch (entityOwner.GetEntityType)
            {
                case EntityType.Player:
                    entitiesInEffectiveCardRadius.RemoveAll(entity => entity.GetEntityType == entityOwner.GetEntityType ||
                    entity.GetEntityType == EntityType.Ally);
                    break;

                case EntityType.Enemy:
                    entitiesInEffectiveCardRadius.RemoveAll(entity => entity.GetEntityType == entityOwner.GetEntityType);
                    break;

                case EntityType.Ally:
                    entitiesInEffectiveCardRadius.RemoveAll(entity => entity.GetEntityType == entityOwner.GetEntityType ||
                    entity.GetEntityType == EntityType.Player);
                    break;

                default:
                    break;
            }

            if (entitiesInEffectiveCardRadius.Count == 0)
            {
                CanUse = false;
                return false;
            }

            CanUse = true;
            TargetObjectsList = entitiesInEffectiveCardRadius.Cast<IOutlinable>().ToList();
            return true;
        }
        public void ToggleMarkAsCounterCard(bool isMarked)
        {
            CanCounter = isMarked;
        }

        public abstract IEnumerator ApplyEffect(Action onComplete, IEntity entityInit = null);
        public abstract void TryToUseCard(Action<bool> onComplete, IEntity entityInit);
    }
}