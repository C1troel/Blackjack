using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Singleplayer
{
    public class PreviewsHandler : MonoBehaviour, IPointerClickHandler, IOutlinable
    {
        [SerializeField] private GameObject entityPreviewPrefab;

        [SerializeField] private ScrollRect entitiesListView;
        [SerializeField] private HorizontalLayoutGroup previewsContainer;
        [SerializeField] private TextMeshPro entitiesCountText;

        private List<EntityPreviewHandler> previews = new List<EntityPreviewHandler>();

        private SpriteRenderer spriteRenderer;
        private Material defaultSpriteMaterial;
        private Material outlineSpriteMaterial;

        public event Action<bool> OnOutlineChanged;

        public bool IsOutlined { get; private set; }

        public void SetupEntityPreviews(PanelScript panelOwner, List<IEntity> stayedEntities)
        {
            var mainCanvas = GetComponentInChildren<Canvas>();
            mainCanvas.worldCamera = GameManager.Instance.PlayerCamera.GetComponent<Camera>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultSpriteMaterial = spriteRenderer.material;
            outlineSpriteMaterial = EffectCardDealer.Instance.GetEffectCardOutlineMaterial; // заглушка, щоб була хоч якась обводка

            panelOwner.OnEntityAdded += OnEntityAddedToPanel;
            panelOwner.OnEntityRemoved += OnEntityRemovedFromPanel;

            entitiesCountText.text = stayedEntities.Count.ToString();

            foreach (var stayedEntity in stayedEntities)
            {
                var entityPreview = Instantiate(entityPreviewPrefab, previewsContainer.transform).GetComponent<EntityPreviewHandler>();
                entityPreview.ManageEntityPreview(stayedEntity);
                entityPreview.OnOutlineChanged += HandlePreviewsOutlineChanged;
                previews.Add(entityPreview);
            }
        }

        private void OnEntityAddedToPanel(IEntity addedEntity)
        {
            var entityPreview = Instantiate(entityPreviewPrefab, previewsContainer.transform)
                .GetComponent<EntityPreviewHandler>();

            entityPreview.ManageEntityPreview(addedEntity);
            entityPreview.OnOutlineChanged += HandlePreviewsOutlineChanged;

            previews.Add(entityPreview);

            entitiesCountText.text = previews.Count.ToString();
        }

        private void OnEntityRemovedFromPanel(IEntity removedEntity)
        {
            var previewToRemove = previews.FirstOrDefault(p => p.ManagedEntity == removedEntity);
            if (previewToRemove != null)
            {
                previewToRemove.OnOutlineChanged -= HandlePreviewsOutlineChanged;
                previewToRemove.RemovePreview();

                previews.Remove(previewToRemove);
            }

            entitiesCountText.text = previews.Count.ToString();
        }

        private void LateUpdate()
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 0.5f);
        }

        public void ToggleEntitiesList(bool isActive)
        {
            RemoveOutline();

            entitiesListView.gameObject.SetActive(isActive);

            if (!isActive)
                HandlePreviewsOutlineChanged(isActive);
        }

        void Start()
        {

        }

        void Update()
        {

        }

        private void HandlePreviewsOutlineChanged(bool isOutlined)
        {
            bool anyOutlined = previews.Any(a => a.IsOutlined);

            if (IsOutlined != anyOutlined)
            {
                if (anyOutlined)
                    SetOutline();
                else
                    RemoveOutline();
            }
        }

        public void SetOutline()
        {
            if (entitiesListView.gameObject.activeSelf)
                return;

            IsOutlined = true;
            spriteRenderer.material = outlineSpriteMaterial;
        }

        public void RemoveOutline()
        {
            IsOutlined = false;
            spriteRenderer.material = defaultSpriteMaterial;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            bool entitiesListActivity = entitiesListView.gameObject.activeSelf;
            ToggleEntitiesList(!entitiesListActivity);
        }

        public void RemoveHandler()
        {
            foreach (var preview in previews)
                preview.RemovePreview();

            Destroy(gameObject);
        }
    }
}