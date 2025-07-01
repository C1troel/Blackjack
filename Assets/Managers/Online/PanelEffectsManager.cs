using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using Panel;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using JSG.FortuneSpinWheel;
using Multiplayer.Panel;

namespace Multiplayer
{
    public class PanelEffectsManager : NetworkBehaviour
    {
        private const int BACKSTABDMG = 20;
        private const int DESICIONS_COUNT = 2;
        private const int HOSPITAL_HEAL_AMOUNT = 20;
        private const int DISASTER_DMG = 20;

        private const string DECISION_FLIP_ANIMATION_NAME = "FlipCardReveal";

        [SerializeField] private Transform playerInfosContainer;
        [SerializeField] private FortuneSpinWheel fortuneWheel;
        public static PanelEffectsManager Instance { get; private set; }

        private TestPlayerSpawner gameManager;

        private MapManager mapManager;

        private ulong? choosedPlayerId = null;

        private int? choosedOption = null;

        private List<OptionCard.DecisionOptions> availablePlayerDecisionOptions;

        private Coroutine waitForPlayer;

        private void Start()
        {
        }

        #region Стандартні мережеві функції
        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            gameManager = TestPlayerSpawner.Instance;
            mapManager = MapManager.Instance;
        }
        #endregion

        #region ServerRpc

        [ServerRpc(RequireOwnership = false)]
        public void RequestForPlayerActionEndServerRpc(ServerRpcParams rpcParams = default)
        {
            // код перевірки, чи дійсно гравець з тим айді просить закінчити свою дію
            StopWaiting();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestForDecisionOptionChooseServerRpc(int option)
        {
            ChooseDecisionOption(option);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestForPlayerChooseServerRpc(ulong choosedPlayerId)
        {
            ChoosePlayer(choosedPlayerId);
        }
        #endregion

        #region ClientRpc

        [ClientRpc]
        private void ActivateFortuneWheelClientRpc(int predefinedReward)
        {
            fortuneWheel.SetReward(predefinedReward);
            fortuneWheel.StartSpin();
        }

        [ClientRpc]
        private void ActivateChoosingClientRpc(ulong[] exceptionsId = null, ClientRpcParams clientRpcParams = default)
        {
            foreach (Transform playerInfo in playerInfosContainer)
            {
                var info = playerInfo.GetComponent<TestPlayerInfoHandler>();

                if (exceptionsId != null && exceptionsId.Contains(info.PlayerId))
                    continue;

                info.ActivateButton();
            }
        }

        [ClientRpc]
        private void AddChoiceForDecisionClientRpc(int option, ClientRpcParams clientRpcParams = default) =>
            OptionsPickerController.Instance.AddChoiceForDesicion(option);

        [ClientRpc]
        private void EnablePickerClientRpc(ClientRpcParams clientRpcParams = default) =>
            StartCoroutine(OptionsPickerController.Instance.EnablePicker());

        [ClientRpc]
        private void DecisionCardSetRequirementClientRpc(int option, int requirement, ClientRpcParams clientRpcParams = default)
        {
            var foundCard = OptionsPickerController.Instance.FindOptionAndSetRequirement(
                (OptionCard.DecisionOptions)option, (OptionCard.DesicionRequirements)requirement);

            StartCoroutine(DecisionCardSetRequirement(foundCard));
        }

        [ClientRpc]
        private void EnableBettingPickerClientRpc(ClientRpcParams clientRpcParams = default)
        {
            BettingManager.Instance.EnablePicker();
        }

        #endregion

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

        private IEnumerator OnPayoffActivation(ulong playerInitId)
        {
            var randomPredefinedReward = UnityEngine.Random.Range(0, fortuneWheel.m_RewardData.Length);
            var randomRewardData = fortuneWheel.m_RewardData[randomPredefinedReward];

            Debug.Log($"Random reward is {randomRewardData.m_Type} and value is {randomRewardData.m_Count}");

            ActivateFortuneWheelClientRpc(randomPredefinedReward);

            yield return new WaitForSeconds(2);

            HandleRewardForPlayer(playerInitId, randomRewardData);
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

        private void HandleRewardForPlayer(ulong playerInitId, RewardData reward)
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

        private void OnDecisionPanelActivation(ulong playerInitId)
        {
            List<OptionCard.DecisionOptions> alreadyPickedOptions = new List<OptionCard.DecisionOptions>();
            var options = Enum.GetValues(typeof(OptionCard.DecisionOptions));

            var targetClient = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerInitId }
                }
            };

            for (int i = 0; i < DESICIONS_COUNT; i++)
            {
                int randomOption;
                do
                {
                    randomOption = (int)options.GetValue(UnityEngine.Random.Range(0, options.Length));
                }
                while (alreadyPickedOptions.Contains((OptionCard.DecisionOptions)randomOption));

                alreadyPickedOptions.Add((OptionCard.DecisionOptions)randomOption);

                AddChoiceForDecisionClientRpc(randomOption, targetClient);
            }

            availablePlayerDecisionOptions = alreadyPickedOptions;

            EnablePickerClientRpc(targetClient);
        }
        public void ChoosePlayer(ulong playerId) => choosedPlayerId = playerId;

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

