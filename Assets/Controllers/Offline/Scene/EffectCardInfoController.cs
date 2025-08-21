using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Singleplayer
{
    public class EffectCardInfoController : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private TextMeshProUGUI effectCardNameText;
        [SerializeField] private TextMeshProUGUI effectCardDescText;

        private void Start()
        {
            background.GetComponent<ClickHandler>().OnEntityClickEvent += OnBackgroundClick;
        }

        public void ShowUpEffectCardInfo(EffectCardInfo effectCard)
        {
            var effectCardName = effectCard.EffectCardType;
            //var effectCardDesc = effectCard.EffectCardInfo.

            effectCardNameText.text = effectCardName.ToString();
            //effectCardDescText.text = "";

            background.SetActive(true);
        }

        private void OnBackgroundClick()
        {
            background.SetActive(false);

            effectCardNameText.text = "";
            effectCardDescText.text = "";
        }
    }
}