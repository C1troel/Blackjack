using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.PassiveEffects;

namespace Singleplayer
{
    public interface IEntity
    {
        void StartMove(Direction direction = (Direction)(-1), PanelScript panel = null); // PanelScript також потрібен офлайновий
        void MoveEnd();
        void StopMoving();
        void GetDamage(int value);
        void RaiseAtkStat(int value);
        void Heal(int value);
        void GetSteps(int value);
        void OnNewTurnStart();
        void StartTurn();
        IEnumerator Move(IEntity entity, Direction direction = (Direction)(-1), PanelScript panel = null); // PanelScript та PlayerController також потрібен офлайновий
        IEnumerator OnStepOntoPanel(PanelScript panel);
        IEnumerator StopAnimationSmoothly(float duration);
        IEnumerator ResumeAnimationSmoothly(float duration);
        void SetOutline();
        void RemoveOutline();
        void TurnEntity();
        void EnableAttacking();
        void DecreaseEffectCardsUsages();

        event Action<IEntity> moveEndEvent;
        event Action<IEntity> OnSelfClickHandled;
        bool SuppressPanelEffectTrigger { get; set; }
        string GetEntityName { get; }
        EntityType GetEntityType { get; }
        PanelScript GetCurrentPanel { get; }
        int GetEntityDef { get; }
        int GetEntityHp { get; }
        int GetEntityMaxHp { get; }
        int GetEntityMoney { get; }
        int GetEntityAtk { get; }
        int GetEntityLeftCards { get; }
        bool GetEntityAttackAccess { get; }
        Direction GetEntityDirection { get; }
        IPassiveEffectHandler PassiveEffectHandler { get; }
        Animator Animator { get; }
    }

    public enum Direction
    {
        Standart = -1,
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3,
    };

    public enum EntityType
    {
        Player = 0,
        Enemy = 1,
        Ally = 2
    }
}
