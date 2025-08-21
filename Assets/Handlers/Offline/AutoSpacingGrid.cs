using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AutoSpacingGrid : MonoBehaviour
    {
        private RectTransform container;

        private GridLayoutGroup grid;

        private float startSpacing;

        void Awake()
        {
            container = GetComponent<RectTransform>();
            grid = GetComponent<GridLayoutGroup>();
            startSpacing = grid.spacing.x;
        }

        void OnTransformChildrenChanged()
        {
            RecalculateSpacing();
        }

        public void RecalculateSpacing()
        {
            int cardCount = transform.childCount;
            if (cardCount <= 1) return;

            float containerWidth = container.rect.width;
            float cardWidth = grid.cellSize.x;

            float neededWidth = cardCount * cardWidth + (cardCount - 1) * startSpacing;

            float spacing = startSpacing;

            if (neededWidth > containerWidth)
            {
                spacing = (containerWidth - cardCount * cardWidth) / (cardCount - 1);
            }

            grid.spacing = new Vector2(spacing, grid.spacing.y);
        }
    }
}