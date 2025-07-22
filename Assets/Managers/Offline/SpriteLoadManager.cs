using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class SpriteLoadManager : MonoBehaviour
    {
        [SerializeField] private string playersSpritePath;
        [SerializeField] private string basicCardsPath;
        [SerializeField] private string animatorControllersPath;
        [SerializeField] private string pathEnderSpritePath;

        public static SpriteLoadManager Instance { get; private set; }

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

        public Sprite GetPathEnderSprite()
        {
            var temp = Resources.Load<Sprite>(pathEnderSpritePath + "chip");

            if (temp == null)
            {
                Debug.LogWarning("PathEnderSprite IS NULL!!!");
            }

            return Resources.Load<Sprite>(pathEnderSpritePath + "chip");
        }

        public Sprite GetBasicCardSprite(string cardName)
        {
            var fullpath = basicCardsPath + cardName;
            var sprite = Resources.Load<Sprite>(fullpath);
            /*Debug.LogWarning($"SpriteName: {sprite}");*/

            var texture2d = Resources.Load<Texture2D>(fullpath);
            /*Debug.LogWarning($"Texture2DName: {texture2d}");*/

            return sprite;
        }

        public List<Sprite> GetAllBasicCardSprites()
        {
            var allBasicCardSprites = Resources.LoadAll<Sprite>(basicCardsPath);
            var basicCardSpritesList = new List<Sprite>(allBasicCardSprites);
            return basicCardSpritesList;
        }

        public List<Sprite> GetAllBasicCardSpritesOfSuit(string suitName)
        {
            List<Sprite> matchedSprites = new List<Sprite>();

            Sprite[] allSprites = Resources.LoadAll<Sprite>(basicCardsPath);

            if (allSprites == null || allSprites.Length == 0)
            {
                Debug.LogWarning($"No sprites found in path: {basicCardsPath}");
                return matchedSprites;
            }

            foreach (var sprite in allSprites)
            {
                if (sprite.name.StartsWith(suitName))
                {
                    matchedSprites.Add(sprite);
                }
            }

            return matchedSprites;
        }

        public RuntimeAnimatorController GetAnimatorController(string controllerName, bool isSprite = true)
        {
            if (isSprite)
                return Resources.Load<RuntimeAnimatorController>(animatorControllersPath + controllerName + "_RectT");
            else
                return Resources.Load<RuntimeAnimatorController>(animatorControllersPath + controllerName);
        }
    }
}