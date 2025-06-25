using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;

public class GlobalEventsManager : NetworkBehaviour
{
    public static GlobalEventsManager Instance { get; private set; }

    private static readonly List<EventInfo> _events = new List<EventInfo>()
    {
        new("Equalization", true), // Ділить порівно гроші та фішки між усіма гравцями
        new("Hurry", true), // Наступний хід гравець витягне відразу 2 карти на хід
        new("Philantropy", true), // Найбагатший гравець втрачає 5$
        new("Tornado", true), // Усі гравці випадково міняються між собою позиціями
        new("Plague", false), // На усіх гравців накладається ефект "Чума" на 3 ходи
        new("Bets", true), // Усі гравці беруть участь у "Ставках"
        new("BadTrip", false), // Усі гравці негайно отримують 20% неблокуючих пошкоджень
        new("Penalty", false), // Усі гравці втрачають 1 випадкову "Ігрову карту"
        new("Shopaholism", true), // Накладається ефект "Мапи" "Шопоманія" на 3 ходи
        new("LineUp", true), // Усі гравці телепортуються на випадкову позицію на мапі в лінію(один біля одного)
        new("Dealing", true), // Усі гравці отримують 5(фішок) та 1 "Ігрову карту"
        new("Quarantine", true), // Усі гравці телепортуються на панель "Шпиталю", не задіюючи її
        new("ToxicFumes", false), // На 6 випадкових панелях з'являється газ, зупинившись в якому гравець втрачає 20% здоров'я
        new("HuntingSeason", false), // На місці кожного гравця з'являється "Штрафна виписка"
        new("LeakyPockets", false), // Кожен гравець отримує ефект "Діряві кармани" на 1 хід
        new("ArmedUp", true), // Кожен гравець отримує потрійну карту атаки
        new("Fortify", true), // Кожен гравець отримує потрійну карту захисту
        new("Shuffle",true), // Кожен гравець отримує колоду "Ігрових карт" випадкового гравця
        new("Ludomania", false) // Кожен гравець примусово вступає в "Гру в блекджек" на ставку 25% від свого балансу в $ + (фішок)?, поділивши між вигравшими куш (якщо виграшна рука буде в 2ох або більше гравців)
    };

    private static readonly List<EventInfo> _fateEvents = new List<EventInfo>()
    {
        new("Heal", true), // Гравець негайно виліковує 20% здоров'я
        new("SafetyPatch", true), // Гравець отримує ефект "Заплатка"
        new("+SCards", true), // Гравець отримує 1 випадкову "Ігрову карту"
        new("+Money", true), // Гравець отримує 5$
        new("SProfitableExchange", true), // Гравець конвертує 5$ у 10(фішок)
        new("+BCards", true), // Гравець отримує 3 випадкові "Ігрові карти"
        new("DoubleMoney", true), // Гравець подвоює свої $
        new("BProfitableExchange", true), // Гравець конвертує половину своїх $ у X2 (фішок)
        new("Damage", false), // Гравець негайно втрачає 20% від макс. здоров'я
        new("Wound", false), // Гравець отримує ефект "Рана"
        new("-SCards", false), // Гравець втрачає 1 випадкову "Ігрову карту"
        new("-Money", false), // Гравець втрачає 5$
        new("-BCards", false), // Гравець втрачає 3 випадкові "Ігрові карти"
        new("HalfCutMoney", false) // Гравець втрачає половину $
    };

    private List<EventInfo> activeEvents = new List<EventInfo>();

    public static List<EventInfo> Events
    {
        get { return _events; }
        private set { }
    }

    public static List<EventInfo> FateEvents
    {
        get { return _fateEvents; }
        private set { }
    }

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public struct EventInfo
    {
        private string eventName;
        private bool isGood;
        private int roundsLeft;

        public EventInfo(string eventName, bool isGood, int roundsLeft = 0)
        {
            this.eventName = eventName;
            this.isGood = isGood;
            this.roundsLeft = roundsLeft;
        }

        public void DecreaseEventRounds() => --roundsLeft;

        public string GetEventName => eventName;
        public bool IsGood => isGood;
        public int GetRoundsLeft => roundsLeft;
    }

    public bool CheckForActiveEvent(string eventName)
    {
        return activeEvents.Any(activeEvent => activeEvent.GetEventName == eventName);
    }

