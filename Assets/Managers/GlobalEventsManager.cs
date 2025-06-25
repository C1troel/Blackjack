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
        new("Equalization", true), // ĳ���� ������ ����� �� ����� �� ���� ��������
        new("Hurry", true), // ��������� ��� ������� ������� ������ 2 ����� �� ���
        new("Philantropy", true), // ����������� ������� ������ 5$
        new("Tornado", true), // �� ������ ��������� �������� �� ����� ���������
        new("Plague", false), // �� ��� ������� ����������� ����� "����" �� 3 ����
        new("Bets", true), // �� ������ ������ ������ � "�������"
        new("BadTrip", false), // �� ������ ������� ��������� 20% ����������� ����������
        new("Penalty", false), // �� ������ ��������� 1 ��������� "������ �����"
        new("Shopaholism", true), // ����������� ����� "����" "��������" �� 3 ����
        new("LineUp", true), // �� ������ �������������� �� ��������� ������� �� ��� � ���(���� ��� ������)
        new("Dealing", true), // �� ������ ��������� 5(�����) �� 1 "������ �����"
        new("Quarantine", true), // �� ������ �������������� �� ������ "�������", �� ������� ��
        new("ToxicFumes", false), // �� 6 ���������� ������� �'��������� ���, ����������� � ����� ������� ������ 20% ������'�
        new("HuntingSeason", false), // �� ���� ������� ������ �'��������� "������� �������"
        new("LeakyPockets", false), // ����� ������� ������ ����� "ĳ��� �������" �� 1 ���
        new("ArmedUp", true), // ����� ������� ������ ������� ����� �����
        new("Fortify", true), // ����� ������� ������ ������� ����� �������
        new("Shuffle",true), // ����� ������� ������ ������ "������� ����" ����������� ������
        new("Ludomania", false) // ����� ������� ��������� ������ � "��� � ��������" �� ������ 25% �� ����� ������� � $ + (�����)?, �������� �� ���������� ��� (���� �������� ���� ���� � 2�� ��� ����� �������)
    };

    private static readonly List<EventInfo> _fateEvents = new List<EventInfo>()
    {
        new("Heal", true), // ������� ������� ������� 20% ������'�
        new("SafetyPatch", true), // ������� ������ ����� "��������"
        new("+SCards", true), // ������� ������ 1 ��������� "������ �����"
        new("+Money", true), // ������� ������ 5$
        new("SProfitableExchange", true), // ������� �������� 5$ � 10(�����)
        new("+BCards", true), // ������� ������ 3 �������� "����� �����"
        new("DoubleMoney", true), // ������� ������� ��� $
        new("BProfitableExchange", true), // ������� �������� �������� ���� $ � X2 (�����)
        new("Damage", false), // ������� ������� ������ 20% �� ����. ������'�
        new("Wound", false), // ������� ������ ����� "����"
        new("-SCards", false), // ������� ������ 1 ��������� "������ �����"
        new("-Money", false), // ������� ������ 5$
        new("-BCards", false), // ������� ������ 3 �������� "����� �����"
        new("HalfCutMoney", false) // ������� ������ �������� $
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

    private void OnTurnStart() // ����� �� ������� ������� ����
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
                // �������� ��� "Equalization"
                Debug.Log("Equalization Processing...");
                break;

            case "Hurry":
                // �������� ��� "Hurry"
                Debug.Log("Hurry Processing...");
                break;

            case "Philantropy":
                // �������� ��� "Philantropy"
                Debug.Log("Philantropy Processing...");
                break;

            case "Tornado":
                // �������� ��� "Tornado"
                Debug.Log("Tornado Processing...");
                break;

            case "Plague":
                // �������� ��� "Plague"
                Debug.Log("Plague Processing...");
                break;

            case "Bets":
                // �������� ��� "Bets"
                Debug.Log("Bets Processing...");
                break;

            case "BadTrip":
                // �������� ��� "BadTrip"
                Debug.Log("BadTrip Processing...");
                break;

            case "Penalty":
                // �������� ��� "Penalty"
                Debug.Log("Penalty Processing...");
                break;

            case "Shopaholism":
                // �������� ��� "Shopaholism"
                Debug.Log("Shopaholism Processing...");
                break;

            case "LineUp":
                // �������� ��� "LineUp"
                Debug.Log("LineUp Processing...");
                break;

            case "Dealing":
                // �������� ��� "Dealing"
                Debug.Log("Dealing Processing...");
                break;

            case "Quarantine":
                // �������� ��� "Quarantine"
                Debug.Log("Quarantine Processing...");
                break;

            case "ToxicFumes":
                // �������� ��� "ToxicFumes"
                Debug.Log("ToxicFumes Processing...");
                break;

            case "HuntingSeason":
                // �������� ��� "HuntingSeason"
                Debug.Log("HuntingSeason Processing...");
                break;

            case "LeakyPockets":
                // �������� ��� "LeakyPockets"
                Debug.Log("LeakyPockets Processing...");
                break;

            case "ArmedUp":
                // �������� ��� "ArmedUp"
                Debug.Log("ArmedUp Processing...");
                break;

            case "Fortify":
                // �������� ��� "Fortify"
                Debug.Log("Fortify Processing...");
                break;

            case "Ludomania":
                // �������� ��� "Ludomania"
                Debug.Log("Ludomania Processing...");
                break;

            default:
                // �������� �� ���������, ���� �� ������� ����������
                break;
        }

    }

    public void TriggerFateEvent(ulong playerInit)
    {
        var fateEventName = _fateEvents[UnityEngine.Random.Range(0, _fateEvents.Count)].GetEventName;

        switch (fateEventName)
        {
            case "Heal":
                // ������� ������� ������� 20% ������'�
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "SafetyPatch":
                // ������� ������ ����� "��������"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+SCards":
                // ������� ������ 1 ��������� "������ �����"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+Money":
                // ������� ������ 5$
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "SProfitableExchange":
                // ������� �������� 5$ � 10 (�����)
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "+BCards":
                // ������� ������ 3 �������� "����� �����"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "DoubleMoney":
                // ������� ������� ��� $
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "BProfitableExchange":
                // ������� �������� �������� ���� $ � X2 (�����)
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "Damage":
                // ������� ������� ������ 20% �� ����. ������'�
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "Wound":
                // ������� ������ ����� "����"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-SCards":
                // ������� ������ 1 ��������� "������ �����"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-Money":
                // ������� ������ 5$
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "-BCards":
                // ������� ������ 3 �������� "����� �����"
                Debug.Log($"{fateEventName} fateEvent is triggered for player {playerInit}");
                break;

            case "HalfCutMoney":
                // ������� ������ �������� $
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
