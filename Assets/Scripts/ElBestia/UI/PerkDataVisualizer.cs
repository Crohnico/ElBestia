using System.Collections.Generic;
using ElBestia.Menu;
using ElBestia.Perks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class PerkDataVisualizer : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isHover;

        [Header("Preview")]
        public string previewName;
        public PerkRarity previewRarity;

        [Header("References")]
        public TMP_Text perkName;
        public Image background;
        public RectTransform widthTarget;

        [Header("Rarity Colors")]
        public Color commonColor = new Color32(0x9A, 0x9A, 0x9A, 0xFF);
        public Color uncommonColor = new Color32(0x4E, 0xB5, 0x67, 0xFF);
        public Color rareColor = new Color32(0x4A, 0x84, 0xD8, 0xFF);
        public Color epicColor = new Color32(0xA0, 0x62, 0xD8, 0xFF);
        public Color legendaryColor = new Color32(0xD6, 0xA1, 0x32, 0xFF);

        [Header("Width")]
        public float baseWidth = 0.12f;
        public float charactersPerWidthStep = 2.5f;
        public float widthStep = 0.08f;
        public float maximumWidth = 1f;

        private readonly Dictionary<PerkRarity, Color> colorsByRarity = new Dictionary<PerkRarity, Color>();
        private PerkSO perk;
        private PerkDataInfoContainer dataContainer;

        public bool IsHover => isHover;

        private void Reset()
        {
            widthTarget = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            RebuildColorMap();
        }

        private void Awake()
        {
            RebuildColorMap();
        }

        public void Setup()
        {
            Setup(previewName, previewRarity);
        }

        public void Setup(PerkSO perkData, PerkDataInfoContainer perkDataInfoContainer)
        {
            perk = perkData;
            dataContainer = perkDataInfoContainer;
            isHover = false;

            if (perk == null)
            {
                Setup(string.Empty, PerkRarity.Common);
                return;
            }

            Setup(perk.DisplayName, perk.Rarity);
        }

        public void Setup(string displayName, PerkRarity rarity)
        {
            RebuildColorMap();

            if (perkName != null)
            {
                perkName.text = displayName;
            }

            if (background != null)
            {
                background.color = GetColor(rarity);
            }

            ApplyWidth(displayName);
        }

        public void OnHover(bool isHovering)
        {
            isHover = isHovering;

            if (!isHover || perk == null || dataContainer == null)
            {
                return;
            }

            dataContainer.gameObject.SetActive(true);
            dataContainer.SetTitle(perk.DisplayName);
            dataContainer.SetDescription(perk.Description);
        }

        public void OnClick()
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHover(false);
        }

        private void ApplyWidth(string displayName)
        {
            RectTransform target = widthTarget != null ? widthTarget : transform as RectTransform;
            if (target == null)
            {
                return;
            }

            int characterCount = string.IsNullOrEmpty(displayName) ? 0 : displayName.Length;
            float width = Mathf.Clamp(
                baseWidth + Mathf.Floor(characterCount / Mathf.Max(0.1f, charactersPerWidthStep)) * widthStep,
                baseWidth,
                maximumWidth);

            Vector2 sizeDelta = target.sizeDelta;
            sizeDelta.x = width;
            target.sizeDelta = sizeDelta;
        }

        private Color GetColor(PerkRarity rarity)
        {
            if (colorsByRarity.TryGetValue(rarity, out Color color))
            {
                return color;
            }

            return commonColor;
        }

        private void RebuildColorMap()
        {
            colorsByRarity.Clear();
            colorsByRarity[PerkRarity.Common] = commonColor;
            colorsByRarity[PerkRarity.Rare] = uncommonColor;
            colorsByRarity[PerkRarity.VeryRare] = rareColor;
            colorsByRarity[PerkRarity.Epic] = epicColor;
            colorsByRarity[PerkRarity.Legendary] = legendaryColor;
        }
    }
}
