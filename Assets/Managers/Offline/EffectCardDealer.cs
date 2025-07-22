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
        [SerializeField] private PlayerEffectCardsHandler playerEffectCardsHandler;
        [SerializeField] private Material effectCardOutlineMaterial;

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
            var logicType = Type.GetType($"{this.GetType().Namespace}.{effectCardInfo.EffectCardType}");

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

                    playerEffectCardsHandler.AddEffectCard(dealtCard);

                    break;

                case EntityType.Enemy:

                    var enemy = entity as BaseEnemy;

                    var effectCardLogicInstance = GetEffectCardLogicInstance(dealtCardInfo);

                    enemy.EnemyEffectCardsHandler.AddEffectCard(effectCardLogicInstance);

                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }

        public void DealEffectCardOfType(IEntity entity, EffectCardType effectCardType)
        {
            var dealtCardInfo = effectCardInfosList.Find(card => card.EffectCardType == effectCardType);

            switch (entity.GetEntityType)
            {
                case EntityType.Player:

                    var dealtCardGO = Instantiate(effectCardPrefab);
                    AttachEffectCardScript(dealtCardGO, dealtCardInfo.EffectCardType);

                    var dealtCard = dealtCardGO.GetComponent<BaseEffectCard>();
                    dealtCard.SetupEffectCard(dealtCardInfo);

                    playerEffectCardsHandler.AddEffectCard(dealtCard);

                    break;

                case EntityType.Enemy:

                    var enemy = entity as BaseEnemy;

                    var effectCardLogicInstance = GetEffectCardLogicInstance(dealtCardInfo);

                    enemy.EnemyEffectCardsHandler.AddEffectCard(effectCardLogicInstance);

                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }

        public Material GetEffectCardOutlineMaterial => effectCardOutlineMaterial;
    }
}