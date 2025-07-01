using Singleplayer;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class OptionCard : MonoBehaviour
{
    /*private bool isFacedDown = true;*/
    private OptionCardType cardType = OptionCardType.None;
    private DecisionOptions option = DecisionOptions.None;
    private DesicionRequirements requirement = DesicionRequirements.None;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void RevealCard()
    {
        transform.GetChild(0).SetSiblingIndex(1);
        /*isFacedDown = !false;*/
    }

    public void ChooseOption()
    {
        switch (cardType)
        {
            case OptionCardType.None:
                Debug.LogWarning($"OptionCard purpose is undefined!");
                break;

            case OptionCardType.Decision:

                PanelEffectsManager.Instance.DecisionOptionChoose((int)option);
                break;

            case OptionCardType.Shop:

                break;

            case OptionCardType.Bets:

                BettingManager.Instance.DisablePicker();
                BettingManager.Instance.AddPlayerBet(GetBetFromCard());
                Destroy(gameObject);
                break;

            default:
                break;
        }
    }

    /*private void OnRevealEnd()
    {
        animator.
        isFacedDown = true;
    }*/

    public void FlipCard()
    {
        animator.SetTrigger("Flip");
    }

    #region Setters
    public void SetCardDecisionRequirement(DesicionRequirements requirement)
    {
        this.requirement = requirement;

        // Вигрузити спрайт з ім'ям requirement.ToString();
    }

    private int GetBetFromCard()
    {
        var spriteName = gameObject.transform.GetChild(1).GetComponent<Image>().sprite.name;
        int betScore = -1;

        try
        {
            betScore = int.Parse(spriteName.Substring(spriteName.Length - 2));
        }
        catch (System.Exception)
        {
            Debug.Log($"Cannot get score for card");
        }

        return betScore;
    }

    public void SetCardDecisionOption(DecisionOptions option)
    {
        this.option = option;
        cardType = OptionCardType.Decision;

        // Вигрузити спрайт с ім'ям option.ToString();
    }

    public void SetUpCardForBets(int cardValue, string playerSuit = null, string playerCardSkin = null) // playerCardSkin - мб не тут потрібно підвантажувати
    {
        RevealCard();
        cardType = OptionCardType.Bets;

        var cardSprite = SpriteLoadManager.Instance.GetBasicCardSprite($"{playerSuit + (cardValue < 10 ? $"0{cardValue}" : cardValue)}");

        if (cardSprite == null)
            cardSprite = SpriteLoadManager.Instance.GetBasicCardSprite($"{"Heart" + (cardValue < 10 ? $"0{cardValue}" : cardValue)}");

        transform.GetChild(1).GetComponent<Image>().sprite = cardSprite;
    }
    #endregion

    #region Enums
    public enum DecisionOptions
    {
        None = -1,
        AddMoney,
        RemoveMoney,
        AddChips,
        RemoveChips,
        Heal,
        GetDamage,
        AddCards,
        RemoveCards
    }

    public enum DesicionRequirements
    {
        None = -1,
        MostStars,
        LeastStars,
        MostHealth,
        LeastHealth,
        MostMoney,
        LeastMoney,
        MostChips,
        LeastChips,
        MostCards,
        LeastCards
    }

    public enum OptionCardType
    {
        None = -1,
        Decision,
        Shop,
        Bets
    }
    #endregion

    #region Getters
    public AnimatorStateInfo GetAnimatorState => animator.GetCurrentAnimatorStateInfo(0);

    public DecisionOptions GetOption => option;
    #endregion
}
