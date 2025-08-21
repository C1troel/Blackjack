using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Singleplayer
{
    public class EnemyHpController : MonoBehaviour
    {
        private TextMeshPro hpText;
        private BaseEnemy handledEnemy;

        private void Start()
        {
            hpText = GetComponent<TextMeshPro>();
            handledEnemy = GetComponentInParent<BaseEnemy>();

            if (handledEnemy == null)
            {
                Debug.LogWarning($"{GetType().Name} trying to attach to {transform.parent} but this is not BaseEnemy");
            }

            handledEnemy.OnHpChange += OnEnemyHpChange;
            OnEnemyHpChange();
        }

        private void OnEnemyHpChange()
        {
            hpText.text = $"{handledEnemy.GetEntityHp}/{handledEnemy.GetEntityMaxHp}";
        }
    }
}