using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class PanelFrame : MonoBehaviour
    {
        void Start()
        {
            var currentSpriteRenderer = GetComponent<SpriteRenderer>();
            var parentSpriteRenderer = GetComponentInParent<SpriteRenderer>();
            currentSpriteRenderer.sortingOrder = parentSpriteRenderer.sortingOrder + 5;
        }

        void Update()
        {

        }
    }
}