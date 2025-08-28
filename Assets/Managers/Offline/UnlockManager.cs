using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singleplayer
{
    public class UnlockManager : MonoBehaviour
    {
        public static UnlockManager Instance { get; private set; }

        private HashSet<EffectCardType> unlockedCards = new HashSet<EffectCardType>();

        private const string PlayerPrefsKey = "UnlockedEffectCards";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadUnlockedCards();
            UnlockDefaultCards();
        }

        private void Start()
        {
            LoadUnlockedCards();
            UnlockDefaultCards();

            EffectCardDealer.Instance.ManageUnlockedCardList();
        }

        private void UnlockDefaultCards()
        {
            var allCards = InfosLoadManager.Instance.GetAllEffectCardInfos();
            foreach (var card in allCards)
            {
                if (card.UnlockedByDefault)
                    unlockedCards.Add(card.EffectCardType);
            }

            SaveUnlockedCards();
        }

        public bool IsCardUnlocked(EffectCardType type)
        {
            return unlockedCards.Contains(type);
        }

        public void UnlockCard(EffectCardType type)
        {
            if (!unlockedCards.Contains(type))
            {
                unlockedCards.Add(type);
                SaveUnlockedCards();
            }
        }

        public List<EffectCardType> GetAllUnlockedCards()
        {
            return new List<EffectCardType>(unlockedCards);
        }

        #region Save/Load

        private void SaveUnlockedCards()
        {
            /*string data = string.Join(",", unlockedCards);
            PlayerPrefs.SetString(PlayerPrefsKey, data);
            PlayerPrefs.Save();*/
        }

        private void LoadUnlockedCards()
        {
            /*unlockedCards.Clear();

            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return;

            string data = PlayerPrefs.GetString(PlayerPrefsKey);
            if (string.IsNullOrEmpty(data))
                return;

            string[] types = data.Split(',');
            foreach (var t in types)
            {
                if (System.Enum.TryParse(t, out EffectCardType type))
                {
                    unlockedCards.Add(type);
                }
            }*/
        }

        #endregion
    }
}