        private void OnBettingPanelActivation(ulong playerInitId, PanelEffect panelEffect)
        {
            EnableBettingPickerClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerInitId }
                }
            });

            waitForPlayer = StartCoroutine(WaitForPlayer(panelEffect));
        }

        private void OnHospitalActivation(ulong playerInitId)
        {
            var playerInit = gameManager.GetPlayerWithId(playerInitId);

            gameManager.Heal(playerInit, HOSPITAL_HEAL_AMOUNT, true);
        }

        private void OnDisasterActivation(ulong playerInitId)
        {
            var playerInit = gameManager.GetPlayerWithId(playerInitId);
            gameManager.DealDamage(playerInit, DISASTER_DMG, false);
        }

        private void OnPursuitActivation(ulong? chasedPlayerId, ulong playerInitId) // логіка для панелі "Натиск"
        {
            this.choosedPlayerId = null;

            if (!IsServer)
                Debug.LogWarning("NONSERVERCALL");

            if (chasedPlayerId == null)
            {
                Debug.Log("chasedPlayerIdERROR");
                return;
            }

            if (chasedPlayerId == playerInitId)
            {
                Debug.Log("SkipChasing");
                return;
            }

            var playerInit = TestPlayerSpawner.Instance.GetPlayerWithId(playerInitId);
            var chasedPlayer = TestPlayerSpawner.Instance.GetPlayerWithId(chasedPlayerId ?? playerInitId);
            playerInit.EnableAttacking();

            playerInit.gameObject.transform.position = chasedPlayer.transform.position;
        }

        private void OnBackStabActivation(ulong? choosedPlayerId) // логіка для панелі "Ніж у спину"
        {
            if (!IsServer)
                return;

            this.choosedPlayerId = null;

            if (choosedPlayerId == null)
            {
                Debug.Log("chasedPlayerIdERROR");
                return;
            }

            if (gameManager == null)
            {
                Debug.LogWarning("gameManager IS NULL!!!");
                return;
            }

            gameManager.DealDamage(gameManager.GetPlayerWithId((ulong)choosedPlayerId), BACKSTABDMG);
        }

        private void OnRechargeActivation(ulong playerInitId)
        {
            var playerInit = gameManager.GetPlayerWithId(playerInitId);

            mapManager.MakeADraw(playerInit);
        }

        private void OnPortalPanelActivation(ulong playerInitId, PanelEffect panelEffect) // логіка для панелі "Портал"
        {
            var playerInit = gameManager.GetPlayerWithId((ulong)playerInitId);

            var portals = mapManager.GetAllPanelsOfType(panelEffect);
            portals.RemoveAll(panel => panel == playerInit.GetCurrentPanel);

            gameManager.TeleportPlayer(portals[UnityEngine.Random.Range(0, portals.Count)].transform.position, playerInitId);
            Debug.Log("PanelEffectManager: Portal panel teleported player");
        }

        private void DecisionActivation(int? choosedOption, ulong playerInitId)
        {
            var option = (OptionCard.DecisionOptions)choosedOption;

            var requirements = Enum.GetValues(typeof(OptionCard.DesicionRequirements));
            var randomRequirement = (OptionCard.DesicionRequirements)requirements.GetValue(UnityEngine.Random.Range(0, requirements.Length));

            var players = gameManager.GetPlayersList();

            switch (randomRequirement)
            {
                case OptionCard.DesicionRequirements.None:
                    break;

                case OptionCard.DesicionRequirements.MostStars:

                    Debug.Log("MostStarsRequirementPocessing...");

                    Debug.Log("Players with mostStars: ");
                    Debug.Log("Cannot get stars from the players-_-");

                    break;

                case OptionCard.DesicionRequirements.LeastStars:
                    Debug.Log("LeastStarsRequirementPocessing...");

                    Debug.Log("Players with leastStars: ");
                    Debug.Log("Cannot get stars from the players-_-");
                    break;

                case OptionCard.DesicionRequirements.MostHealth:

                    Debug.Log("MostHealthRequirementPocessing...");
                    int maxHealth = players.Max(player => player.GetPlayerHp);
                    var playersWithMostHealth = players.Where(player => player.GetPlayerMoney == maxHealth).ToList();

                    Debug.Log("Players with mostHealth: ");
                    foreach (var player in playersWithMostHealth) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }

                    break;

                case OptionCard.DesicionRequirements.LeastHealth:

                    Debug.Log("LeastHealthRequirementPocessing...");
                    int minHealth = players.Min(player => player.GetPlayerHp);
                    var playersWithLeastHealth = players.Where(player => player.GetPlayerMoney == minHealth).ToList();

                    Debug.Log("Players with leastHealth: ");
                    foreach (var player in playersWithLeastHealth) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }

                    break;

                case OptionCard.DesicionRequirements.MostMoney:

                    Debug.Log("MostMoneyRequirementPocessing...");
                    int maxMoney = players.Max(player => player.GetPlayerMoney);
                    var playersWithMostMoney = players.Where(player => player.GetPlayerMoney == maxMoney).ToList();

                    Debug.Log("Players with mostMoney: ");
                    foreach (var player in playersWithMostMoney) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }

                    break;

                case OptionCard.DesicionRequirements.LeastMoney:

                    Debug.Log("LeastMoneyRequirementPocessing...");
                    int minMoney = players.Min(player => player.GetPlayerMoney);
                    var playersWithLeastMoney = players.Where(player => player.GetPlayerMoney == minMoney).ToList();

                    Debug.Log("Players with leastMoney: ");
                    foreach (var player in playersWithLeastMoney) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }

                    break;

                case OptionCard.DesicionRequirements.MostChips:

                    Debug.Log("MostChipsRequirementPocessing...");
                    int maxChips = players.Max(player => player.GetPlayerChips);
                    var playersWithMostChips = players.Where(player => player.GetPlayerMoney == maxChips).ToList();

                    Debug.Log("Players with mostChips: ");
                    foreach (var player in playersWithMostChips) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }
                    break;

                case OptionCard.DesicionRequirements.LeastChips:

                    Debug.Log("LeastChipsRequirementPocessing...");
                    int minChips = players.Min(player => player.GetPlayerChips);
                    var playersWithLeastChips = players.Where(player => player.GetPlayerMoney == minChips).ToList();

                    Debug.Log("Players with leastChips: ");
                    foreach (var player in playersWithLeastChips) // дебаг інфа
                    {
                        Debug.Log($"Player with ID: {player.GetPlayerId}");
                    }
                    break;

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

            DecisionCardSetRequirementClientRpc((int)option, (int)randomRequirement, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerInitId }
                }
            });

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

        public IEnumerator TriggerPanelEffect(PanelEffect panelEffect, ulong playerInitId)
        {
            switch (panelEffect)
            {
                case Panel.PanelEffect.None:
                    Debug.Log("EmptyPanel");
                    break;

                case Panel.PanelEffect.Portal:
                    OnPortalPanelActivation(playerInitId, panelEffect);
                    break;

                case Panel.PanelEffect.Pursuit:

                    ActivateChoosingClientRpc(null, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { playerInitId }
                        }
                    });

                    while (choosedPlayerId == null)
                        yield return null;

                    OnPursuitActivation(choosedPlayerId, playerInitId);

                    break;

                case Panel.PanelEffect.Decision:

                    OnDecisionPanelActivation(playerInitId);

                    while (choosedOption == null)
                        yield return null;

                    Debug.Log("Predecision debug log!");
                    DecisionActivation(choosedOption, playerInitId);
                    Debug.Log("Decision panel activation end...");
                    break;

                case Panel.PanelEffect.Shop:
                    // Реалізувати коли будуть готові карти з ефектами
                    break;

                case Panel.PanelEffect.Event:
                    Debug.Log("Event panel activation!");
                    GlobalEventsManager.Instance.TriggerGlobalEvent(playerInitId);
                    Debug.Log("Event panel activation end...");
                    break;

                case Panel.PanelEffect.Betting:
                    OnBettingPanelActivation(playerInitId, panelEffect);

                    while (waitForPlayer != null)
                        yield return null;

                    Debug.Log("Betting panel processing end!");

                    break;

                case Panel.PanelEffect.Backstab:
                    Debug.Log("BackstabPanel");

                    ActivateChoosingClientRpc(new ulong[] { playerInitId }, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { playerInitId }
                        }
                    });

                    while (choosedPlayerId == null)
                        yield return null;

                    OnBackStabActivation(choosedPlayerId);

                    break;

                case Panel.PanelEffect.Recharge:

                    OnRechargeActivation(playerInitId);

                    yield break;

                case Panel.PanelEffect.Hospital:

                    Debug.Log("Hospital Panel");

                    OnHospitalActivation(playerInitId);

                    break;

                case Panel.PanelEffect.Dealing:

                    // Зробити реалізацію даної панелі, після того, як ефектні карти будуть до кінця реалізовані

                    break;

                case Panel.PanelEffect.Payoff:

                    Debug.Log("Payoff Panel");

                    yield return StartCoroutine(OnPayoffActivation(playerInitId));

                    break;

                case Panel.PanelEffect.Fate:

                    Debug.Log("Fate Panel");
                    GlobalEventsManager.Instance.TriggerFateEvent(playerInitId);
                    Debug.Log("Fate panel activation end...");

                    break;

                case Panel.PanelEffect.Disaster:

                    Debug.Log("Disaster Panel");

                    OnDisasterActivation(playerInitId);

                    break;

                case Panel.PanelEffect.Casino:
                    break;

                case Panel.PanelEffect.IllegalCasino:
                    break;

                case Panel.PanelEffect.Spawn:
                    break;

                default:
                    Debug.Log("EmpyPanel");
                    break;
            }

            MapManager.Instance.TempResetMapValuesInfo();
            // Код для початку ходу наступного гравця

        }
    }
}
