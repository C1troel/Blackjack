using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Singleplayer;


    public class BettingPickerController : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup bettingPickerContainer;
        [SerializeField] private GameObject inputBlock;
        [SerializeField] private GameObject bettingCardPref;
        /*[SerializeField] private List<GameObject> possibleBets;*/

        [SerializeField] private int highestCardValue; // вказує не на очки карти, а на її номер в ресурсах
        [SerializeField] int gridStartXSpacingValue;
        [SerializeField] int grindEndXSpacingValue;
        [SerializeField] int gridStartYSpacingValue;
        [SerializeField] int grindEndYSpacingValue;

        [SerializeField] float lerpSpeed;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public IEnumerator DisablePicker()
        {
            while (Mathf.Abs(bettingPickerContainer.spacing.x - gridStartXSpacingValue) > 0.1f)
            {
                bettingPickerContainer.spacing = Vector2.Lerp(
                    bettingPickerContainer.spacing,
                    new Vector2(gridStartXSpacingValue, bettingPickerContainer.spacing.y),
                    Time.deltaTime * lerpSpeed
                );

                yield return null;
            }

            bettingPickerContainer.spacing = new Vector2(gridStartXSpacingValue, bettingPickerContainer.spacing.y);

            while (Mathf.Abs(bettingPickerContainer.spacing.y - gridStartYSpacingValue) > 0.1f)
            {
                bettingPickerContainer.spacing = Vector2.Lerp(
                    bettingPickerContainer.spacing,
                    new Vector2(bettingPickerContainer.spacing.x, gridStartYSpacingValue),
                    Time.deltaTime * lerpSpeed
                );

                yield return null;
            }

            bettingPickerContainer.spacing = new Vector2(bettingPickerContainer.spacing.x, gridStartYSpacingValue);

            bettingPickerContainer.gameObject.SetActive(false);
            inputBlock.SetActive(false);
        }

        public IEnumerator EnablePicker()
        {
            inputBlock.SetActive(true);
            bettingPickerContainer.gameObject.SetActive(true);

            while (Mathf.Abs(bettingPickerContainer.spacing.x - grindEndXSpacingValue) > 0.1f)
            {
                bettingPickerContainer.spacing = Vector2.Lerp(
                    bettingPickerContainer.spacing,
                    new Vector2(grindEndXSpacingValue, bettingPickerContainer.spacing.y),
                    Time.deltaTime * lerpSpeed
                );

                yield return null;
            }

            bettingPickerContainer.spacing = new Vector2(grindEndXSpacingValue, bettingPickerContainer.spacing.y);

            while (Mathf.Abs(bettingPickerContainer.spacing.y - grindEndYSpacingValue) > 0.1f)
            {
                bettingPickerContainer.spacing = Vector2.Lerp(
                    bettingPickerContainer.spacing,
                    new Vector2(bettingPickerContainer.spacing.x, grindEndYSpacingValue),
                    Time.deltaTime * lerpSpeed
                );

                yield return null;
            }

            bettingPickerContainer.spacing = new Vector2(bettingPickerContainer.spacing.x, grindEndYSpacingValue);
        }


        public IEnumerator SetUpBets()
        {
            BasePlayerController player = null;

            while (player == null)
            {
                player = GameManager.Instance.GetEntityWithType(EntityType.Player) as BasePlayerController;
                yield return null;
            }

            for (int i = 1; i < highestCardValue; i++)
            {
                var betCard = Instantiate(bettingCardPref, bettingPickerContainer.transform);
                betCard.GetComponent<OptionCard>().SetUpCardForBets(i, player.GetEntitySuit);
            }
        }
    }
