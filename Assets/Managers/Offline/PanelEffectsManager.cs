using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using Panel;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using JSG.FortuneSpinWheel;
using Multiplayer.Panel;
using Random = UnityEngine.Random;
using Singeplayer;

namespace Singleplayer
{
    public class PanelEffectsManager : MonoBehaviour
    {
        private const int BACKSTABDMG = 20;
        private const int DESICIONS_COUNT = 2;
        private const int HOSPITAL_HEAL_AMOUNT = 20;
        private const int DISASTER_DMG = 20;

        private const string DECISION_FLIP_ANIMATION_NAME = "FlipCardReveal";

        [SerializeField] private Transform playerInfosContainer;
        [SerializeField] private FortuneSpinWheel fortuneWheel;
        public static PanelEffectsManager Instance { get; private set; }

        private GameManager gameManager;

        private MapManager mapManager;

        private IEntity choosedEntity = null;

        private int? choosedOption = null;

        private List<OptionCard.DecisionOptions> availablePlayerDecisionOptions;

        private Coroutine waitForPlayer;

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
            gameManager = GameManager.Instance;
            mapManager = MapManager.Instance;
        }

        public void PlayerActionEnd()
        {
            // код перевірки, чи дійсно гравець з тим айді просить закінчити свою дію
            StopWaiting();
        }

        public void DecisionOptionChoose(int option)
        {
            ChooseDecisionOption(option);
        }

        public void PlayerChoose(IEntity choosedEntity)
        {
            ChooseEntity(choosedEntity);
        }

        private void ActivateFortuneWheel(int predefinedReward)
        {
            fortuneWheel.SetReward(predefinedReward);
            fortuneWheel.StartSpin();
        }

        private void ActivateChoosing(ulong[] exceptionsId = null) // треба розробити нормальний вивід противників
        {
            Debug.Log("Choosing activated");
            /*foreach (Transform playerInfo in playerInfosContainer)
            {
                var info = playerInfo.GetComponent<TestPlayerInfoHandler>();

                if (exceptionsId != null && exceptionsId.Contains(info.PlayerId))
                    continue;

                info.ActivateButton();
            }*/
        }

        private void AddChoiceForDecision(int option) =>
            OptionsPickerController.Instance.AddChoiceForDesicion(option);

        private void EnablePicker() =>
            StartCoroutine(OptionsPickerController.Instance.EnablePicker());

        private void DecisionCardSetRequirement(int option, int requirement)
        {
            var foundCard = OptionsPickerController.Instance.FindOptionAndSetRequirement(
                (OptionCard.DecisionOptions)option, (OptionCard.DesicionRequirements)requirement);

            StartCoroutine(DecisionCardSetRequirement(foundCard));
        }

        private void EnableBettingPicker()
        {
            BettingManager.Instance.EnablePicker();
        }

        private IEnumerator DecisionCardSetRequirement(OptionCard foundCard)
        {
            foundCard.FlipCard();

            yield return null;

            while (foundCard.GetAnimatorState.IsName(DECISION_FLIP_ANIMATION_NAME))
            {
                yield return null;
            }

            StartCoroutine(OptionsPickerController.Instance.DisablePicker());
        }
        private IEnumerator WaitForPlayer(PanelEffect panelEffect)
        {
            while (true)
            {
                // !інформацію про гравця можеш передавати з загального свіч-кейсу(playerInit)
                // коли таймер спливає, то можна виповнити якийсь код, відповідний панелі(через свіч-кейс), якщо гравець нічого не зробив по сплину таймера
                UnityEngine.Debug.Log("Waiting for player action end...");
                yield return null;
            }
        }

        private IEnumerator OnPayoffActivation(IEntity entityInit)
        {
            var randomPredefinedReward = UnityEngine.Random.Range(0, fortuneWheel.m_RewardData.Length);
            var randomRewardData = fortuneWheel.m_RewardData[randomPredefinedReward];

            Debug.Log($"Random reward is {randomRewardData.m_Type} and value is {randomRewardData.m_Count}");

            ActivateFortuneWheel(randomPredefinedReward);

            yield return new WaitForSeconds(2);

            HandleReward(entityInit, randomRewardData);
        }

        #region Звичайні функції

        /*private void OnShopPanelActivation(ulong playerInitId) // Реалізувати коли будуть готові карти з ефектами
        {

        }*/

        public void StopWaiting()
        {
            if (waitForPlayer == null)
                return;

            StopCoroutine(waitForPlayer);
            waitForPlayer = null;
        }

