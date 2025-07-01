using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IEntity
    {
        void StartMove(Direction direction = (Direction)(-1), PanelScript panel = null); // PanelScript також потрібен офлайновий
        void MoveEnd();
        void StopMoving();
        void SetupEntity(string entityName);
        void GetDamage(int value);
        void Heal(int value);
        void GetSteps(int value);
        IEnumerator Move(PlayerController player, Direction direction = (Direction)(-1), PanelScript panel = null); // PanelScript та PlayerController також потрібен офлайновий
        void TurnEntity();
        void EnableAttacking();

        event Action<IEntity> moveEndEvent;
        string GetEntityName { get; }
        EntityType GetEntityType { get; }
        PanelScript GetCurrentPanel { get; }
        int GetEntityDef { get; }
        int GetEntityHp { get; }
        int GetEntityMaxHp { get; }
        int GetEntityMoney { get; }
        int GetEntityChips { get; }
        int GetEntityAtk { get; }
        int GetEntityLeftCards { get; }
        bool GetEntityAttackAccess { get; }
        string GetEntitySuit { get; }
        Direction GetEntityDirection { get; }

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
