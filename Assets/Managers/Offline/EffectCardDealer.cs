using Singleplayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Singleplayer
{
    public class EffectCardDealer : MonoBehaviour
    {
        [SerializeField] private GameObject effectCardPrefab;
        [SerializeField] private GridLayoutGroup playerEffectCardsContainer;

        private List<EffectCardInfo> effectCardInfosList;

        public static EffectCardDealer Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            effectCardInfosList = InfosLoadManager.Instance.GetAllEffectCardInfos().ToList();
        }

        private void AttachEffectCardScript(GameObject effectCardGO, EffectCardType effectCardType)
        {
            string typeName = $"{this.GetType().Namespace}.{effectCardType}Card";
            GameManager.AddComponentByName(effectCardGO, typeName);
        }

        private IEffectCardLogic GetEffectCardLogicInstance(EffectCardInfo effectCardInfo)
        {
            var logicType = Type.GetType($"{effectCardInfo.EffectCardType}");

            if (logicType == null)
            {
                Debug.LogError($"Unknown logic type: {effectCardInfo.EffectCardType}");
                return null;
            }

            IEffectCardLogic effectCardLogic = Activator.CreateInstance(logicType) as IEffectCardLogic;
            effectCardLogic.SetupEffectCardLogic(effectCardInfo);

            return effectCardLogic;
        }

        public void DealRandomEffectCard(IEntity entity)
        {
            var dealtCardInfo = effectCardInfosList[Random.Range(0, effectCardInfosList.Count)];

            switch (entity.GetEntityType)
            {
                case EntityType.Player:

                    var dealtCardGO = Instantiate(effectCardPrefab);
                    AttachEffectCardScript(dealtCardGO, dealtCardInfo.EffectCardType);

                    var dealtCard = dealtCardGO.GetComponent<BaseEffectCard>();
                    dealtCard.SetupEffectCard(dealtCardInfo);

                    dealtCardGO.transform.SetParent(playerEffectCardsContainer.transform);

                    break;

                case EntityType.Enemy:

                    var enemy = entity as BaseEnemy;

                    var effectCardLogicInstance = GetEffectCardLogicInstance(dealtCardInfo);

                    enemy.ReceiveEffectCard(effectCardLogicInstance);

                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }
    }
}