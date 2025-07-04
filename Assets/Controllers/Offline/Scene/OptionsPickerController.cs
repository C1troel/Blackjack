using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Singleplayer;

public class OptionsPickerController : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup optionsPickerContainer;
    [SerializeField] private GameObject optionCardPref;
    [SerializeField] private GameObject inputBlock;

    [SerializeField] private int gridStartSpacingValue;
    [SerializeField] private int grindEndSpacingValue;

    [SerializeField] private float lerpSpeed;

    public static OptionsPickerController Instance { get; private set; }

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public OptionCard AddChoiceForDesicion(int option)
    {
        var optionsCard = Instantiate(optionCardPref, optionsPickerContainer.transform);
        optionsCard.GetComponent<OptionCard>().SetCardDecisionOption((OptionCard.DecisionOptions)option);

        return optionsCard.GetComponent<OptionCard>();
    }

    public OptionCard FindOptionAndSetRequirement(OptionCard.DecisionOptions option, OptionCard.DesicionRequirements requirement)
    {
        foreach (Transform card in optionsPickerContainer.transform)
        {
            var optionCard = card.GetComponent<OptionCard>();

            if (optionCard.GetOption == option)
            {
                optionCard.SetCardDecisionRequirement(requirement);
                return optionCard;
            }
        }

        Debug.LogWarning($"Card in Decision with option: {option.ToString()} NOT FOUND!");
        return null;
    }

    public IEnumerator DisablePicker()
    {
        while (Mathf.Abs(optionsPickerContainer.spacing.x - gridStartSpacingValue) > 0.1f)
        {
            optionsPickerContainer.spacing = Vector2.Lerp(
                optionsPickerContainer.spacing,
                new Vector2(gridStartSpacingValue, 0),
                Time.deltaTime * lerpSpeed
            );

            yield return null;
        }

        optionsPickerContainer.spacing = new Vector2(gridStartSpacingValue, 0);

        optionsPickerContainer.gameObject.SetActive(false);
        inputBlock.SetActive(false);

        foreach (Transform option in optionsPickerContainer.transform)
            Destroy(option.gameObject);
    }

    public IEnumerator EnablePicker()
    {
        inputBlock.SetActive(true);
        optionsPickerContainer.gameObject.SetActive(true);
        

        while (Mathf.Abs(optionsPickerContainer.spacing.x - grindEndSpacingValue) > 0.1f)
        {
            optionsPickerContainer.spacing = Vector2.Lerp(
                optionsPickerContainer.spacing,
                new Vector2(grindEndSpacingValue, 0),
                Time.deltaTime * lerpSpeed
            );

            yield return null;
        }

        optionsPickerContainer.spacing = new Vector2(grindEndSpacingValue, 0);

        foreach (Transform option in optionsPickerContainer.transform)
        {
            option.GetComponent<OptionCard>().FlipCard();
        }
    }
}
