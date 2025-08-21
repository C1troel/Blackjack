using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Singleplayer
{
    public class EntityPreviewHandler : MonoBehaviour, IPointerClickHandler, IOutlinable
    {
        [SerializeField] private TextMeshProUGUI entityHpText;
        [SerializeField] private TextMeshProUGUI entityNameText;
        [SerializeField] private Image entityAvatar;

        private Image image;
        private Material defaultSpriteMaterial;
        private Material outlineSpriteMaterial;

        public IEntity ManagedEntity {  get; private set; }

        public bool IsOutlined { get; private set; }
        public event Action<bool> OnOutlineChanged;

        public void ManageEntityPreview(IEntity entity)
        {
            image = GetComponent<Image>();
            defaultSpriteMaterial = image.material;
            outlineSpriteMaterial = EffectCardDealer.Instance.GetEffectCardOutlineMaterial; // заглушка, щоб була хоч якась обводка

            ManagedEntity = entity;
            entityHpText.text = ManagedEntity.GetEntityHp.ToString();
            entityNameText.text = ManagedEntity.GetEntityName;
            //entityAvatar.sprite = // аватар сутності буде підгружатись коли будуть налаштовані дані спрайти

            switch (ManagedEntity.GetEntityType)
            {
                case EntityType.Player:
                    var player = ManagedEntity as BasePlayerController;
                    player.HpChangeEvent += OnEntityHpChanged;
                    break;

                case EntityType.Enemy:
                    var enemy = ManagedEntity as BaseEnemy;
                    enemy.OnHpChange += OnEntityHpChanged;
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            (ManagedEntity as IOutlinable).OnOutlineChanged += OnEntityOutlineChanged;
        }

        private void OnEntityHpChanged()
        {
            entityHpText.text = ManagedEntity.GetEntityHp.ToString();
        }

        private void OnEntityOutlineChanged(bool isOutlined)
        {
            if (isOutlined)
                SetOutline();
            else
                RemoveOutline();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"{this.gameObject.name} is being clicked");

            if (!IsOutlined)
                return;

            ManagedEntity.OnEntityClick();
        }

        public void SetOutline()
        {
            IsOutlined = true;
            image.material = outlineSpriteMaterial;
            OnOutlineChanged?.Invoke(true);
        }

        public void RemoveOutline()
        {
            IsOutlined = false;
            image.material = defaultSpriteMaterial;
            OnOutlineChanged?.Invoke(false);
        }

        public void RemovePreview()
        {
            (ManagedEntity as IOutlinable).OnOutlineChanged -= OnEntityOutlineChanged;
            OnOutlineChanged = null;

            switch (ManagedEntity.GetEntityType)
            {
                case EntityType.Player:
                    var player = ManagedEntity as BasePlayerController;
                    player.HpChangeEvent -= OnEntityHpChanged;
                    break;

                case EntityType.Enemy:
                    var enemy = ManagedEntity as BaseEnemy;
                    enemy.OnHpChange -= OnEntityHpChanged;
                    break;

                case EntityType.Ally:
                    break;

                default:
                    break;
            }

            ManagedEntity = null;
            Destroy(gameObject);
        }
    }
}