    private void OnTurnStart() // логіка на початку кожного ходу
    {

        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            activeEvents[i].DecreaseEventRounds();

            if (activeEvents[i].GetRoundsLeft == 0)
            {
                activeEvents.RemoveAt(i);
            }
        }
    }

    private void ShowUpTriggeredEventForPlayers(string eventName) => MapManager.Instance.ShowUpEventCardClientRpc(eventName);

    public void TriggerGlobalEvent(ulong playerInit, string eventName = null)
    {
        string triggeredEventName = eventName;

        if (eventName == null)
        {
            triggeredEventName = _events[UnityEngine.Random.Range(0, _events.Count)].GetEventName;
        }

        switch (triggeredEventName)
        {
            case "Equalization":
                // Действие для "Equalization"
                Debug.Log("Equalization Processing...");
                break;

            case "Hurry":
                // Действие для "Hurry"
                Debug.Log("Hurry Processing...");
                break;

            case "Philantropy":
                // Действие для "Philantropy"
                Debug.Log("Philantropy Processing...");
                break;

            case "Tornado":
                // Действие для "Tornado"
                Debug.Log("Tornado Processing...");
                break;

            case "Plague":
                // Действие для "Plague"
                Debug.Log("Plague Processing...");
                break;

            case "Bets":
                // Действие для "Bets"
                Debug.Log("Bets Processing...");
                break;

            case "BadTrip":
                // Действие для "BadTrip"
                Debug.Log("BadTrip Processing...");
                break;

            case "Penalty":
                // Действие для "Penalty"
                Debug.Log("Penalty Processing...");
                break;

            case "Shopaholism":
                // Действие для "Shopaholism"
                Debug.Log("Shopaholism Processing...");
                break;

            case "LineUp":
                // Действие для "LineUp"
                Debug.Log("LineUp Processing...");
                break;

            case "Dealing":
                // Действие для "Dealing"
                Debug.Log("Dealing Processing...");
                break;

            case "Quarantine":
                // Действие для "Quarantine"
                Debug.Log("Quarantine Processing...");
                break;

            case "ToxicFumes":
                // Действие для "ToxicFumes"
                Debug.Log("ToxicFumes Processing...");
                break;

            case "HuntingSeason":
                // Действие для "HuntingSeason"
                Debug.Log("HuntingSeason Processing...");
                break;

            case "LeakyPockets":
                // Действие для "LeakyPockets"
                Debug.Log("LeakyPockets Processing...");
                break;

            case "ArmedUp":
                // Действие для "ArmedUp"
                Debug.Log("ArmedUp Processing...");
                break;

            case "Fortify":
                // Действие для "Fortify"
                Debug.Log("Fortify Processing...");
                break;

            case "Ludomania":
                // Действие для "Ludomania"
                Debug.Log("Ludomania Processing...");
                break;

            default:
                // Действие по умолчанию, если не найдено совпадение
                break;
        }

    }

    public void TriggerFateEvent(ulong playerInit)
    {
        var fateEventName = _fateEvents[UnityEngine.Random.Range(0, _fateEvents.Count)].GetEventName;

        switch (fateEventName)
        {
            case "Heal":
                // Гравець негайно виліковує 20% здоров'я
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "SafetyPatch":
                // Гравець отримує ефект "Заплатка"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+SCards":
                // Гравець отримує 1 випадкову "Ігрову карту"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+Money":
                // Гравець отримує 5$
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "SProfitableExchange":
                // Гравець конвертує 5$ у 10 (фішок)
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+BCards":
                // Гравець отримує 3 випадкові "Ігрові карти"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "DoubleMoney":
                // Гравець подвоює свої $
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "BProfitableExchange":
                // Гравець конвертує половину своїх $ у X2 (фішок)
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "Damage":
                // Гравець негайно втрачає 20% від макс. здоров'я
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "Wound":
                // Гравець отримує ефект "Рана"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-SCards":
                // Гравець втрачає 1 випадкову "Ігрову карту"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-Money":
                // Гравець втрачає 5$
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-BCards":
                // Гравець втрачає 3 випадкові "Ігрові карти"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "HalfCutMoney":
                // Гравець втрачає половину $
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            default:
                Debug.LogWarning($"Unknown event: {fateEventName}");
                break;
        }
    }

    /*public enum GoodEvents
    {
        None = -1,
        Equalization,
        Hurry,
        Philantropy,
        Tornado,
        Bets,
        Shopaholism,
        LineUp,
        Dealing,
        Quarantine,
        ArmedUp,
        Fortify,
        Reshuffle
    }

    public enum BadEvents
    {
        None = -1,
        Plague = ,
        BadTrip,
        Penalty,
        ToxicFumes,
        HuntingSeason,
        LeakyPockets,
        Ludomania
    }*/
}
