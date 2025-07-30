using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Singleplayer.PassiveEffects;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class FatePanelEffect : IPanelEffect
    {
        public IEnumerator Execute(IEntity entity, Action onComplete)
        {
            if (Random.Range(0, 2) == 0)
                TriggerGoodEffect(entity);
            else
                TriggerBadEffect(entity);

            yield return null;
            onComplete?.Invoke();
        }

        private void TriggerBadEffect(IEntity entityInit)
        {
            var effectId = Random.Range(0, 7);
            var gameManager = GameManager.Instance;

            switch (effectId)
            {
                case 1: // ������� ������� ������ 20% �� ����. ������'�
                    {
                        Debug.Log("The entity immediately loses 20% of its max health.");
                        int initialDmg = (int)(entityInit.GetEntityMaxHp * 0.2f);
                        gameManager.DealDamage(entityInit, initialDmg, false);
                        break;
                    }

                case 2: // ������� ������ ����� "����"
                    Debug.Log("The entity gains the \"Wound\" effect");
                    GiveWoundEffect(entityInit);
                    break;

                case 3: // ������� ������ ��������� ������� �����
                    {
                        Debug.Log("The entity loses a random effect card.");
                        switch (entityInit.GetEntityType)
                        {
                            case EntityType.Player:
                                var player = entityInit as BasePlayerController;
                                player.EffectCardsHandler.RemoveRandomEffectCards(1);
                                break;

                            case EntityType.Enemy:
                                var enemy = entityInit as BaseEnemy;
                                enemy.EnemyEffectCardsHandler.RemoveRandomEffectCards(1);
                                break;

                            case EntityType.Ally:
                                break;

                            default:
                                break;
                        }

                        break;
                    }

                case 4: // ������� ������ 5$
                    {
                        Debug.Log("The player loses $5.");
                        var player = entityInit as BasePlayerController;
                        if (player != null)
                            player.Pay(5, false);
                        break;
                    }

                case 5: // ������� ������ 3 �������� "����� �����"
                    {
                        Debug.Log("The entity loses 3 random \"Playing Cards\"");
                        switch (entityInit.GetEntityType)
                        {
                            case EntityType.Player:
                                var player = entityInit as BasePlayerController;
                                player.EffectCardsHandler.RemoveRandomEffectCards(3);
                                break;

                            case EntityType.Enemy:
                                var enemy = entityInit as BaseEnemy;
                                enemy.EnemyEffectCardsHandler.RemoveRandomEffectCards(3);
                                break;

                            case EntityType.Ally:
                                break;

                            default:
                                break;
                        }

                        break;
                    }

                case 6: // ������� ������ �������� ������
                    {
                        Debug.Log("The player loses half of the money.");
                        var player = entityInit as BasePlayerController;
                        if (player != null)
                            player.Pay(player.GetEntityMoney / 2, false);
                        break;
                    }

                default:
                    break;
            }
        }

        private void TriggerGoodEffect(IEntity entityInit)
        {
            var effectId = Random.Range(0, 8);
            var gameManager = GameManager.Instance;

            switch (effectId)
            {
                case 1: // ������� ������� ������� 20% ������'�
                    Debug.Log("Entity instantly heals 20% health");
                    gameManager.Heal(entityInit, 20, true);
                    break;

                case 2: // ������� ������ ����� "��������"

                    GivePatchEffect(entityInit);
                    break;

                case 3: // ������� ������ ��������� ������� �����
                    Debug.Log("The entity gains the \"Patch\" effect");
                    EffectCardDealer.Instance.DealRandomEffectCard(entityInit);
                    break;

                case 4: // ������� ������ 5$
                    {
                        Debug.Log("The player receives $5");
                        var player = entityInit as BasePlayerController;
                        if (player != null)
                            player.GainMoney(5, false);
                        break;
                    }

                case 5: // ������� �������� 5$ � 10 �����
                    {
                        Debug.Log("Player converts $5 into 10 chips");
                        var player = entityInit as BasePlayerController;
                        if (player != null)
                        {
                            player.Pay(5, false);
                            player.GainMoney(10, true);
                        }

                        break;
                    }

                case 6: // ������� ������ 3 �������� "����� �����"
                    Debug.Log("The entity receives 3 random \"Game Cards\"");
                    for (int i = 0; i < 3; i++)
                        EffectCardDealer.Instance.DealRandomEffectCard(entityInit);
                    break;

                case 7: // ������� ������� ��� �����
                    {
                        Debug.Log("The player doubles his money.");
                        var player = entityInit as BasePlayerController;
                        if (player != null)
                            player.GainMoney(player.GetEntityMoney * 2, false);
                        break;
                    }

                default:
                    break;
            }
        }

        private void GivePatchEffect(IEntity entity)
        {
            var patchEffect = new Patch(1);

            entity.PassiveEffectHandler.TryToAddEffect(patchEffect);
        }

        private void GiveWoundEffect(IEntity entity)
        {
            var woundEffect = new Wound(1);

            entity.PassiveEffectHandler.TryToAddEffect(woundEffect);
        }
    }
}