        private void HandleReward(IEntity playerInitId, RewardData reward)
        {
            switch (reward.m_Type)
            {
                case "money":
                    Debug.Log($"Adding money({reward.m_Count})  for player...");
                    break;

                case "chips":
                    Debug.Log($"Adding chips({reward.m_Count}) for player...");
                    break;

            }
        }

        private void OnDecisionPanelActivation(IEntity entityInit)
        {
            List<OptionCard.DecisionOptions> alreadyPickedOptions = new List<OptionCard.DecisionOptions>();
            var options = Enum.GetValues(typeof(OptionCard.DecisionOptions));

            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:

                    for (int i = 0; i < DESICIONS_COUNT; i++)
                    {
                        int randomOption;
                        do
                        {
                            randomOption = (int)options.GetValue(UnityEngine.Random.Range(0, options.Length));
                        }
                        while (alreadyPickedOptions.Contains((OptionCard.DecisionOptions)randomOption));

                        alreadyPickedOptions.Add((OptionCard.DecisionOptions)randomOption);

                        AddChoiceForDecision(randomOption);
                    }

                    EnablePicker();

                    break;

                case EntityType.Enemy:

                    for (int i = 0; i < DESICIONS_COUNT; i++)
                    {
                        int randomOption;
                        do
                        {
                            randomOption = (int)options.GetValue(UnityEngine.Random.Range(0, options.Length));
                        }
                        while (alreadyPickedOptions.Contains((OptionCard.DecisionOptions)randomOption));

                        alreadyPickedOptions.Add((OptionCard.DecisionOptions)randomOption);
                    }

                    choosedOption = (int)alreadyPickedOptions[Random.Range(0, alreadyPickedOptions.Count)];

                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }
        public void ChooseEntity(IEntity entity) => choosedEntity = entity;

        public void ChooseDecisionOption(int option)
        {
            if (!availablePlayerDecisionOptions.Contains((OptionCard.DecisionOptions)option))
            {
                Debug.LogWarning($"Decison {(OptionCard.DecisionOptions)option} cannot be picked!");
                return;
            }

            Debug.Log($"Choosed option from client: {((OptionCard.DecisionOptions)option).ToString()}");

            choosedOption = option;
        }

        private void OnBettingPanelActivation(IEntity playerInitId, PanelEffect panelEffect)
        {
            waitForPlayer = StartCoroutine(WaitForPlayer(panelEffect));
        }

        private void OnHospitalActivation(IEntity entityInit)
        {
            gameManager.Heal(entityInit, HOSPITAL_HEAL_AMOUNT, true);
        }

        private void OnDisasterActivation(IEntity entityInit)
        {
            gameManager.DealDamage(entityInit, DISASTER_DMG, false);
        }

        private void PursuitPanelHandler(IEntity entityInit)
        {
            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                    ActivateChoosing();
                    break;

                case EntityType.Enemy:
                    choosedEntity = GameManager.Instance.GetEntityWithType(EntityType.Player);
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }

        private void OnPursuitActivation(IEntity chasedEntity, IEntity EntityInit) // логіка для панелі "Натиск"
        {
            this.choosedEntity = null;

            if (chasedEntity == null)
            {
                Debug.Log("chasedPlayerIdERROR");
                return;
            }

            if (chasedEntity == EntityInit)
            {
                Debug.Log("SkipChasing");
                return;
            }

            EntityInit.EnableAttacking();

            ((MonoBehaviour)EntityInit).gameObject.transform.position = ((MonoBehaviour)chasedEntity).transform.position;
        }

        private void BackstabHandler(IEntity entityInit)
        {
            switch (entityInit.GetEntityType)
            {
                case EntityType.Player:
                    ActivateChoosing();
                    break;

                case EntityType.Enemy:
                    choosedEntity = GameManager.Instance.GetEntityWithType(EntityType.Player);
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }
        }

        private void OnBackStabActivation(IEntity choosedEntity) // логіка для панелі "Ніж у спину"
        {
            this.choosedEntity = null;

            if (choosedEntity == null)
            {
                Debug.Log("chasedPlayerIdERROR");
                return;
            }

            if (gameManager == null)
            {
                Debug.LogWarning("gameManager IS NULL!!!");
                return;
            }

            gameManager.DealDamage(choosedEntity, BACKSTABDMG);
        }

        private void OnRechargeActivation(IEntity entityInit)
        {
            mapManager.MakeADraw(entityInit);
        }

