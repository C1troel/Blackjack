using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class SpriteShadowHandler : MonoBehaviour
    {
        [SerializeField]private Vector2 offset = new Vector2(-3, -3);
        [SerializeField]private Material material;
        [SerializeField]private bool isInnerShadow;


        private SpriteRenderer sprRndCaster;
        private SpriteRenderer sprRndShadow;

        private Transform transCaster;
        private Transform transShadow;

        void Start()
        {
            transCaster = transform;
            transShadow = new GameObject().transform;
            transShadow.parent = transCaster;
            transShadow.gameObject.name = "shadow";

            sprRndCaster = GetComponent<SpriteRenderer>();
            sprRndShadow = transShadow.gameObject.AddComponent<SpriteRenderer>();

            sprRndShadow.material = material;
            sprRndShadow.sortingLayerName = sprRndCaster.sortingLayerName;
            sprRndShadow.sortingOrder = sprRndCaster.sortingOrder - 1;

            if (isInnerShadow)
                transShadow.localScale = new Vector3(0.95f, 0.95f, 0);
            else
                transShadow.localScale = new Vector3(1,1,0);

        }

        void LateUpdate()
        {

            transShadow.position = new Vector2(transCaster.position.x + offset.x,
            transCaster.position.y + offset.y);

            sprRndShadow.sprite = sprRndCaster.sprite;

        }
    }
}