        private void OnPortalPanelActivation(IEntity entityInit, PanelEffect panelEffect) // логіка для панелі "Портал"
        {
            var portals = mapManager.GetAllPanelsOfType(panelEffect);
            portals.RemoveAll(panel => panel == entityInit.GetCurrentPanel);

            gameManager.TeleportEntity(portals[UnityEngine.Random.Range(0, portals.Count)].transform.position, entityInit);
            Debug.Log("PanelEffectManager: Portal panel teleported player");
        }

        private void DecisionActivation(int? choosedOption, IEntity entityInit)
        {
            var option = (OptionCard.DecisionOptions)choosedOption;

            var requirements = Enum.GetValues(typeof(OptionCard.DesicionRequirements));
            var randomRequirement = (OptionCard.DesicionRequirements)requirements.GetValue(UnityEngine.Random.Range(0, requirements.Length));

            var entities = gameManager.GetEntitiesList();

            switch (randomRequirement)
            {
                case OptionCard.DesicionRequirements.None:
                    break;

                case OptionCard.DesicionRequirements.MostStars:

                    Debug.Log("MostStarsRequirementPocessing...");

                    Debug.Log("Entities with mostStars: ");
                    Debug.Log("Cannot get stars from the Entities-_-");

                    break;

                case OptionCard.DesicionRequirements.LeastStars:
                    Debug.Log("LeastStarsRequirementPocessing...");

                    Debug.Log("Entities with leastStars: ");
                    Debug.Log("Cannot get stars from the Entities-_-");
                    break;

                case OptionCard.DesicionRequirements.MostHealth:

                    Debug.Log("MostHealthRequirementPocessing...");
                    int maxHealth = entities.Max(entity => entity.GetEntityHp);
                    var entitiesWithMostHealth = entities.Where(entity => entity.GetEntityMoney == maxHealth).ToList();

                    Debug.Log("Entities with mostHealth: ");
                    foreach (var player in entitiesWithMostHealth) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.GetEntityName}");
                    }

                    break;

                case OptionCard.DesicionRequirements.LeastHealth:

                    Debug.Log("LeastHealthRequirementPocessing...");
                    int minHealth = entities.Min(player => player.GetEntityHp);
                    var playersWithLeastHealth = entities.Where(player => player.GetEntityMoney == minHealth).ToList();

                    Debug.Log("Entity with leastHealth: ");
                    foreach (var player in playersWithLeastHealth) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.GetEntityName}");
                    }

                    break;

                case OptionCard.DesicionRequirements.MostMoney:

                    Debug.Log("MostMoneyRequirementPocessing...");
                    int maxMoney = entities.Max(player => player.GetEntityMoney);
                    var playersWithMostMoney = entities.Where(player => player.GetEntityMoney == maxMoney).ToList();

                    Debug.Log("Players with mostMoney: ");
                    foreach (var player in playersWithMostMoney) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.GetEntityName}");
                    }

                    break;

                case OptionCard.DesicionRequirements.LeastMoney:

                    Debug.Log("LeastMoneyRequirementPocessing...");
                    int minMoney = entities.Min(player => player.GetEntityMoney);
                    var playersWithLeastMoney = entities.Where(player => player.GetEntityMoney == minMoney).ToList();

                    Debug.Log("Players with leastMoney: ");
                    foreach (var player in playersWithLeastMoney) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.GetEntityName}");
                    }

                    break;

                case OptionCard.DesicionRequirements.MostChips: // маніпуляції з фішками не підходять для одиночної гри

                    /*Debug.Log("MostChipsRequirementPocessing...");
                    int maxChips = entities.Max(player => player.GetEn);
                    var playersWithMostChips = entities.Where(player => player.GetEntityMoney == maxChips).ToList();

                    Debug.Log("Players with mostChips: ");
                    foreach (var player in playersWithMostChips) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.name}");
                    }
                    break;*/

                case OptionCard.DesicionRequirements.LeastChips: // маніпуляції з фішками не підходять для одиночної гри

                    /*Debug.Log("LeastChipsRequirementPocessing...");
                    int minChips = entities.Min(player => player.GetEntityChips);
                    var playersWithLeastChips = entities.Where(player => player.GetEntityMoney == minChips).ToList();

                    Debug.Log("Players with leastChips: ");
                    foreach (var player in playersWithLeastChips) // дебаг інфа
                    {
                        Debug.Log($"Entity with name: {player.name}");
                    }
                    break;*/

                case OptionCard.DesicionRequirements.MostCards:

                    Debug.Log("MostCardsRequirementPocessing...");
                    Debug.Log("Players with MostCards: ");
                    Debug.Log("Cannot get cards from the players-_-");
                    break;

                case OptionCard.DesicionRequirements.LeastCards:
                    Debug.Log("LeastCardsRequirementPocessing...");
                    Debug.Log("Players with LeastCards: ");
                    Debug.Log("Cannot get cards from the players-_-");
                    break;

                default:
                    break;
            }

            DecisionCardSetRequirement((int)option, (int)randomRequirement);

            switch (option)
            {
                case OptionCard.DecisionOptions.None:
                    break;

                case OptionCard.DecisionOptions.AddMoney:
                    Debug.Log("AddMoney");
                    break;

                case OptionCard.DecisionOptions.RemoveMoney:
                    Debug.Log("RemoveMoney");
                    break;

                case OptionCard.DecisionOptions.AddChips:
                    Debug.Log("AddChips");
                    break;

                case OptionCard.DecisionOptions.RemoveChips:
                    Debug.Log("RemoveChips");
                    break;

                case OptionCard.DecisionOptions.Heal:
                    Debug.Log("Heal");
                    break;

                case OptionCard.DecisionOptions.GetDamage:
                    Debug.Log("GetDamage");
                    break;

                case OptionCard.DecisionOptions.AddCards:
                    Debug.Log("AddCards");
                    break;

                case OptionCard.DecisionOptions.RemoveCards:
                    Debug.Log("RemoveCards");
                    break;

                default:
                    break;
            }
        }

        #endregion

        public IEnumerator TriggerPanelEffect(PanelEffect panelEffect, IEntity entityInit)
        {
            switch (panelEffect)
            {
                case PanelEffect.None:
                    Debug.Log("EmptyPanel");
                    break;

                case PanelEffect.Portal:
                    OnPortalPanelActivation(entityInit, panelEffect);
                    break;

                case PanelEffect.Pursuit:

                    PursuitPanelHandler(entityInit);

                    while (choosedEntity == null)
                        yield return null;

                    OnPursuitActivation(choosedEntity, entityInit);

                    break;

                case PanelEffect.Decision:

                    OnDecisionPanelActivation(entityInit);

                    while (choosedOption == null)
                        yield return null;

                    Debug.Log("Predecision debug log!");
                    DecisionActivation(choosedOption, entityInit);
                    Debug.Log("Decision panel activation end...");
                    break;

                case PanelEffect.Shop:
                    // Реалізувати коли будуть готові карти з ефектами
                    break;

                case PanelEffect.Event:
                    Debug.Log("Event panel activation!");
                    GlobalEventsManager.Instance.TriggerGlobalEvent(entityInit);
                    Debug.Log("Event panel activation end...");
                    break;

                case PanelEffect.Betting:
                    OnBettingPanelActivation(entityInit, panelEffect);

                    while (waitForPlayer != null)
                        yield return null;

                    Debug.Log("Betting panel processing end!");

                    break;

                case PanelEffect.Backstab:
                    Debug.Log("BackstabPanel");

                    BackstabHandler(entityInit);

                    while (choosedEntity == null)
                        yield return null;

                    OnBackStabActivation(choosedEntity);

                    break;

                case PanelEffect.Recharge:

                    OnRechargeActivation(entityInit);

                    yield break;

                case PanelEffect.Hospital:

                    Debug.Log("Hospital Panel");

                    OnHospitalActivation(entityInit);

                    break;

                case PanelEffect.Dealing:

                    // Зробити реалізацію даної панелі, після того, як ефектні карти будуть до кінця реалізовані

                    break;

                case PanelEffect.Payoff:

                    Debug.Log("Payoff Panel");

                    yield return StartCoroutine(OnPayoffActivation(entityInit));

                    break;

                case PanelEffect.Fate:

                    Debug.Log("Fate Panel");
                    GlobalEventsManager.Instance.TriggerFateEvent(entityInit);
                    Debug.Log("Fate panel activation end...");

                    break;

                case PanelEffect.Disaster:

                    Debug.Log("Disaster Panel");

                    OnDisasterActivation(entityInit);

                    break;

                case PanelEffect.Casino:
                    break;

                case PanelEffect.IllegalCasino:
                    break;

                case PanelEffect.Spawn:
                    break;

                default:
                    Debug.Log("EmpyPanel");
                    break;
            }

            MapManager.Instance.TempResetMapValuesInfo();
            TurnManager.Instance.EndTurnRequest(entityInit);
            // Код для початку ходу наступного гравця

        }
    }
}